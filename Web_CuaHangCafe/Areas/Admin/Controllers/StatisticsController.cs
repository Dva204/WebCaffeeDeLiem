using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_CuaHangCafe.Data;
using Web_CuaHangCafe.Models.Authentication;

namespace Web_CuaHangCafe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Statistics")]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Route("")]
        [Route("Index")]
        [Authentication]
        public IActionResult Index(int? nam, int? thang)
        {
            // Mặc định: tháng và năm hiện tại
            int selectedYear = nam ?? DateTime.Now.Year;
            int selectedMonth = thang ?? DateTime.Now.Month;

            ViewBag.SelectedYear = selectedYear;
            ViewBag.SelectedMonth = selectedMonth;

            // ── Danh sách năm có đơn hàng để render dropdown ──────
            var years = _context.TbHoaDonBans
                .Where(h => h.NgayBan != null && h.TrangThai != "đã hủy")
                .Select(h => h.NgayBan!.Value.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            if (!years.Contains(selectedYear)) years.Insert(0, selectedYear);
            ViewBag.Years = years;

            // ── Tổng quan tháng được chọn ──────────────────────────
            var ordersThisMonth = _context.TbHoaDonBans
                .Where(h => h.NgayBan != null
                         && h.NgayBan.Value.Year == selectedYear
                         && h.NgayBan.Value.Month == selectedMonth
                         && h.TrangThai != "đã hủy")
                .ToList();

            ViewBag.TongDoanhThu = ordersThisMonth.Sum(h => h.TongTien ?? 0);
            ViewBag.TongDonHang = ordersThisMonth.Count;
            ViewBag.DonHoanThanh = ordersThisMonth.Count(h => h.TrangThai == "đã giao");
            ViewBag.DonChorDuyet = ordersThisMonth.Count(h => h.TrangThai == "chờ duyệt");

            // ── Doanh thu theo ngày trong tháng (cho biểu đồ) ──────
            var doanhThuTheoNgay = ordersThisMonth
                .GroupBy(h => h.NgayBan!.Value.Day)
                .Select(g => new { Ngay = g.Key, DoanhThu = g.Sum(h => h.TongTien ?? 0) })
                .OrderBy(x => x.Ngay)
                .ToList();

            // Tạo mảng đủ 28-31 ngày (ngày không có đơn = 0)
            int daysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth);
            var labels = Enumerable.Range(1, daysInMonth).Select(d => d.ToString()).ToList();
            var dataChart = Enumerable.Range(1, daysInMonth)
                .Select(d => doanhThuTheoNgay.FirstOrDefault(x => x.Ngay == d)?.DoanhThu ?? 0)
                .ToList();

            ViewBag.ChartLabels = System.Text.Json.JsonSerializer.Serialize(labels);
            ViewBag.ChartData = System.Text.Json.JsonSerializer.Serialize(dataChart);

            // ── Doanh thu 12 tháng của năm được chọn (cho biểu đồ cột) ──
            var doanhThuTheoThang = _context.TbHoaDonBans
                .Where(h => h.NgayBan != null
                         && h.NgayBan.Value.Year == selectedYear
                         && h.TrangThai != "đã hủy")
                .GroupBy(h => h.NgayBan!.Value.Month)
                .Select(g => new { Thang = g.Key, DoanhThu = g.Sum(h => h.TongTien ?? 0) })
                .ToList();

            var monthLabels = new[] { "T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12" };
            var monthData = Enumerable.Range(1, 12)
                .Select(m => doanhThuTheoThang.FirstOrDefault(x => x.Thang == m)?.DoanhThu ?? 0)
                .ToList();

            ViewBag.MonthLabels = System.Text.Json.JsonSerializer.Serialize(monthLabels);
            ViewBag.MonthData = System.Text.Json.JsonSerializer.Serialize(monthData);

            // ── Top 5 sản phẩm bán chạy tháng được chọn ───────────
            var maHoaDonThang = ordersThisMonth.Select(h => h.MaHoaDon).ToList();

            var top5 = _context.TbChiTietHoaDonBans
                .Where(ct => maHoaDonThang.Contains(ct.MaHoaDon))
                .Include(ct => ct.MaSanPhamNavigation)
                .GroupBy(ct => ct.MaSanPhamNavigation!.TenSanPham)
                .Select(g => new {
                    TenSanPham = g.Key,
                    TongSoLuong = g.Sum(x => x.SoLuong),
                    TongTien = g.Sum(x => x.ThanhTien ?? 0)
                })
                .OrderByDescending(x => x.TongSoLuong)
                .Take(5)
                .ToList();

            ViewBag.Top5 = top5;

            // ── Thống kê trạng thái đơn hàng tháng (cho pie chart) ─
            var trangThaiData = ordersThisMonth
                .GroupBy(h => h.TrangThai ?? "chưa rõ")
                .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
                .ToList();

            ViewBag.PieLabels = System.Text.Json.JsonSerializer.Serialize(trangThaiData.Select(x => x.TrangThai).ToList());
            ViewBag.PieData = System.Text.Json.JsonSerializer.Serialize(trangThaiData.Select(x => x.SoLuong).ToList());

            return View();
        }

        // ── API: Doanh thu theo tháng (AJAX) ──────────────────────
        [Route("GetMonthData")]
        [Authentication]
        public IActionResult GetMonthData(int nam, int thang)
        {
            int daysInMonth = DateTime.DaysInMonth(nam, thang);

            var orders = _context.TbHoaDonBans
                .Where(h => h.NgayBan != null
                         && h.NgayBan.Value.Year == nam
                         && h.NgayBan.Value.Month == thang
                         && h.TrangThai != "đã hủy")
                .ToList();

            var data = Enumerable.Range(1, daysInMonth)
                .Select(d => orders
                    .Where(h => h.NgayBan!.Value.Day == d)
                    .Sum(h => h.TongTien ?? 0))
                .ToList();

            return Json(new
            {
                labels = Enumerable.Range(1, daysInMonth).Select(d => d.ToString()),
                data,
                tongDoanhThu = orders.Sum(h => h.TongTien ?? 0),
                tongDonHang = orders.Count,
                donHoanThanh = orders.Count(h => h.TrangThai == "đã giao"),
                donChorDuyet = orders.Count(h => h.TrangThai == "chờ duyệt")
            });
        }
    }
}