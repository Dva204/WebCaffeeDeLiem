using Microsoft.AspNetCore.Mvc;
using Web_CuaHangCafe.Data;
using Web_CuaHangCafe.Models;
using Web_CuaHangCafe.ViewModels;

namespace Web_CuaHangCafe.Controllers
{
    public class LoginController : Controller
    {

        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.TbKhachHangs
                .FirstOrDefault(x => x.Email == model.Email);

            if (user == null)
            {
                ViewBag.Error = "Email không hợp lệ!";
                return View(model);
            }

            if (user.MatKhau != model.MatKhau)
            {
                ViewBag.Error = "Mật khẩu không chính xác!";
                return View(model);
            }

            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserName", user.TenKhachHang);

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var checkEmail = _context.TbKhachHangs
                .FirstOrDefault(x => x.Email == model.Email);

            if (checkEmail != null)
            {
                ViewBag.Error = "Email đã tồn tại !";
                return View(model);
            }

            var user = new TbKhachHang
            {
                Id = Guid.NewGuid(),
                TenKhachHang = model.TenKhachHang,
                SdtkhachHang = model.SdtkhachHang,
                DiaChi = "", // hoặc null
                Email = model.Email,
                MatKhau = model.MatKhau
            };

            _context.TbKhachHangs.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
