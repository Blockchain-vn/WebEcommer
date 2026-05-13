using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebEcommer.Data;
using WebEcommer.Models;

namespace WebEcommer.Controllers;

public class CartController : Controller
{
    private readonly EcommerDemoContext _context;

    public CartController(EcommerDemoContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int maHh, int quantity = 1)
    {
        var maKh = HttpContext.Session.GetString("MaKH");
        if (string.IsNullOrEmpty(maKh))
        {
            // Nếu chưa đăng nhập, chuyển hướng đến trang Login và lưu ReturnUrl
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Home") });
        }

        // Kiểm tra sản phẩm đã có trong giỏ của user này chưa
        var existing = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaKh == maKh && g.MaHh == maHh);

        if (existing != null)
        {
            existing.SoLuong += quantity;
            _context.GioHangs.Update(existing);
        }
        else
        {
            var item = new GioHang
            {
                MaKh = maKh,
                MaHh = maHh,
                SoLuong = quantity,
                NgayThem = DateTime.Now
            };
            await _context.GioHangs.AddAsync(item);
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã thêm vào giỏ hàng!";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Index()
    {
        var maKh = HttpContext.Session.GetString("MaKH");
        if (string.IsNullOrEmpty(maKh))
        {
            return RedirectToAction("Login", "Account");
        }

        var items = await _context.GioHangs
            .Where(g => g.MaKh == maKh)
            .Include(g => g.HangHoa)
            .ToListAsync();

        return View(items);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int maGh, int quantity)
    {
        var maKh = HttpContext.Session.GetString("MaKH");
        if (string.IsNullOrEmpty(maKh)) return RedirectToAction("Login", "Account");

        var item = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaGh == maGh && g.MaKh == maKh);
        if (item != null)
        {
            if (quantity <= 0)
            {
                _context.GioHangs.Remove(item);
            }
            else
            {
                item.SoLuong = quantity;
                _context.GioHangs.Update(item);
            }
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFromCart(int maGh)
    {
        var maKh = HttpContext.Session.GetString("MaKH");
        if (string.IsNullOrEmpty(maKh)) return RedirectToAction("Login", "Account");

        var item = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaGh == maGh && g.MaKh == maKh);
        if (item != null)
        {
            _context.GioHangs.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng";
        }
        return RedirectToAction("Index");
    }

}

