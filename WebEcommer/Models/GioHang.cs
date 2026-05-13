using System;

namespace WebEcommer.Models;

public class GioHang
{
    public int MaGh { get; set; }
    public string MaKh { get; set; } = null!; // Mã khách hàng (bắt buộc)
    public int MaHh { get; set; }
    public int SoLuong { get; set; }
    public DateTime NgayThem { get; set; }

    public virtual HangHoa? HangHoa { get; set; }
    public virtual KhachHang? KhachHang { get; set; }
}

