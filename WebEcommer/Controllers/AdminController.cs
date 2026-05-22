using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebEcommer.Data;
using WebEcommer.Helpers;
using WebEcommer.Models;
using System.Linq;
using System.Threading.Tasks;

namespace WebEcommer.Controllers;

public class AdminController : Controller
{
    private readonly EcommerDemoContext _context;

    public AdminController(EcommerDemoContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Kiểm tra quyền Admin
    /// </summary>
    private bool CheckAdminAccess()
    {
        if (!AuthHelper.IsAdmin(HttpContext))
        {
            TempData["Error"] = "Bạn không có quyền truy cập trang này!";
            return false;
        }
        return true;
    }

    /// <summary>
    /// TC11 - Admin thêm sản phẩm mới
    /// </summary>
    public async Task<IActionResult> AddProduct()
    {
        if (!CheckAdminAccess())
            return RedirectToAction("Index", "Home");

        ViewBag.Loais = await _context.Loais.ToListAsync();
        ViewBag.NhaCungCaps = await _context.NhaCungCaps.ToListAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct(HangHoa model, IFormFile imgFile)
    {
        if (!CheckAdminAccess())
            return RedirectToAction("Index", "Home");

        try
        {
            // Validate sản phẩm
            if (string.IsNullOrWhiteSpace(model.TenHh))
            {
                TempData["Error"] = "Tên sản phẩm không được để trống!";
                ViewBag.Loais = await _context.Loais.ToListAsync();
                ViewBag.NhaCungCaps = await _context.NhaCungCaps.ToListAsync();
                return View(model);
            }

            if (model.DonGia <= 0)
            {
                TempData["Error"] = "Giá sản phẩm phải lớn hơn 0!";
                ViewBag.Loais = await _context.Loais.ToListAsync();
                ViewBag.NhaCungCaps = await _context.NhaCungCaps.ToListAsync();
                return View(model);
            }

            // Xử lý upload ảnh
            if (imgFile != null && imgFile.Length > 0)
            {
                var fileName = Path.GetFileName(imgFile.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "image", "Hinh", "HangHoa");
                
                Directory.CreateDirectory(uploadPath);
                
                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imgFile.CopyToAsync(stream);
                }
                
                model.Hinh = fileName;
            }

            // Để SQL Server tự tăng MaHh (IDENTITY), không gán thủ công

            // Gán giá trị mặc định cho các trường bắt buộc không có trong form
            if (model.MaLoai == 0)
            {
                var firstLoai = await _context.Loais.FirstOrDefaultAsync();
                model.MaLoai = firstLoai?.MaLoai ?? 1;
            }
            if (string.IsNullOrEmpty(model.MaNcc))
            {
                var firstNcc = await _context.NhaCungCaps.FirstOrDefaultAsync();
                model.MaNcc = firstNcc?.MaNcc ?? "NCC01";
            }
            if (model.NgaySx == default)
                model.NgaySx = DateTime.Now;

            _context.HangHoas.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Sản phẩm '{model.TenHh}' được thêm thành công!";
            return RedirectToAction("ProductList");
        }
        catch (Exception ex)
        {
            // Lấy inner exception để biết lỗi chi tiết
            var innerMsg = ex.InnerException?.Message ?? "(không có inner exception)";
            TempData["Error"] = $"Lỗi: {ex.Message} | Chi tiết: {innerMsg}";
        }

        ViewBag.Loais = await _context.Loais.ToListAsync();
        ViewBag.NhaCungCaps = await _context.NhaCungCaps.ToListAsync();
        return View(model);
    }

    /// <summary>
    /// TC12 - Admin sửa thông tin sản phẩm
    /// </summary>
    public async Task<IActionResult> EditProduct(int id)
    {
        if (!CheckAdminAccess())
            return RedirectToAction("Index", "Home");

        var product = await _context.HangHoas.FindAsync(id);
        if (product == null)
        {
            TempData["Error"] = "Sản phẩm không tồn tại!";
            return RedirectToAction("ProductList");
        }

        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> EditProduct(int id, HangHoa model, IFormFile imgFile)
    {
        if (!CheckAdminAccess())
            return RedirectToAction("Index", "Home");

        var product = await _context.HangHoas.FindAsync(id);
        if (product == null)
        {
            TempData["Error"] = "Sản phẩm không tồn tại!";
            return RedirectToAction("ProductList");
        }

        try
        {
            // Validate
            if (string.IsNullOrWhiteSpace(model.TenHh))
            {
                TempData["Error"] = "Tên sản phẩm không được để trống!";
                return View(product);
            }

            if (model.DonGia <= 0)
            {
                TempData["Error"] = "Giá sản phẩm phải lớn hơn 0!";
                return View(product);
            }

            // Cập nhật thông tin
            product.TenHh = model.TenHh;
            product.DonGia = model.DonGia;
            product.SoLuong = model.SoLuong;
            product.MoTa = model.MoTa;
            product.MoTaDonVi = model.MoTaDonVi;

            // Xử lý upload ảnh mới
            if (imgFile != null && imgFile.Length > 0)
            {
                var fileName = Path.GetFileName(imgFile.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "image", "Hinh", "HangHoa");
                
                Directory.CreateDirectory(uploadPath);
                
                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imgFile.CopyToAsync(stream);
                }
                
                product.Hinh = fileName;
            }

            _context.HangHoas.Update(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Sản phẩm '{product.TenHh}' được cập nhật thành công!";
            return RedirectToAction("ProductList");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Lỗi: {ex.Message}";
        }

        return View(product);
    }

    /// <summary>
    /// TC13 - Admin xóa sản phẩm
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        if (!CheckAdminAccess())
            return RedirectToAction("Index", "Home");

        var product = await _context.HangHoas.FindAsync(id);
        if (product == null)
        {
            TempData["Error"] = "Sản phẩm không tồn tại!";
            return RedirectToAction("ProductList");
        }

        try
        {
            _context.HangHoas.Remove(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Sản phẩm '{product.TenHh}' đã được xóa!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Lỗi: {ex.Message}";
        }

        return RedirectToAction("ProductList");
    }

    /// <summary>
    /// Danh sách sản phẩm
    /// </summary>
    public async Task<IActionResult> ProductList(int page = 1, int record = 10)
    {
        if (!CheckAdminAccess())
            return RedirectToAction("Index", "Home");

        if (page < 1) page = 1;
        if (record <= 0) record = 10;

        var totalCount = await _context.HangHoas.CountAsync();
        var totalPages = (int)System.Math.Ceiling((double)totalCount / record);
        if (totalPages == 0) totalPages = 1;
        if (page > totalPages) page = totalPages;

        var products = await _context.HangHoas
            .OrderByDescending(h => h.MaHh)
            .Skip((page - 1) * record)
            .Take(record)
            .ToListAsync();

        ViewBag.Page = page;
        ViewBag.Record = record;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCount = totalCount;

        return View(products);
    }

    /// <summary>
    /// TC14 - Admin duyệt đơn hàng
    /// </summary>
    public async Task<IActionResult> OrderList(int page = 1, int record = 10)
    {
        if (!CheckAdminAccess())
            return RedirectToAction("Index", "Home");

        if (page < 1) page = 1;
        if (record <= 0) record = 10;

        // Note: Bạn cần tạo model DonHang để lưu trữ đơn hàng
        // Hiện tại trả về view trống
        ViewBag.Page = page;
        ViewBag.Record = record;
        
        return View();
    }

    /// <summary>
    /// Cập nhật trạng thái đơn hàng
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, int status)
    {
        if (!CheckAdminAccess())
            return RedirectToAction("Index", "Home");

        // Implement logic cập nhật trạng thái đơn hàng
        TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công!";
        return RedirectToAction("OrderList");
    }

    /// <summary>
    /// Trang Admin Dashboard
    /// </summary>
    public IActionResult Dashboard()
    {
        if (!CheckAdminAccess())
            return RedirectToAction("Index", "Home");

        return View();
    }
}
