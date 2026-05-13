namespace WebEcommer.Models.ViewModels;

public class ProductViewModel
{
    public int MaHh { get; set; }
    public string TenHh { get; set; } = null!;
    public string? MoTa { get; set; }
    public string? Hinh { get; set; }
    public string ImageUrl { get; set; } = null!;
}
