using Web_CuaHangCafe.Models;
using Web_CuaHangCafe.Helpers;
using Web_CuaHangCafe.Data;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;

namespace Web_CuaHangCafe.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<CartItem> Carts
        {
            get
            {
                var data = HttpContext.Session.Get<List<CartItem>>("Cart");

                if (data == null)
                {
                    data = new List<CartItem>();
                }

                return data;
            }
        }

        public IActionResult Index()
        {
            var cartItems = HttpContext.Session.Get<List<CartItem>>("Cart");

            if (cartItems != null && cartItems.Any())
            {
                decimal? tongTien = cartItems.Sum(p => p.DonGia * p.SoLuong);
                string totalCart = tongTien.Value.ToString("n0");
                ViewData["total"] = totalCart;
                return View(Carts);
            }
            else
            {
                ViewData["Message"] = "Không có sản phẩm trong giỏ hàng.";
                ViewData["total"] = "0";
                return View(Carts);
            }
        }

        public IActionResult Add(int id, int quantity)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return Json(new { Success = false, requireLogin = true, redirectUrl = Url.Action("Login", "Login") });
            }

            var myCart = Carts;
            var item = myCart.SingleOrDefault(p => p.MaSp == id);
            decimal? tongTien = 0;

            if (item == null)
            {
                var hangHoa = _context.TbSanPhams.SingleOrDefault(p => p.MaSanPham == id);

                item = new CartItem
                {
                    MaSp = id,
                    TenSp = hangHoa.TenSanPham,
                    DonGia = hangHoa.GiaBan.Value,
                    SoLuong = quantity,
                    AnhSp = hangHoa.HinhAnh
                };

                myCart.Add(item);
            }
            else
            {
                item.SoLuong += quantity;
            }



            HttpContext.Session.Set("Cart", myCart);

            int totalItems = myCart.Sum(x => x.SoLuong);

            return Json(new { Success = true, count = totalItems });
        }

        [HttpPost]
        public IActionResult Update([FromBody] List<UpdateQuantityRequest> updates)
        {
            if (updates == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid request.");
            }

            var cartItems = HttpContext.Session.Get<List<CartItem>>("Cart");

            if (cartItems != null)
            {
                foreach (var update in updates)
                {
                    var cartItemToUpdate = cartItems.Find(item => item.MaSp == update.ProductId);

                    if (cartItemToUpdate != null)
                    {
                        cartItemToUpdate.SoLuong = update.Quantity;
                    }
                }

                HttpContext.Session.Set("Cart", cartItems);

                decimal? totalAmount = 0;
                foreach (var item in cartItems)
                {
                    totalAmount += item.SoLuong * item.DonGia;
                }

                return Json(new { success = true, message = "Số lượng đã được cập nhật.", totalAmount = totalAmount, cartItems = cartItems });
            }

            return BadRequest("Invalid cart.");
        }

        public class UpdateQuantityRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }

        [HttpPost]
        public IActionResult Remove(int maSp)
        {
            try
            {
                var cartItems = HttpContext.Session.Get<List<CartItem>>("Cart");

                if (cartItems != null)
                {
                    var productToRemove = cartItems.SingleOrDefault(item => item.MaSp == maSp);

                    if (productToRemove != null)
                    {
                        cartItems.Remove(productToRemove);

                        HttpContext.Session.Set("Cart", cartItems);

                        decimal? totalAmount = 0;

                        foreach (var item in cartItems)
                        {
                            totalAmount += item.SoLuong * item.DonGia;
                        }

                        return Json(new { success = true, message = "Đã xoá sản phẩm.", totalAmount = totalAmount, cartItems = cartItems });
                    }
                    else
                    {
                        Console.WriteLine(maSp);
                        return Json(new { success = false, message = "Không có sản phẩm." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Giỏ hàng rỗng." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public IActionResult Checkout()
        {
            var cartItems = HttpContext.Session.Get<List<CartItem>>("Cart");


            if (cartItems != null && cartItems.Any())
            {
                decimal tongTien = (decimal)cartItems.Sum(p => p.DonGia * p.SoLuong);

                //Đọc discout từ session
                var discountStr = HttpContext.Session.GetString("DiscountAmount");
                decimal discount = decimal.TryParse(discountStr, out var d) ? d : 0;
                decimal tongSauGiam = tongTien - discount;

                ViewData["total"] = tongTien.ToString("n0");
                ViewData["discount"] = discount.ToString("n0");
                ViewData["tongSauGiam"] = tongSauGiam.ToString("n0");
                ViewData["appliedVoucher"] = HttpContext.Session.GetString("AppliedVoucher") ?? "";

                //string totalCart = tongTien.Value.ToString("n0");
                //ViewData["total"] = totalCart;
                return View(Carts);
            }
            else
            {
                return RedirectToAction("index", "home");
            }
        }

        public IActionResult Confirmation(string customerName, string phoneNumber, string address)
        {
            var cartItems = HttpContext.Session.Get<List<CartItem>>("Cart");

            if (cartItems == null || !cartItems.Any())
                return RedirectToAction("index", "home");

            Random random = new Random();
            Guid orderId = Guid.NewGuid();

            // ─── Tính tiền ───────────────────────────────────────────────
            decimal tongTien = cartItems.Sum(p => (decimal)(p.DonGia ?? 0) * p.SoLuong);

            var discountStr = HttpContext.Session.GetString("DiscountAmount");
            decimal soTienGiam = decimal.TryParse(discountStr, out var d) ? d : 0;
            decimal tongSauGiam = tongTien - soTienGiam;

            // ─── Lấy MaVoucher nếu có ────────────────────────────────────
            var appliedCode = HttpContext.Session.GetString("AppliedVoucher");
            int? maVoucher = null;
            if (!string.IsNullOrEmpty(appliedCode))
            {
                maVoucher = _context.tbVouchers
                    .Where(v => v.MaCode == appliedCode)
                    .Select(v => (int?)v.MaVoucher)
                    .FirstOrDefault();
            }

            // ─── Xử lý khách hàng ────────────────────────────────────────
            //var customer = _context.TbKhachHangs
            //    .FirstOrDefault(x => x.SdtkhachHang != null && x.SdtkhachHang == phoneNumber);

            //if (customer == null)
            //{
                var customer = new TbKhachHang
                {
                    Id = Guid.NewGuid(),   // ← dùng Guid mới trực tiếp, không dùng biến ngoài
                    TenKhachHang = customerName,
                    SdtkhachHang = phoneNumber,
                    DiaChi = address
                };
                _context.TbKhachHangs.Add(customer);
                _context.SaveChanges();
            //}
            //else
            //{
            //    customer.TenKhachHang = customerName;
            //    customer.DiaChi = address;
            //    _context.SaveChanges();
            //}

            // ─── Tạo hoá đơn ─────────────────────────────────────────────
            var order = new TbHoaDonBan
            {
                MaHoaDon = orderId,
                SoHoaDon = random.Next(1, 100000).ToString(),
                NgayBan = DateTime.Now,
                TongTien = tongSauGiam,
                CustomerId = customer.Id,   // ← luôn dùng customer.Id, tránh nhầm biến
                MaVoucher = maVoucher,
                TrangThai = "đã đặt",
                DiaChiGiaoHang = address,
                SoDienThoai = phoneNumber
            };

            _context.TbHoaDonBans.Add(order);
            _context.SaveChanges();

            // ─── Tạo chi tiết hoá đơn ────────────────────────────────────
            decimal tyLeGiam = tongTien > 0 ? soTienGiam / tongTien : 0;

            var chiTietList = cartItems.Select(cartItem =>
            {
                decimal giaBanGoc = (decimal)(cartItem.DonGia ?? 0);
                decimal giamGiaSanPham = Math.Round(giaBanGoc * cartItem.SoLuong * tyLeGiam, 0);

                return new TbChiTietHoaDonBan
                {
                    MaHoaDon = orderId,
                    MaSanPham = cartItem.MaSp,
                    GiaBan = cartItem.DonGia,
                    GiamGia = (int?)giamGiaSanPham,   // ← decimal thẳng, không cast lòng vòng
                    SoLuong = cartItem.SoLuong,
                    ThanhTien = cartItem.ThanhTien
                };
            }).ToList();

            _context.TbChiTietHoaDonBans.AddRange(chiTietList);  // ← AddRange thay vì Add từng cái
            _context.SaveChanges();

            // ─── Xoá session ─────────────────────────────────────────────
            HttpContext.Session.Remove("Cart");
            HttpContext.Session.Remove("AppliedVoucher");
            HttpContext.Session.Remove("DiscountAmount");

            return RedirectToAction("success");
        }
        public IActionResult Success()
        {
            return View();
        }

        // THAY TOÀN BỘ method GetVouchers() bằng:
        public IActionResult GetVouchers()
        {
            var vouchers = _context.tbVouchers
                .Where(v => v.NgayHetHan >= DateTime.Now && v.IsActive == true)
                .ToList();

            var result = vouchers.Select(v => new {
                maCode = v.MaCode,
                phanTramGiam = v.PhanTramGiam,
                soTienGiamToiDa = v.SoTienGiamToiDa,
                ngayHetHan = v.NgayHetHan
            });

            return Json(new { success = true, vouchers = result });
        }
        public IActionResult ApplyVoucher(string maCode)
        {
            var cartItems = HttpContext.Session.Get<List<CartItem>>("Cart");
            if (cartItems == null || !cartItems.Any())
                return Json(new { success = false, message = "Giỏ hàng trống" });

            // LỖI CŨ: thiếu v.MaCode == maCode → lấy bừa voucher đầu tiên
            var voucher = _context.tbVouchers.FirstOrDefault(v =>
                v.MaCode == maCode &&
                v.IsActive == true &&
                v.NgayHetHan >= DateTime.Now);

            if (voucher == null)
                return Json(new { success = false, message = "Voucher không hợp lệ hoặc đã hết hạn" });

            decimal tongTien = (decimal)cartItems.Sum(p => p.DonGia * p.SoLuong);
            decimal soTienGiam = tongTien * voucher.PhanTramGiam / 100;

            if (voucher.SoTienGiamToiDa.HasValue && soTienGiam > voucher.SoTienGiamToiDa.Value)
                soTienGiam = voucher.SoTienGiamToiDa.Value;

            decimal tongSauGiam = tongTien - soTienGiam;

            HttpContext.Session.SetString("AppliedVoucher", maCode);
            HttpContext.Session.SetString("DiscountAmount", soTienGiam.ToString());

            return Json(new
            {
                success = true,
                tongTien = tongTien.ToString("n0"),
                soTienGiam = soTienGiam.ToString("n0"),
                tongSauGiam = tongSauGiam.ToString("n0"),
                phanTramGiam = voucher.PhanTramGiam
            });
        }

    }
}