using System.ComponentModel.DataAnnotations;

namespace Web_CuaHangCafe.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string TenKhachHang { get; set; }

        [Required]
        public string SdtkhachHang { get; set; }

        public string? DiaChi { get; set; }
        [Required]
        public string Email { get; set; }

        [Required]
        public string MatKhau { get; set; }

        [Compare("MatKhau")]
        public string XacNhanMatKhau { get; set; }
    }
}
