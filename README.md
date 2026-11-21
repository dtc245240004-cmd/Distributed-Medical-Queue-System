
# Distributed Medical Queue System
Hệ thống đặt lịch khám và quản lý hàng đợi

## 1. Giới thiệu
Dự án xây dựng hệ thống đặt lịch khám và quản lý hàng đợi khám bệnh theo kiến trúc ứng dụng phân tán.

Hệ thống cho phép bệnh nhân tìm kiếm bác sĩ, xem lịch khám, đặt lịch, hủy lịch và theo dõi số thứ tự khám.

## 2. Công nghệ sử dụng
- C#: ngôn ngữ lập trình chính
- .NET 10: nền tảng phát triển
- ASP.NET Core Minimal API: xây dựng REST API
- PostgreSQL: hệ quản trị cơ sở dữ liệu
- RabbitMQ: hệ thống message broker
- SignalR: giao tiếp thời gian thực
- Docker và Docker Compose: triển khai các thành phần

## 3. Kiến trúc hệ thống
Hệ thống gồm các thành phần:
- Patient Client: giao diện bệnh nhân
- Appointment API: quản lý lịch hẹn
- Queue Worker: xử lý yêu cầu cấp số thứ tự
- Notification Service: gửi thông báo thời gian thực
- Shared.Contracts: chia sẻ cấu trúc message giữa các dịch vụ
- PostgreSQL: lưu trữ dữ liệu
- RabbitMQ: truyền tải message giữa các dịch vụ

## 4. Cấu trúc dự án
```text
Distributed-Medical-Queue-System/
├── docs/
├── shared/
│   └── Shared.Contracts/
├── src/
│   ├── Appointment.Api/
│   ├── Notification.Service/
│   ├── Patient.Client/
│   └── Queue.Worker/
├── .env
├── .env.example
├── .gitignore
├── docker-compose.yml
├── DistributedMedicalQueue.slnx
└── README.md
```

## 5. Yêu cầu môi trường
- .NET SDK 10
- Docker Desktop
- Git
- Visual Studio Code

## 6. Khởi chạy cơ sở hạ tầng
Tạo file `.env` từ `.env.example`, sau đó chạy:

```powershell
docker compose up -d
```

Kiểm tra trạng thái:

```powershell
docker compose ps
```

PostgreSQL:
- Host: localhost
- Port: 5432
- Database: MedicalQueueDb

RabbitMQ:
- Host: localhost
- AMQP Port: 5672
- Management UI: http://localhost:15672

Thông tin đăng nhập được cấu hình trong file `.env`.

## 7. Build solution
Tại thư mục gốc dự án:

```powershell
dotnet restore
dotnet build
```

## 8. Tiến độ thực hiện
- [x] Khởi tạo solution và các project
- [x] Thiết lập PostgreSQL bằng Docker
- [x] Thiết lập RabbitMQ bằng Docker
- [ ] Xây dựng mô hình dữ liệu
- [ ] Xây dựng REST API
- [ ] Tích hợp xử lý hàng đợi bằng RabbitMQ
- [ ] Tích hợp thông báo thời gian thực bằng SignalR
- [ ] Xây dựng giao diện bệnh nhân
- [ ] Kiểm thử và hoàn thiện hệ thống

## 9. Tác giả
Sinh viên: Phùng Thị Phương Thảo
