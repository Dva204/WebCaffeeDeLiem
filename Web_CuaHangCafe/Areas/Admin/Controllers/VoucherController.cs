using Microsoft.AspNetCore.Mvc;
using Web_CuaHangCafe.Data;
using Web_CuaHangCafe.Models;
using Web_CuaHangCafe.Models.Authentication;
using X.PagedList;

namespace Web_CuaHangCafe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Voucher")]
    public class VoucherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VoucherController(ApplicationDbContext context)
        {
            _context = context;
        }

        //Danh sách
        [Route("")]
        [Route("Index")]
        [Authentication]
        public IActionResult Index(int? page)
        {
            int pageSize = 15;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            var list = _context.tbVouchers.OrderByDescending(v=>v.NgayHetHan).ToList();

            var paged = new PagedList<TbVoucher>(list, pageNumber, pageSize);
            return View(paged);
        }

        //Thêm mới
        [Route("Create")]
        [Authentication]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Route("Create")]
        [Authentication]
        [HttpPost]
        public IActionResult Create(TbVoucher model)
        {
            if(_context.tbVouchers.Any(v => v.MaCode == model.MaCode))
            {
                ModelState.AddModelError("MaCode", "Mã Voucher này đã tồn tại");
                return View(model);
            }

            if(ModelState.IsValid)
            {
                _context.tbVouchers.Add(model);
                _context.SaveChanges();
                TempData["success"] = "Thêm voucher thành công";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        //Xem chi tiết
        [Route("Details/{id}")]
        [Authentication]
        [HttpGet]
        public IActionResult Details(int id)
        {
            var voucher = _context.tbVouchers.Find(id);
            if(voucher == null) return NotFound();
            return View(voucher);
        }

        //Xóa
        [Route("Delete/{id}")]
        [Authentication]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var voucher = _context.tbVouchers.Find(id);
            if (voucher == null)
                return Json(new { success = false, message = "Không tìm thấy voucher" });

            // Kiểm tra voucher đã được dùng trong hóa đơn chưa
            bool daDung = _context.TbHoaDonBans.Any(h => h.MaVoucher == id);
            if (daDung)
                return Json(new { success = false, message = "Voucher đã được sử dụng, không thể xóa" });

            _context.tbVouchers.Remove(voucher);
            _context.SaveChanges();
            return Json(new { success = true, message = "Xóa voucher thành công" });
        }

        //Bật tắt
        [Route("ToggleActive/{id}")]
        [Authentication]
        [HttpPost]
        public IActionResult ToggleActive(int id)
        {
            var voucher = _context.tbVouchers.Find(id);
            if (voucher == null)
                return Json(new { success = false, message = "Không tìm thấy voucher" });

            voucher.IsActive = !(voucher.IsActive ?? false);
            _context.SaveChanges();

            return Json(new { success = true, isActive = voucher.IsActive });
        }
    }
}
