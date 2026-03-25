🎬 WebsitePhim — Hệ thống Quản lý & Xem Phim Trực tuyến

Ứng dụng web xây dựng bằng ASP.NET Core 8 MVC, cho phép người dùng khám phá phim, xem trailer YouTube, bình luận & đánh giá — kèm Admin Panel quản lý toàn bộ nội dung.
✨ Tính năng nổi bật
<img width="486" height="643" alt="image" src="https://github.com/user-attachments/assets/05486a39-815e-49b3-b444-cb545d31eabc" />
<img width="489" height="485" alt="image" src="https://github.com/user-attachments/assets/a1e165e5-dd70-4d60-b85d-c9b9b3f09a39" />
<img width="503" height="254" alt="image" src="https://github.com/user-attachments/assets/22bbc944-c5ea-4d09-a1a2-79b156500d76" />
<img width="493" height="291" alt="image" src="https://github.com/user-attachments/assets/965e9997-2547-4026-a2db-2812cf5e7277" />
<img width="490" height="296" alt="image" src="https://github.com/user-attachments/assets/b98f7aab-adb4-4349-a512-7a1b639e873c" />
<img width="485" height="245" alt="image" src="https://github.com/user-attachments/assets/f682099f-c8b8-495c-a860-7db8e1f76e3b" />

👤 Người dùng (User)

Trang chủ — Danh sách phim mới nhất, phim nổi bật.
Lọc & Tìm kiếm — Theo tên phim, thể loại, năm phát hành.
Chi tiết phim — Mô tả, poster, trailer/tập phim nhúng từ YouTube.
Tương tác — Đăng nhập để bình luận và chấm điểm (Rating ⭐).
Phụ đề — Hỗ trợ hiển thị thông tin phụ đề đa ngôn ngữ.

🔐 Quản trị viên (Admin)

Quản lý phim — CRUD đầy đủ: thêm, sửa, ẩn/hiện phim.
Quản lý tập phim — Thêm tập cho phim bộ.
Quản lý người dùng — Danh sách thành viên, phân quyền Admin/User, khóa/mở khóa tài khoản.
Quản lý thể loại — Thêm/sửa/xóa danh mục thể loại phim.
Xử lý YouTube thông minh — Tự động trích xuất Video ID từ nhiều dạng URL YouTube khác nhau để nhúng trình phát.


🛠 Công nghệ sử dụng
LayerCông nghệBackendASP.NET Core 8.0 MVCORMEntity Framework Core 8DatabaseMicrosoft SQL ServerAuthenticationASP.NET Core IdentityFrontendHTML5, CSS3, JavaScript, Bootstrap 5IconsFont Awesome

🚀 Hướng dẫn cài đặt
Yêu cầu hệ thống

.NET 8 SDK
SQL Server (bất kỳ phiên bản nào)
Visual Studio 2022 hoặc VS Code

Các bước cài đặt
1. Clone dự án
bashgit clone https://github.com/YourUsername/WebsitePhim.git
cd WebsitePhim
2. Cấu hình Database
Mở file appsettings.json và chỉnh sửa chuỗi kết nối:
json"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=MovieDB;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
}

Thay YOUR_SERVER_NAME bằng tên SQL Server instance của bạn (ví dụ: localhost, .\\SQLEXPRESS).

3. Chạy Database Migration
Mở Package Manager Console trong Visual Studio và chạy:
powershellUpdate-Database
Hoặc dùng terminal:
bashdotnet ef database update
4. Khởi chạy ứng dụng
bashdotnet run
Hoặc nhấn F5 trong Visual Studio. Website sẽ chạy tại https://localhost:xxxx.

📝 Tài khoản dùng thử
Vai tròEmailMật khẩuAdminadmin@websitephim.comAdmin@123Useruser@gmail.comUser@123

⚠️ Ứng dụng tự động tạo các tài khoản mẫu và dữ liệu thể loại phim khi khởi chạy lần đầu (Seed Data trong Program.cs).


📁 Cấu trúc dự án
WebsitePhim/
├── Areas/
│   └── Admin/              # Khu vực quản trị (Admin Panel)
│       ├── Controllers/
│       └── Views/
├── Controllers/            # Controller phía người dùng
├── Models/                 # Entity models & ViewModels
├── Views/                  # Razor Views phía người dùng
├── wwwroot/                # Static files (CSS, JS, Images)
│   └── images/movies/      # Ảnh poster phim (upload)
├── appsettings.json        # Cấu hình ứng dụng
└── Program.cs              # Entry point & Seed Data

📩 Liên hệ

Tác giả: Nguyễn Anh Phát
Email: nphat8179@gmail.com
LinkedIn: 
GitHub: https://github.com/phatnguyen-08/WebsiteXemPhim-MVC.NET
