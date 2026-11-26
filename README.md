# Distributed Medical Queue System

Hệ thống đặt lịch khám và quản lý hàng đợi

## 1. Giới thiệu

Dự án xây dựng hệ thống đặt lịch khám và quản lý hàng đợi khám bệnh theo kiến trúc ứng dụng phân tán.

Hệ thống cho phép bệnh nhân tìm kiếm bác sĩ, xem lịch khám, đặt lịch, hủy lịch và theo dõi số thứ tự khám.

## 2. Công nghệ sử dụng

- C#: ngôn ngữ lập trình chính
- .NET 10: nền tảng phát triển
- ASP.NET Core Minimal API: xây dựng REST API
- Entity Framework Core: ORM làm việc với cơ sở dữ liệu
- PostgreSQL: hệ quản trị cơ sở dữ liệu
- RabbitMQ: hệ thống message broker
- SignalR: giao tiếp thời gian thực
- Docker và Docker Compose: triển khai các thành phần
- Git và GitHub: quản lý mã nguồn

## 3. Kiến trúc hệ thống

Hệ thống gồm các thành phần:

- Patient Client: giao diện bệnh nhân
- Appointment API: quản lý lịch hẹn
- Queue Worker: xử lý yêu cầu cấp số thứ tự
- Notification Service: gửi thông báo thời gian thực
- Shared.Contracts: chia sẻ cấu trúc message giữa các dịch vụ
- PostgreSQL: lưu trữ dữ liệu
- RabbitMQ: truyền tải message giữa các dịch vụ

Luồng xử lý dự kiến:

1. Bệnh nhân gửi yêu cầu đặt lịch qua Patient Client.
2. Appointment API kiểm tra thông tin và lưu lịch hẹn vào PostgreSQL.
3. Appointment API gửi yêu cầu cấp số thứ tự qua RabbitMQ.
4. Queue Worker xử lý yêu cầu và cập nhật số thứ tự.
5. Notification Service nhận sự kiện và gửi thông báo qua SignalR.

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
- dotnet-ef 10 (phục vụ quản lý migration)

## 6. Khởi chạy cơ sở hạ tầng

Tạo file `.env` từ `.env.example` và cấu hình thông tin kết nối phù hợp.

Khởi động PostgreSQL và RabbitMQ:

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

Thông tin đăng nhập được cấu hình trong file `.env`. Không đưa file `.env` chứa mật khẩu thật lên GitHub.

## 7. Cấu hình và cập nhật cơ sở dữ liệu

Appointment API sử dụng Entity Framework Core và PostgreSQL thông qua Npgsql.

Cấu hình connection string bằng .NET User Secrets trong môi trường phát triển:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=MedicalQueueDb;Username=YOUR_USER;Password=YOUR_PASSWORD" --project .\src\Appointment.Api\Appointment.Api.csproj
```

Tạo migration:

```powershell
dotnet ef migrations add InitialCreate --project .\src\Appointment.Api\Appointment.Api.csproj --startup-project .\src\Appointment.Api\Appointment.Api.csproj
```

Cập nhật database:

```powershell
dotnet ef database update --project .\src\Appointment.Api\Appointment.Api.csproj --startup-project .\src\Appointment.Api\Appointment.Api.csproj
```

Ứng dụng tự động tạo dữ liệu mẫu khi khởi động nếu bảng Doctors chưa có dữ liệu.

Dữ liệu mẫu gồm:

- 3 bác sĩ
- 3 bệnh nhân
- 6 khung giờ khám

## 8. Build solution

Tại thư mục gốc dự án:

```powershell
dotnet restore
dotnet build
```

## 9. Chạy Appointment API

```powershell
dotnet run --project .\src\Appointment.Api\Appointment.Api.csproj --urls http://localhost:5001
```

API chạy tại:

http://localhost:5001

Kiểm tra trạng thái:

```powershell
curl.exe http://localhost:5001/
```

Kiểm tra kết nối PostgreSQL:

```powershell
curl.exe http://localhost:5001/health/db
```

## 10. REST API hiện có

| Method | Endpoint | Chức năng |
|---|---|---|
| GET | `/doctors` | Lấy danh sách bác sĩ |
| GET | `/doctors/{id}/slots` | Lấy khung giờ khám còn trống |
| POST | `/appointments` | Tạo lịch hẹn |
| GET | `/appointments/{id}` | Xem thông tin lịch hẹn |
| DELETE | `/appointments/{id}` | Hủy lịch hẹn |
| GET | `/queues/{doctorId}` | Xem danh sách hàng đợi của bác sĩ |

Các mã phản hồi HTTP được sử dụng:

- 200 OK: yêu cầu thành công
- 201 Created: tạo lịch hẹn thành công
- 400 Bad Request: dữ liệu yêu cầu không hợp lệ
- 404 Not Found: không tìm thấy dữ liệu
- 409 Conflict: xung đột, ví dụ khung giờ đã được đặt hoặc lịch đã hủy

Lưu ý: Chức năng cấp số thứ tự và xử lý hàng đợi bằng RabbitMQ sẽ được triển khai ở tuần 3. Vì vậy, API hàng đợi hiện có thể trả về danh sách rỗng.

## 11. Tiến độ thực hiện

- [x] Tuần 1: Khởi tạo solution, các project, Docker Compose và thiết kế kiến trúc.
- [x] Tuần 2: Xây dựng mô hình dữ liệu, EF Core, PostgreSQL, migration, dữ liệu mẫu và REST API cơ bản.
- [ ] Tuần 3: Tích hợp RabbitMQ và Queue Worker.
- [ ] Tuần 4: Tích hợp Notification Service và SignalR.
- [ ] Tuần 5: Xây dựng Patient Client và hoàn thiện Docker Compose.
- [ ] Tuần 6: Kiểm thử, xử lý lỗi, hoàn thiện tài liệu và báo cáo.

## 12. Tác giả

Sinh viên: Phùng Thị Phương Thảo
