using System.ComponentModel.DataAnnotations;

namespace Web_CuaHangCafe.Models
{
    public class TbVoucher
    {
        [Key]
        public  int MaVoucher { set; get; }
        public string MaCode { set; get; }
        public decimal PhanTramGiam { set; get; }
        public decimal? SoTienGiamToiDa { set; get; }
        public DateTime NgayHetHan { set; get; }
        public bool? IsActive { set; get; }
    }
}
