using Web_CuaHangCafe.Data;
using Web_CuaHangCafe.Models;
using Microsoft.AspNetCore.Mvc;

namespace Web_CuaHangCafe.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(TbPhanHoi phanHoi)
        {
            // Bỏ validate KhachHangId vì sẽ tự gán bên dưới
            ModelState.Remove("KhachHangId");

            if (ModelState.IsValid)
            {
                try
                {
                    // Nếu đang đăng nhập thì gắn KhachHangId
                    var userEmail = HttpContext.Session.GetString("UserEmail");
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        var khachHang = _context.TbKhachHangs
                            .FirstOrDefault(k => k.Email == userEmail);
                        if (khachHang != null)
                            phanHoi.KhachHangId = khachHang.Id;
                    }

                    _context.TbPhanHois.Add(phanHoi);
                    _context.SaveChanges();

                    TempData["Status"] = "success";
                    TempData["Message"] = "Gửi thành công";
                    return RedirectToAction("index");
                }
                catch (Exception ex)
                {
                    TempData["Status"] = "error";
                    TempData["Message"] = "Không gửi được lời nhắn: " + ex.Message;
                }
            }

            return View(phanHoi);
        }
    }
}