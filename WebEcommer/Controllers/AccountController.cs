using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebEcommer.Data;
using WebEcommer.Models;
using WebEcommer.Models.ViewModels;
using System.Security.Cryptography;
using System.Text;
using WebEcommer.Services;
using System.Text.Json;

namespace WebEcommer.Controllers
{
    public class AccountController : Controller
    {
        private readonly EcommerDemoContext _context;
        private readonly IEmailSender _emailSender;

        public AccountController(EcommerDemoContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET: /Account/Login
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Tìm khách hàng theo Email
                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.Email == model.Email);

                if (khachHang != null)
                {
                    // Mã hóa mật khẩu nhập vào để so sánh
                    string hashedPassword = HashPassword(model.MatKhau);

                    if (khachHang.MatKhau == hashedPassword && khachHang.HieuLuc == true)
                    {
                        // Lưu thông tin vào Session
                        HttpContext.Session.SetString("MaKH", khachHang.MaKh);
                        HttpContext.Session.SetString("HoTen", khachHang.HoTen);
                        HttpContext.Session.SetString("Email", khachHang.Email);
                        HttpContext.Session.SetInt32("VaiTro", khachHang.VaiTro);

                        TempData["Success"] = "Đăng nhập thành công!";

                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            return LocalRedirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
            }
            return View(model);
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.Email == model.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email đã được đăng ký");
                    return View(model);
                }

                try
                {
                    // 1. Tạo thông tin khách hàng tạm thời
                    var khachHang = new KhachHang
                    {
                        MaKh = GenerateMaKH(),
                        HoTen = model.HoTen,
                        Email = model.Email,
                        DienThoai = model.DienThoai ?? "",
                        DiaChi = model.DiaChi ?? "",
                        MatKhau = HashPassword(model.MatKhau),
                        NgaySinh = DateTime.Now,
                        GioiTinh = false,
                        Hinh = "User.jpg",
                        HieuLuc = true,
                        VaiTro = 0,
                        RandomKey = GenerateRandomKey()
                    };

                    // 2. Tạo mã OTP 6 số
                    string otpCode = new Random().Next(100000, 999999).ToString();

                    // 3. Lưu thông tin và OTP vào Session
                    HttpContext.Session.SetString("PendingUser", JsonSerializer.Serialize(khachHang));
                    HttpContext.Session.SetString("RegistrationOTP", otpCode);

                    // 4. Gửi Email
                    await _emailSender.SendEmailAsync(model.Email, "Mã xác thực đăng ký - Ecommer", 
                        $"<h3>Mã OTP của bạn là: <b style='color:red'>{otpCode}</b></h3><p>Mã có hiệu lực trong 5 phút.</p>");

                    TempData["Success"] = "Mã OTP đã được gửi về Email của bạn.";
                    return RedirectToAction("VerifyOTP");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                    return View(model);
                }
            }
            return View(model);
        }

        // GET: /Account/VerifyOTP
        public IActionResult VerifyOTP()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("RegistrationOTP")))
            {
                return RedirectToAction("Register");
            }
            return View();
        }

        // POST: /Account/VerifyOTP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOTP(string otp)
        {
            var sessionOtp = HttpContext.Session.GetString("RegistrationOTP");
            var pendingUserJson = HttpContext.Session.GetString("PendingUser");

            if (string.IsNullOrEmpty(sessionOtp) || string.IsNullOrEmpty(pendingUserJson))
            {
                return RedirectToAction("Register");
            }

            if (otp == sessionOtp)
            {
                // OTP đúng -> Lưu user vào DB
                var khachHang = JsonSerializer.Deserialize<KhachHang>(pendingUserJson);
                if (khachHang != null)
                {
                    _context.KhachHangs.Add(khachHang);
                    await _context.SaveChangesAsync();

                    // Xóa session tạm
                    HttpContext.Session.Remove("RegistrationOTP");
                    HttpContext.Session.Remove("PendingUser");

                    TempData["Success"] = "Đăng ký và xác thực thành công!";
                    return RedirectToAction("Login");
                }
            }

            ModelState.AddModelError("", "Mã OTP không chính xác.");
            return View();
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Đã đăng xuất";
            return RedirectToAction("Index", "Home");
        }

        // Hàm mã hóa mật khẩu MD5
        private string HashPassword(string password)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        // Hàm tạo mã khách hàng
        private string GenerateMaKH()
        {
            try
            {
                // Lấy tất cả mã khách hàng bắt đầu bằng "KH"
                var allKH = _context.KhachHangs
                    .Where(k => k.MaKh.StartsWith("KH"))
                    .ToList(); // Chuyển sang memory để xử lý

                if (allKH.Count > 0)
                {
                    // Sắp xếp theo số, không phải theo chuỗi
                    var maxNumber = allKH
                        .Select(k => new { 
                            Number = int.TryParse(k.MaKh.Substring(2), out var num) ? num : 0,
                            MaKh = k.MaKh
                        })
                        .OrderByDescending(x => x.Number)
                        .FirstOrDefault();

                    if (maxNumber != null && maxNumber.Number > 0)
                    {
                        return $"KH{(maxNumber.Number + 1):0000}";
                    }
                }
            }
            catch (Exception ex)
            {
                // Nếu có lỗi, log và sử dụng GUID làm backup
                System.Diagnostics.Debug.WriteLine($"Lỗi tạo mã KH: {ex.Message}");
            }

            // Mặc định: sử dụng GUID nếu không lấy được mã
            return "KH" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }

        // Hàm tạo RandomKey
        private string GenerateRandomKey()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }
    }
}