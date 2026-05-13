using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WebEcommer.Models;
using WebEcommer.Models.ViewModels;
using WebEcommer.Data;
using Microsoft.EntityFrameworkCore;

namespace WebEcommer.Controllers
{
    public class HomeController : Controller
    {
        private readonly EcommerDemoContext _context;

        public HomeController(EcommerDemoContext context)
        {
            _context = context;
        }

        public IActionResult Index(int page = 1, int record = 9)
        {
            if (page < 1) page = 1;
            if (record <= 0) record = 9;

            var totalCount = _context.HangHoas.Count();
            var totalPages = (int)System.Math.Ceiling((double)totalCount / record);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var products = _context.HangHoas
                .Skip((page - 1) * record)
                .Take(record)
                .ToList();

            var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "image", "Hinh", "HangHoa");
            var files = Directory.Exists(imageFolder) ? Directory.GetFiles(imageFolder).Select(Path.GetFileName).ToArray() : System.Array.Empty<string>();

            var vm = products.Select(p => new ProductViewModel
            {
                MaHh = p.MaHh,
                TenHh = p.TenHh,
                MoTa = string.IsNullOrEmpty(p.MoTa) ? p.MoTaDonVi : p.MoTa,
                Hinh = p.Hinh,
                ImageUrl = Url.Content($"~/image/Hinh/HangHoa/{(string.IsNullOrEmpty(p.Hinh) ? "default.jpg" : p.Hinh)}")
            }).ToList();

            // truyền thông tin phân trang cho view
            ViewBag.Page = page;
            ViewBag.Record = record;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
