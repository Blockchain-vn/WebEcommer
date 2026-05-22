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
                .OrderByDescending(h => h.MaHh)
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
            ViewBag.IsSearch = false;

            return View(vm);
        }

        [HttpGet]
        public IActionResult Search(string keyword, int page = 1, int record = 9)
        {
            // Validate input - TC01: Tìm kiếm sản phẩm theo tên
            if (string.IsNullOrWhiteSpace(keyword))
            {
                TempData["Error"] = "Vui lòng nhập từ khóa tìm kiếm!";
                return RedirectToAction("Index");
            }

            // Trim và validate độ dài
            keyword = keyword.Trim();
            if (keyword.Length < 1)
            {
                TempData["Error"] = "Từ khóa tìm kiếm không hợp lệ!";
                return RedirectToAction("Index");
            }

            if (page < 1) page = 1;
            if (record <= 0) record = 9;

            // TC01 & TC02: Tìm kiếm theo tên sản phẩm
            var query = _context.HangHoas
                .Where(h => h.TenHh.Contains(keyword))
                .OrderByDescending(h => h.MaHh)
                .AsQueryable();

            var totalCount = query.Count();

            // TC03: Tìm kiếm không có kết quả
            if (totalCount == 0)
            {
                TempData["Info"] = $"Không tìm thấy sản phẩm với từ khóa '{keyword}'.";
                return RedirectToAction("Index");
            }

            var totalPages = (int)System.Math.Ceiling((double)totalCount / record);
            if (totalPages == 0) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var products = query
                .Skip((page - 1) * record)
                .Take(record)
                .ToList();

            var vm = products.Select(p => new ProductViewModel
            {
                MaHh = p.MaHh,
                TenHh = p.TenHh,
                MoTa = string.IsNullOrEmpty(p.MoTa) ? p.MoTaDonVi : p.MoTa,
                Hinh = p.Hinh,
                ImageUrl = Url.Content($"~/image/Hinh/HangHoa/{(string.IsNullOrEmpty(p.Hinh) ? "default.jpg" : p.Hinh)}")
            }).ToList();

            ViewBag.Page = page;
            ViewBag.Record = record;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.Keyword = keyword;
            ViewBag.IsSearch = true;

            TempData["Success"] = $"Tìm thấy {totalCount} sản phẩm với từ khóa '{keyword}'.";
            return View("Index", vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Services()
        {
            return View();
        }

        public IActionResult Team()
        {
            return View();
        }

        public IActionResult Testimonial()
        {
            return View();
        }

        public IActionResult Page404()
        {
            return View();
        }

        public IActionResult Contact()
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
