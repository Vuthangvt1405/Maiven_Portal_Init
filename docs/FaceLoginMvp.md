# Face Login MVP - kiến trúc, flow và cách chạy

## 1. Phạm vi

Face Login là **bonus feature của project midterm một tuần**, chạy local trên Windows và chỉ dành cho Student. Đây chưa phải hệ thống sinh trắc học production.

Nguyên tắc chính:

- Student vẫn đăng nhập bằng password bình thường.
- Đăng ký, thay hoặc xóa Face ID cần JWT và nhập lại password.
- Face Login không cần email hoặc password, nhưng mỗi lần phải gửi đúng 3 ảnh.
- Ảnh chỉ tồn tại trong lúc xử lý, không lưu lâu dài.
- SQL Server chỉ lưu embedding cùng tên và phiên bản model.
- YuNet và SFace chạy local từ file ONNX, không gọi API ngoài.
- Các ngưỡng hiện tại là giá trị development, cần benchmark lại.

## 2. Ý tưởng kiến trúc

```text
React/Swagger
     |
     v
Controller
     |
     v
Service nghiệp vụ
     |----------------------|
     v                      v
FaceRecognitionService   Repository
YuNet + SFace               |
                             v
                         SQL Server
```

Controller chỉ nhận request và trả response. Service quyết định flow. Repository đọc/ghi database. Toàn bộ xử lý ảnh nằm trong `FaceRecognitionService`.

## 3. Vai trò từng file

### Controller và DTO

| File | Tác dụng |
| --- | --- |
| [`Controllers/StudentFaceController.cs`](../Controllers/StudentFaceController.cs) | API xem trạng thái, bắt đầu đăng ký, gửi frame, xác nhận, hủy session và xóa Face ID. Chỉ role Student được gọi. |
| [`Controllers/AuthController.cs`](../Controllers/AuthController.cs) | Chứa `POST /api/auth/face-login`, endpoint public nhận đúng 3 file ảnh. |
| [`Dtos/request/FaceRegisterRequest.cs`](../Dtos/request/FaceRegisterRequest.cs) | Password nhập lại khi bắt đầu đăng ký/thay Face ID. |
| [`Dtos/request/DeleteFaceRequest.cs`](../Dtos/request/DeleteFaceRequest.cs) | Password nhập lại khi xóa Face ID. |
| [`Dtos/response/FaceRegisterResponse.cs`](../Dtos/response/FaceRegisterResponse.cs) | Trả `SessionId`, số frame cần có và thời điểm session hết hạn. |
| [`Dtos/response/FaceFrameResponse.cs`](../Dtos/response/FaceFrameResponse.cs) | Trả `Accepted`, `AcceptedCount`, `RequiredCount`, `IsComplete`. |
| [`Dtos/response/FaceStatusResponse.cs`](../Dtos/response/FaceStatusResponse.cs) | Cho biết Student đã có Face ID hay chưa. |
| [`Dtos/response/FaceVerifyResponse.cs`](../Dtos/response/FaceVerifyResponse.cs) | Xác nhận Face ID đã được lưu, kèm model và version. |

### Service

| File | Tác dụng |
| --- | --- |
| [`Services/FaceRecognitionService.cs`](../Services/FaceRecognitionService.cs) | Đọc ảnh, chạy YuNet, kiểm tra chất lượng, căn mặt, chạy SFace, chuẩn hóa embedding và tính cosine similarity. |
| [`Services/FaceRegisterSessionService.cs`](../Services/FaceRegisterSessionService.cs) | Giữ tạm embedding trong RAM. Một Student có một session, cần 3 frame, hết hạn sau 10 phút. |
| [`Services/FaceCredentialService.cs`](../Services/FaceCredentialService.cs) | Điều khiển đăng ký, thay, xem trạng thái và xóa Face ID; lấy Student từ JWT và kiểm tra lại password. |
| [`Services/AuthService.cs`](../Services/AuthService.cs) | Điều khiển Face Login, xếp hạng các khuôn mặt và dùng flow JWT hiện có khi nhận diện thành công. |

### Database và cấu hình

| File | Tác dụng |
| --- | --- |
| [`Data/Entities/FaceCredential.cs`](../Data/Entities/FaceCredential.cs) | Entity chứa `UserId`, embedding, số chiều, model name/version và thời gian xóa. |
| [`Data/Configurations/FaceCredentialConfiguration.cs`](../Data/Configurations/FaceCredentialConfiguration.cs) | Ánh xạ bảng `FACE_CREDENTIALS`, kiểu cột, khóa ngoại, check constraint và unique index. |
| [`Data/AppDbContext.cs`](../Data/AppDbContext.cs) | Khai báo `DbSet<FaceCredential>` và tự ẩn dữ liệu đã soft-delete. |
| [`Repository/FaceCredentialRepository.cs`](../Repository/FaceCredentialRepository.cs) | Tìm Face ID đang hoạt động, lấy danh sách để so sánh, thay Face ID bằng transaction và soft-delete. |
| [`Repository/AuthRepository.cs`](../Repository/AuthRepository.cs) | Lấy tài khoản theo `UserId` sau khi Face Login tìm được Student. |
| [`Repository/UserRepository.cs`](../Repository/UserRepository.cs) | Khi soft-delete Student, đồng thời soft-delete Face ID và xóa embedding. |
| [`Configuration/FaceRecognitionOptions.cs`](../Configuration/FaceRecognitionOptions.cs) | Khai báo và kiểm tra các giá trị trong phần `FaceRecognition` của config. |
| [`Program.cs`](../Program.cs) | Đăng ký DI, đọc config và giữ `FaceRecognitionService`/session service dưới dạng singleton. |
| [`Migrations/20260923155732_AddFaceCredentials.cs`](../Migrations/20260923155732_AddFaceCredentials.cs) | Migration tạo bảng Face ID mới; không sửa migration cũ. |
| [`appsettings.json`](../appsettings.json) | Đường dẫn model, điều kiện chất lượng và các ngưỡng so sánh. |
| [`Models/README.md`](../Models/README.md) | Tên file model, nguồn tải và lưu ý khi đổi model. |

## 4. Xử lý một ảnh

Method `TryCreateEmbeddingAsync` chạy theo thứ tự:

1. Kiểm tra file không rỗng và không vượt `MaxImageBytes` (hiện là 5 MB).
2. Đọc file vào RAM và giải mã thành ảnh OpenCV.
3. Lazy-load YuNet và SFace ở request Face ID đầu tiên.
4. YuNet phải tìm thấy **đúng một khuôn mặt**.
5. `PassesQualityChecks` kiểm tra chất lượng.
6. SFace `AlignCrop` căn khuôn mặt theo mắt, mũi và miệng.
7. SFace tạo embedding.
8. Chuẩn hóa embedding về độ dài 1.

### Điều kiện của YuNet và `PassesQualityChecks`

Các giá trị dưới đây lấy trực tiếp từ `appsettings.json`:

| Kiểm tra | Điều kiện hiện tại | Ý nghĩa |
| --- | --- | --- |
| Điểm phát hiện | `DetectionScoreThreshold = 0.9` | YuNet chỉ nhận khuôn mặt đủ chắc chắn. |
| Số khuôn mặt | Chính xác 1 | Không nhận ảnh không có mặt hoặc có nhiều người. |
| Kích thước mặt | `faceWidth / imageWidth >= 0.15` | Chiều rộng mặt phải chiếm ít nhất 15% chiều rộng ảnh. |
| Độ sáng | Từ `40` đến `220` | Loại ảnh quá tối hoặc quá sáng. |
| Độ nét | `blurVariance >= 40` | Dùng Laplacian variance; giá trị thấp thường là ảnh mờ. |
| Độ nghiêng | `rollDegrees <= 15` | Đường nối hai mắt không được nghiêng quá 15 độ. |

`NmsThreshold = 0.3` giúp YuNet gộp các vùng phát hiện trùng nhau. `DetectionTopK = 5000` giới hạn số vùng được xem xét.

Khi ảnh bị loại, server ghi lý do vào terminal và `Logs/api.log`, ví dụ:

```text
Reason=NoFaceDetected
Reason=FaceCountNotOne
Reason=FaceTooSmall
Reason=BrightnessOutOfRange
Reason=ImageTooBlurry
Reason=FaceTooTilted
```

Client chỉ nhận lỗi chung để không làm lộ thông tin nhận diện.

## 5. Tạo embedding đại diện

Cả đăng ký và đăng nhập đều dùng 3 embedding:

```text
Chuẩn hóa từng embedding
        |
        v
Lấy trung bình từng phần tử
        |
        v
Chuẩn hóa kết quả lần nữa
```

Kết quả cuối là một embedding đại diện. Hai embedding được so bằng cosine similarity: càng gần `1` thì càng giống nhau.

## 6. Flow đăng ký hoặc thay Face ID

```text
Student login password -> JWT
       |
POST /api/student/face/register/start + password
       |
Tạo session RAM 10 phút
       |
Gửi 3 lần POST /register/{sessionId}/frames
       |
YuNet -> quality check -> SFace
       |
POST /register/{sessionId}/confirm
       |
Gộp 3 embedding
       |
Kiểm tra trùng với Student khác
       |
Transaction thay Face ID trong SQL Server
```

Chi tiết:

1. Controller yêu cầu role Student.
2. Backend lấy `UserId` từ JWT; frontend không gửi `UserId`.
3. Password được kiểm tra lại.
4. Mỗi request frame dùng `multipart/form-data` với field `frame`.
5. Embedding hợp lệ được giữ trong RAM; ảnh được dispose sau khi xử lý.
6. Confirm chỉ chạy khi đủ 3 embedding.
7. Nếu similarity với Face ID của Student khác đạt `DuplicateThreshold`, hệ thống trả conflict.
8. Nếu Student đã có Face ID, flow được coi là **replace**:
   - soft-delete bản cũ;
   - đặt embedding cũ thành `NULL`;
   - tạo bản mới trong cùng transaction.
9. Nếu lưu bản mới thất bại, transaction rollback và Face ID cũ vẫn còn.
10. Session chỉ bị xóa sau khi lưu thành công.

Duplicate protection cố ý bỏ qua Face ID của chính Student hiện tại để cho phép thay Face ID.

## 7. Flow Face Login

Endpoint public:

```text
POST /api/auth/face-login
Content-Type: multipart/form-data
Field: request (đúng 3 file)
```

Flow:

1. Backend yêu cầu đúng 3 ảnh mới; 3 ảnh lúc đăng ký không được dùng lại tự động.
2. Mỗi ảnh đi qua YuNet, quality check và SFace.
3. Ba embedding được gộp thành một embedding đại diện.
4. Repository lấy tất cả Face ID Student đang hoạt động có cùng model, version và số chiều.
5. Backend tính cosine similarity và xếp hạng từ cao xuống thấp.
6. Chỉ đăng nhập khi:

```text
top1 >= MatchThreshold
và
top1 - top2 >= MinMargin
```

Nếu chỉ có một Face ID thì chỉ kiểm tra `MatchThreshold`. Khi thành công, backend lấy đúng Student và dùng flow JWT hiện có.

Log so sánh có dạng:

```text
Top1Similarity=0.9607
Top2Similarity=N/A
MatchThreshold=0.3630
MinMargin=0.0500
```

Các lý do từ chối thường gặp:

```text
FrameValidationFailed
NoCompatibleCredential
BelowMatchThreshold
InsufficientMargin
MatchedStudentUnavailable
```

## 8. Database

Bảng `FACE_CREDENTIALS` lưu:

| Cột | Dữ liệu |
| --- | --- |
| `user_id` | Student sở hữu Face ID. |
| `embedding` | `varbinary(max)`, mỗi số `float` chiếm 4 byte. |
| `embedding_dimension` | Số phần tử embedding. |
| `model_name`, `model_version` | Xác định pipeline đã tạo embedding. |
| `isDelete`, `deleted_at` | Trạng thái soft-delete. |
| `created_at`, `updated_at` | Thời gian UTC. |

Filtered unique index đảm bảo mỗi Student chỉ có một Face ID đang hoạt động. Khi xóa Face ID hoặc Student:

```text
isDelete = true
embedding = NULL
deleted_at = UTC now
```

Đổi recognition model hoặc processing version yêu cầu Student đăng ký lại vì hệ thống không lưu ảnh gốc.

## 9. Model local

Cần hai file trong thư mục `Models`:

| Model | File | Dùng để |
| --- | --- | --- |
| YuNet | `face_detection_yunet_2023mar.onnx` | Tìm mặt và các điểm mắt/mũi/miệng. |
| SFace FP32 | `face_recognition_sface_2021dec.onnx` | Căn mặt và tạo embedding. |

Nguồn chính thức:

- [YuNet trên OpenCV Zoo](https://github.com/opencv/opencv_zoo/tree/main/models/face_detection_yunet)
- [SFace trên OpenCV Zoo](https://github.com/opencv/opencv_zoo/tree/main/models/face_recognition_sface)

Hash của các file đang dùng:

| File | SHA-256 |
| --- | --- |
| YuNet | `8F2383E4DD3CFBB4553EA8718107FC0423210DC964F9F4280604804ED2552FA4` |
| SFace FP32 | `0BA9FBFA01B5270C96627C4EF784DA859931E02F04419C829E83484087C34E79` |

File ONNX bị Git ignore và phải được chép riêng sang VM. API không tự tải model. Trước khi phân phối model trong môi trường công ty, cần kiểm tra license tại trang model chính thức.

Model được lazy-load một lần ở request Face ID đầu tiên. Có khóa thread-safe để nhiều request đầu tiên không load model cùng lúc. Nếu model thiếu hoặc lỗi, chỉ Face ID thất bại; password login và startup vẫn hoạt động.

## 10. Thư viện

Các package đã có trong `Maiven_Portal_Managment.csproj`; thông thường chỉ cần chạy `dotnet restore`.

| Package | Tác dụng |
| --- | --- |
| `OpenCvSharp5.Windows` | Gọi OpenCV, YuNet và SFace trên Windows. |
| `Microsoft.EntityFrameworkCore.SqlServer` | Đọc/ghi SQL Server. |
| `Microsoft.EntityFrameworkCore.Design` | Tạo và chạy migration. |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Xác thực JWT. |
| `Swashbuckle.AspNetCore` | Swagger UI. |
| `log4net` | Ghi log terminal và file. |
| `DotNetEnv` | Đọc secret local từ `.env` trong Development. |

Project hiện dùng .NET 10 và `OpenCvSharp5.Windows 5.0.0.20260905`.

## 11. Cách cài và chạy trên Windows

Yêu cầu:

- .NET 10 SDK.
- SQL Server/SQL Server Express/LocalDB đang chạy.
- Database connection string đúng.
- Hai file ONNX trong `Models`.

Tạo `.env` từ `.env.example`, sau đó sửa connection string và JWT secret. File `.env` chỉ được đọc khi chạy Development.

Cài EF tool nếu máy chưa có:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12
```

Restore, cập nhật database và chạy:

```powershell
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet restore
dotnet ef database update
dotnet run
```

Mở URL Swagger được in trong terminal.

Nếu ổ C thiếu dung lượng:

```powershell
$env:NUGET_PACKAGES = 'E:\NuGetPackages'
$env:NUGET_HTTP_CACHE_PATH = 'E:\NuGetHttpCache'
dotnet restore
```

## 12. Test end-to-end bằng Swagger

### Đăng ký Face ID

1. Gọi `POST /api/auth/login` bằng tài khoản Student.
2. Copy `accessToken`.
3. Chọn **Authorize** và nhập `Bearer <accessToken>`.
4. Gọi `GET /api/student/face/status`.
5. Gọi `POST /api/student/face/register/start` với password hiện tại.
6. Copy `sessionId`.
7. Gọi `POST /api/student/face/register/{sessionId}/frames` ba lần, mỗi lần chọn một file ở field `frame`.
8. Chờ `acceptedCount = 3` và `isComplete = true`.
9. Gọi `POST /api/student/face/register/{sessionId}/confirm`.
10. Kiểm tra lại status.

### Face Login

1. Bỏ JWT khỏi Swagger.
2. Gọi `POST /api/auth/face-login`.
3. Chọn đúng 3 file trong field `request`.
4. Thành công sẽ trả JWT giống password login.

Có thể test cùng file ba lần bằng `curl.exe`, nhưng cách này không đánh giá được độ ổn định thực tế:

```powershell
curl.exe -X POST "http://localhost:<port>/api/auth/face-login" `
  -F "request=@face.jpg" `
  -F "request=@face.jpg" `
  -F "request=@face.jpg"
```

### Xóa Face ID

1. Authorize bằng JWT của Student.
2. Gọi `DELETE /api/student/face` với password.
3. Status phải trả `isRegistered = false`.
4. Face Login bằng credential cũ phải thất bại.

## 13. Cách tracing lỗi

Theo thứ tự:

1. Tìm endpoint trong Controller.
2. Theo method sang `FaceCredentialService` hoặc `AuthService`.
3. Nếu ảnh bị loại, xem `FaceRecognitionService`.
4. Nếu dữ liệu sai, xem `FaceCredentialRepository` và bảng `FACE_CREDENTIALS`.
5. Xem terminal hoặc:

```text
Logs/api.log
```

Sau khi sửa code, phải dừng app đang chạy và chạy lại `dotnet run` để log mới có hiệu lực.

## 14. Các giá trị chưa chốt và giới hạn MVP

- `MatchThreshold = 0.363`, `MinMargin = 0.05` và `DuplicateThreshold = 0.5` chưa phải giá trị cuối.
- Cần benchmark SFace normal, INT8 và INT8 block trước khi chọn model/ngưỡng.
- Chưa có liveness/anti-spoofing.
- Chưa mã hóa embedding ở tầng ứng dụng.
- Linear scan phù hợp quy mô dưới 40 người; chưa cần vector database.
- Xóa Face ID không thu hồi JWT đã cấp.
- Session đang làm dở mất khi backend restart.
- Hiện cùng một file có thể được tính là nhiều frame hợp lệ.
- Đăng ký lại của cùng Student được hiểu là replace, không phải duplicate.
- Ảnh gốc không được lưu nên đổi model phải đăng ký Face ID lại.

## 15. Publish lên Windows VM

```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

Khi chuyển sang VM cần mang theo:

- thư mục publish;
- hai file ONNX;
- connection string và JWT secret qua environment variables;
- .NET 10 runtime nếu publish framework-dependent.

Giữ nguyên các DLL native của OpenCvSharp trong publish output. Không cần Internet khi ứng dụng đang chạy.
