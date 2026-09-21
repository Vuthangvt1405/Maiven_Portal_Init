# =========================
# Stage 1: Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy file .csproj trước để tận dụng Docker layer cache
# (chỉ restore lại package khi .csproj thay đổi, không phải mỗi lần sửa code)
COPY *.sln .
COPY ["YourApp/YourApp.csproj", "YourApp/"]
RUN dotnet restore "YourApp/YourApp.csproj"

# Copy toàn bộ source code còn lại
COPY . .
WORKDIR "/src/YourApp"

# Build và publish ra thư mục /app/publish
RUN dotnet publish -c Release -o /app/publish --no-restore

# =========================
# Stage 2: Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy kết quả build từ stage trước (image runtime nhẹ hơn nhiều so với SDK image)
COPY --from=build /app/publish .

# Render cấp PORT qua biến môi trường PORT, cần lắng nghe đúng cổng đó
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "YourApp.dll"]
