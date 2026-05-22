using Microsoft.EntityFrameworkCore;
using WebEcommer.Data;
using WebEcommer.Models;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register DbContext for EcommerDemo
builder.Services.AddDbContext<EcommerDemoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyConnectString")));

// Configure MailSettings
builder.Services.Configure<WebEcommer.Helpers.MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<WebEcommer.Services.IEmailSender, WebEcommer.Services.EmailSender>();

var app = builder.Build();

// =====================================================
// SEED ADMIN ACCOUNT
// =====================================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EcommerDemoContext>();

    // Kiểm tra tài khoản admin đã tồn tại chưa
    var adminEmail = "admin@gmail.com";
    var adminExists = context.KhachHangs.Any(k => k.Email == adminEmail);

    if (!adminExists)
    {
        // Hàm hash MD5 (giống AccountController)
        static string HashMD5(string input)
        {
            using var md5 = MD5.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = md5.ComputeHash(bytes);
            var sb = new StringBuilder();
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        var adminAccount = new KhachHang
        {
            MaKh       = "KH0000",         // Mã đặc biệt cho admin
            HoTen      = "Administrator",
            Email      = adminEmail,
            DienThoai  = "",
            DiaChi     = "",
            MatKhau    = HashMD5("admin@123"),
            NgaySinh   = new DateTime(1990, 1, 1),
            GioiTinh   = true,
            Hinh       = "User.jpg",
            HieuLuc    = true,
            VaiTro     = 1,                // 1 = Admin
            RandomKey  = Guid.NewGuid().ToString().Substring(0, 8)
        };

        context.KhachHangs.Add(adminAccount);
        context.SaveChanges();
        Console.WriteLine("✅ Đã tạo tài khoản Admin: admin@gmail.com / admin@123");
    }
    else
    {
        // Nếu đã tồn tại, đảm bảo VaiTro = 1 (Admin)
        var admin = context.KhachHangs.First(k => k.Email == adminEmail);
        if (admin.VaiTro != 1)
        {
            admin.VaiTro = 1;
            context.SaveChanges();
            Console.WriteLine("✅ Đã cập nhật quyền Admin cho: admin@gmail.com");
        }
    }
}
// =====================================================

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session before authorization
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
