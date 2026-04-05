using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_CuaHangCafe.Data;
using Web_CuaHangCafe.Models;
using Web_CuaHangCafe.Models.Authentication;
using X.PagedList;

namespace Web_CuaHangCafe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Bill")]
    public class BillController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("")]
        [Route("Index")]
        [Authentication]
        public IActionResult Index(int? page, string? trangThai)
        {
            int pageSize = 15;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            ViewBag.TrangThai = trangThai;

            var query = _context.TbHoaDonBans
                .Include(x => x.Customer)
                .AsNoTracking()
                .OrderByDescending(x => x.NgayBan)
                .AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
            {
                query = query.Where(x => x.TrangThai == trangThai);
            }



            var listItem = query.ToList();
            PagedList<TbHoaDonBan> pagedListItem = new PagedList<TbHoaDonBan>(listItem, pageNumber, pageSize);

            return View(pagedListItem);
        }

        [Route("Search")]
        [Authentication]
        [HttpGet]
        public IActionResult Search(int? page, string search)
        {
            int pageSize = 15;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            ViewBag.search = search;

            var listItem = _context.TbHoaDonBans
                .Include(x => x.Customer)
                .AsNoTracking()
                .Where(x => x.NgayBan.Value.ToString().Contains(search)
                         || x.SoDienThoai.Contains(search)
                         || x.SoHoaDon.Contains(search))
                .OrderByDescending(x => x.NgayBan)
                .ToList();

            PagedList<TbHoaDonBan> pagedListItem = new PagedList<TbHoaDonBan>(listItem, pageNumber, pageSize);
            return View(pagedListItem);
        }

        [Route("Details/{id}")]
        [Authentication]
        [HttpGet]
        public IActionResult Details(string id)
        {
            var maHoaDon = Guid.Parse(id);

            var hoaDon = _context.TbHoaDonBans
                .Include(x => x.Customer)
                .FirstOrDefault(x => x.MaHoaDon == maHoaDon);

            if (hoaDon == null) return NotFound();

            var chiTiet = _context.TbChiTietHoaDonBans
                .Include(x => x.MaSanPhamNavigation)
                .Where(x => x.MaHoaDon == maHoaDon)
                .ToList();

            ViewBag.HoaDon = hoaDon;
            ViewBag.ChiTiet = chiTiet;

            return View();
        }

        [Route("Approve/{id}")]
        [Authentication]
        [HttpPost]
        public IActionResult Approve(string id)
        {
            var hoaDon = _context.TbHoaDonBans.Find(Guid.Parse(id));
            if (hoaDon == null)
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });

            if (hoaDon.TrangThai != "đã đặt")
                return Json(new { success = false, message = "Đơn hàng đã được xử lý trước đó" });

            hoaDon.TrangThai = "đã xong";
            _context.SaveChanges();

            return Json(new { success = true, message = "Duyệt đơn hàng thành công" });
        }

        [Route("UpdateStatus/{id}")]
        [Authentication]
        [HttpPost]
        public IActionResult UpdateStatus(string id, string trangThai)
        {
            var validStatuses = new[] { "đã xong", "đang giao", "đã giao", "đã hủy" };
            if (!validStatuses.Contains(trangThai)) return Json(new { success = false, message = "Trạng thái không hợp lệ" });

            var hoaDon = _context.TbHoaDonBans.Find(Guid.Parse(id));
            if (hoaDon == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng" });

            hoaDon.TrangThai = trangThai;
            _context.SaveChanges();
            return Json(new { success = true, message = $"Đã cập nhật : {trangThai}" });
        }
    }
}
