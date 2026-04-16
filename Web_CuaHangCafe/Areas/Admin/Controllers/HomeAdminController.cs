using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Web_CuaHangCafe.Data;
using Web_CuaHangCafe.Models;
using Web_CuaHangCafe.Models.Authentication;
using Web_CuaHangCafe.ViewModels;
using X.PagedList;

namespace Web_CuaHangCafe.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("HomeAdmin")]
    public class HomeAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        IWebHostEnvironment hostEnvironment;

        public HomeAdminController(ApplicationDbContext context, IWebHostEnvironment hc)
        {
            _context = context;
            hostEnvironment = hc;
        }

        [Route("")]
        [Authentication]
        public IActionResult Index(int? page)
        {
            int pageSize = 30;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            var listItem = (from product in _context.TbSanPhams
                            join type in _context.TbNhomSanPhams on product.MaNhomSp equals type.MaNhomSp
                            orderby product.MaSanPham
                            select new ProductViewModel
                            {
                                MaSanPham = product.MaSanPham,
                                TenSanPham = product.TenSanPham,
                                GiaBan = product.GiaBan,
                                MoTa = product.MoTa,
                                HinhAnh = product.HinhAnh,
                                GhiChu = product.GhiChu,
                                LoaiSanPham = type.TenNhomSp
                            }).ToList();

            PagedList<ProductViewModel> pagedListItem = new PagedList<ProductViewModel>(listItem, pageNumber, pageSize);

            return View(pagedListItem);
        }

        [Route("Search")]
        [Authentication]
        [HttpGet]
        public IActionResult Search(int? page, string search)
        {
            int pageSize = 30;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;

            search = search.ToLower();
            ViewBag.search = search;

            var listItem = _context.TbSanPhams.AsNoTracking().Where(x => x.TenSanPham.ToLower().Contains(search)).OrderBy(x => x.MaSanPham).ToList();
            PagedList<TbSanPham> pagedListItem = new PagedList<TbSanPham>(listItem, pageNumber, pageSize);

            return View(pagedListItem);
        }

        [Route("Create")]
        [Authentication]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.MaNhomSp = new SelectList(_context.TbNhomSanPhams.ToList(), "MaNhomSp", "TenNhomSp");

            return View();
        }

        [Route("Create")]
        [Authentication]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateProductViewModel createProduct)
        {
            if (ModelState.IsValid)
            {
                string fileName = "";

                if (createProduct.HinhAnh != null)
                {
                    string uploadFolder = Path.Combine(hostEnvironment.WebRootPath, "img", "products");
                    fileName = Guid.NewGuid().ToString() + "_" + createProduct.HinhAnh.FileName;
                    string filePath = Path.Combine(uploadFolder, fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        createProduct.HinhAnh.CopyTo(stream);
                    }
                }

                var product = new TbSanPham
                {
                    TenSanPham = createProduct.TenSanPham,
                    GiaBan = createProduct.GiaBan,
                    MoTa = createProduct.MoTa,
                    HinhAnh = fileName,
                    GhiChu = createProduct.GhiChu,
                    MaNhomSp = createProduct.MaLoaiSanPham
                };

                _context.TbSanPhams.Add(product);
                _context.SaveChanges();
                TempData["Message"] = "Thêm sản phẩm thành công";

                return RedirectToAction("Index", "HomeAdmin");
            }

            ViewBag.MaNhomSp = new SelectList(_context.TbNhomSanPhams.ToList(), "MaNhomSp", "TenNhomSp", createProduct.MaLoaiSanPham);
            return View(createProduct);
        }

        [Route("Details")]
        [Authentication]
        [HttpGet]
        public IActionResult Details(int id, string name)
        {
            var productItem = (from product in _context.TbSanPhams
                            join type in _context.TbNhomSanPhams on product.MaNhomSp equals type.MaNhomSp
                            where product.MaSanPham == id
                            select new ProductViewModel
                            {
                                MaSanPham = product.MaSanPham,
                                TenSanPham = product.TenSanPham,
                                GiaBan = product.GiaBan,
                                MoTa = product.MoTa,
                                HinhAnh = product.HinhAnh,
                                GhiChu = product.GhiChu,
                                LoaiSanPham = type.TenNhomSp
                            }).SingleOrDefault();

            ViewBag.name = name;

            return View(productItem);
        }

        [Route("Edit")]
        [Authentication]
        [HttpGet]
        public IActionResult Edit(int id, string name)
        {
            var sanPham = _context.TbSanPhams.Find(id);
            if (sanPham == null)
            {
                return NotFound();
            }

            var viewModel = new CreateProductViewModel
            {
                MaSanPham = sanPham.MaSanPham,
                TenSanPham = sanPham.TenSanPham,
                GiaBan = sanPham.GiaBan,
                MoTa = sanPham.MoTa,
                GhiChu = sanPham.GhiChu,
                MaLoaiSanPham = sanPham.MaNhomSp
            };

            ViewBag.MaNhomSp = new SelectList(_context.TbNhomSanPhams.ToList(), "MaNhomSp", "TenNhomSp", sanPham.MaNhomSp);
            ViewBag.name = name;

            return View(viewModel);
        }

        [Route("Edit")]
        [Authentication]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CreateProductViewModel createProduct)
        {
            // Validate that the category exists
            var categoryExists = _context.TbNhomSanPhams.Any(x => x.MaNhomSp == createProduct.MaLoaiSanPham);
            if (!categoryExists)
            {
                ModelState.AddModelError(nameof(createProduct.MaLoaiSanPham), "Loại sản phẩm không hợp lệ");
                ViewBag.MaNhomSp = new SelectList(_context.TbNhomSanPhams.ToList(), "MaNhomSp", "TenNhomSp", createProduct.MaLoaiSanPham);
                ViewBag.name = createProduct.TenSanPham;
                return View(createProduct);
            }

            // Retrieve the existing product instead of creating new
            var product = _context.TbSanPhams.Find(createProduct.MaSanPham);
            if (product == null)
            {
                return NotFound();
            }

            // Update only the fields from the form
            product.TenSanPham = createProduct.TenSanPham;
            product.GiaBan = createProduct.GiaBan;
            product.MoTa = createProduct.MoTa;
            product.GhiChu = createProduct.GhiChu;
            product.MaNhomSp = createProduct.MaLoaiSanPham;

            // Handle image file if provided
            if (createProduct.HinhAnh != null && createProduct.HinhAnh.Length > 0)
            {
                string uploadFolder = Path.Combine(Path.Combine(hostEnvironment.WebRootPath, "img"), "products");
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + createProduct.HinhAnh.FileName;
                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    createProduct.HinhAnh.CopyTo(stream);
                }

                product.HinhAnh = uniqueFileName;
            }

            _context.SaveChanges();
            TempData["Message"] = "Sửa sản phẩm thành công";
            return RedirectToAction("Index", "HomeAdmin");
        }

        [Route("Delete")]
        [Authentication]
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var product = _context.TbSanPhams.Find(id);
            if (product == null)
            {
                TempData["Message"] = "Sản phẩm không tồn tại";
                return RedirectToAction("Index");
            }

            var hasInvoiceDetails = _context.TbChiTietHoaDonBans.Any(x => x.MaSanPham == id);
            if (hasInvoiceDetails)
            {
                TempData["Message"] = "Không xoá được sản phẩm vì đã có trong hóa đơn";
                return RedirectToAction("Index");
            }

            // Xóa file ảnh nếu tồn tại
            if (!string.IsNullOrEmpty(product.HinhAnh))
            {
                string uploadFolder = Path.Combine(hostEnvironment.WebRootPath, "img", "products");
                string filePath = Path.Combine(uploadFolder, product.HinhAnh);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.TbSanPhams.Remove(product);
            _context.SaveChanges();

            TempData["Message"] = "Sản phẩm đã được xoá";

            return RedirectToAction("Index", "HomeAdmin");
        }
    }
}
