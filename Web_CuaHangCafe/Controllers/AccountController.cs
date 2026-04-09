using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_CuaHangCafe.Data;
using Web_CuaHangCafe.Models;

namespace Web_CuaHangCafe.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─── KIỂM TRA ĐĂNG NHẬP ──────────────────────────────────
        private TbKhachHang? GetCurrentUser()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email)) return null;
            return _context.TbKhachHangs.FirstOrDefault(k => k.Email == email);
        }

        // ─── HỒ SƠ CÁ NHÂN ───────────────────────────────────────
        public IActionResult Profile()
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Login");
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(TbKhachHang model)
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Login");

            // Chỉ cập nhật các field được phép, không cho đổi Email/MatKhau ở đây
            ModelState.Remove("Email");
            ModelState.Remove("MatKhau");

            if (ModelState.IsValid)
            {
                user.TenKhachHang = model.TenKhachHang;
                user.SdtkhachHang = model.SdtkhachHang;
                user.DiaChi = model.DiaChi;
                _context.SaveChanges();

                TempData["Success"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Profile");
            }

            return View(model);
        }

        // ─── ĐỔI MẬT KHẨU ────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string matKhauCu, string matKhauMoi, string xacNhan)
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Login");

            if (user.MatKhau != matKhauCu)
            {
                TempData["ErrorPass"] = "Mật khẩu cũ không đúng.";
                return RedirectToAction("Profile");
            }

            if (matKhauMoi != xacNhan)
            {
                TempData["ErrorPass"] = "Mật khẩu xác nhận không khớp.";
                return RedirectToAction("Profile");
            }

            if (matKhauMoi.Length < 6)
            {
                TempData["ErrorPass"] = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return RedirectToAction("Profile");
            }

            user.MatKhau = matKhauMoi;
            _context.SaveChanges();

            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Profile");
        }

        // ─── THEO DÕI ĐƠN HÀNG ───────────────────────────────────
        public IActionResult Orders()
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Login");

            var orders = _context.TbHoaDonBans
                .Where(h => h.CustomerId == user.Id)
                .OrderByDescending(h => h.NgayBan)
                .ToList();

            return View(orders);
        }

        // ─── CHI TIẾT ĐƠN HÀNG ───────────────────────────────────
        public IActionResult OrderDetail(string id)
        {
            var user = GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Login");

            var maHoaDon = Guid.Parse(id);

            // Đảm bảo đơn hàng thuộc về user đang đăng nhập
            var hoaDon = _context.TbHoaDonBans
                .FirstOrDefault(h => h.MaHoaDon == maHoaDon && h.CustomerId == user.Id);

            if (hoaDon == null) return NotFound();

            var chiTiet = _context.TbChiTietHoaDonBans
                .Include(x => x.MaSanPhamNavigation)
                .Where(x => x.MaHoaDon == maHoaDon)
                .ToList();

            ViewBag.HoaDon = hoaDon;
            ViewBag.ChiTiet = chiTiet;
            return View();
        }
    }
}