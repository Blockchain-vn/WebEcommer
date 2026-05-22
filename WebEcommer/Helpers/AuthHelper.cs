using Microsoft.AspNetCore.Http;

namespace WebEcommer.Helpers;

public static class AuthHelper
{
    public const int ROLE_USER = 0;
    public const int ROLE_ADMIN = 1;

    /// <summary>
    /// Kiểm tra xem user có phải admin không
    /// </summary>
    public static bool IsAdmin(HttpContext context)
    {
        // VaiTro được lưu bằng SetInt32 trong AccountController
        var vaiTro = context.Session.GetInt32("VaiTro");
        return vaiTro.HasValue && vaiTro.Value == ROLE_ADMIN;
    }

    /// <summary>
    /// Kiểm tra xem user đã đăng nhập chưa
    /// </summary>
    public static bool IsLoggedIn(HttpContext context)
    {
        return !string.IsNullOrEmpty(context.Session.GetString("MaKH"));
    }

    /// <summary>
    /// Lấy MaKH từ session
    /// </summary>
    public static string GetUserId(HttpContext context)
    {
        return context.Session.GetString("MaKH") ?? string.Empty;
    }
}
