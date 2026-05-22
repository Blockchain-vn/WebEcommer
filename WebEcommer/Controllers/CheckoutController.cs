using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebEcommer.Data;

namespace WebEcommer.Controllers;

public class CheckoutController : Controller
{
    private readonly EcommerDemoContext _context;

    public CheckoutController(EcommerDemoContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Index(int[] selectedItems)
    {
        var maKh = HttpContext.Session.GetString("MaKH");
        if (string.IsNullOrEmpty(maKh))
        {
            return RedirectToAction("Login", "Account");
        }

        if (selectedItems == null || !selectedItems.Any())
        {
            TempData["Error"] = "Vui lòng chọn ít nhất 1 sản phẩm để thanh toán.";
            return RedirectToAction("Index", "Cart");
        }

        var items = await _context.GioHangs
            .Include(g => g.HangHoa)
            .Where(g => g.MaKh == maKh && selectedItems.Contains(g.MaGh))
            .ToListAsync();

        return View(items);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(int[] selectedItems)
    {
        var maKh = HttpContext.Session.GetString("MaKH");
        if (string.IsNullOrEmpty(maKh))
        {
            return RedirectToAction("Login", "Account");
        }

        if (selectedItems == null || !selectedItems.Any())
        {
            return RedirectToAction("Index", "Cart");
        }

        var items = await _context.GioHangs
            .Include(g => g.HangHoa)
            .Where(g => g.MaKh == maKh && selectedItems.Contains(g.MaGh))
            .ToListAsync();

        if (!items.Any()) return RedirectToAction("Index", "Cart");

        double tongTien = items.Sum(i => i.SoLuong * (i.HangHoa?.DonGia ?? 0));
        
        // Mockup: không lưu vào DB, chỉ tạo mã đơn ảo và KHÔNG xoá giỏ hàng ở bước này
        long mockOrderId = DateTime.Now.Ticks % 1000000; // random id

        return RedirectToAction("MomoMockup", new { orderId = mockOrderId, amount = tongTien });
    }

    public IActionResult MomoMockup(long orderId, double amount)
    {
        ViewBag.OrderId = orderId;
        ViewBag.Amount = amount;
        return View();
    }
    
    public async Task<IActionResult> PaymentSuccess()
    {
        var maKh = HttpContext.Session.GetString("MaKH");
        if (!string.IsNullOrEmpty(maKh))
        {
            var items = await _context.GioHangs.Where(g => g.MaKh == maKh).ToListAsync();
            _context.GioHangs.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Success");
    }

    public IActionResult CancelPayment()
    {
        TempData["Error"] = "Giao dịch không thành công";
        return RedirectToAction("Index", "Cart");
    }
    
    public IActionResult Success()
    {
        return View();
    }
}
