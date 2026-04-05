using System;
using System.Collections.Generic;

namespace Web_CuaHangCafe.Models;

public partial class TbHoaDonBan
{
    public Guid MaHoaDon { get; set; }

    public string SoHoaDon { get; set; } = null!;

    public DateTime? NgayBan { get; set; }

    public decimal? TongTien { get; set; }

    public Guid CustomerId { get; set; }

    public virtual TbKhachHang Customer { get; set; } = null!;
    public int? MaVoucher { get; set; }

    public string? TrangThai { get; set; }
    public string? DiaChiGiaoHang {  get; set; }
    public string? SoDienThoai { get; set; }
    public virtual ICollection<TbChiTietHoaDonBan> TbChiTietHoaDonBans { get; set; } = new List<TbChiTietHoaDonBan>();
}
