using System;

namespace WebEcommer.Models;

public class KhachHang
{
    public string MaKh { get; set; } = null!;
    public string HoTen { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? DienThoai { get; set; }
    public string? DiaChi { get; set; }
    public string MatKhau { get; set; } = null!;
    public DateTime NgaySinh { get; set; }
    public bool GioiTinh { get; set; }
    public string? Hinh { get; set; }
    public bool HieuLuc { get; set; }
    public int VaiTro { get; set; }
    public string? RandomKey { get; set; }
}
