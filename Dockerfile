# =========================
# Stage 1: Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Project chính nằm ngay thư mục gốc repo, không có .sln liên quan ở đây
# Copy .csproj trước để tận dụng Docker layer cache
# (chỉ restore lại package khi .csproj thay đổi, không phải mỗi lần sửa code)
COPY ["Maiven_Portal_Managment.csproj", "./"]
RUN dotnet restore "Maiven_Portal_Managment.csproj"

# Copy toàn bộ source code còn lại
COPY . .

# Build và publish ra thư mục /app/publish
RUN dotnet publish "Maiven_Portal_Managment.csproj" -c Release -o /app/publish --no-restore

# =========================
# Stage 2: Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy kết quả build từ stage trước (image runtime nhẹ hơn nhiều so với SDK image)
COPY --from=build /app/publish .

# Render cấp PORT qua biến môi trường PORT, cần lắng nghe đúng cổng đó
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Maiven_Portal_Managment.dll"]
