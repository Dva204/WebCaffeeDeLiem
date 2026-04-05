using System;
using System.Collections.Generic;

namespace Web_CuaHangCafe.Models;

public partial class TbKhachHang
{
    public Guid Id { get; set; }

    public string TenKhachHang { get; set; } = null!;

    public string? SdtkhachHang { get; set; }

    public string? DiaChi { get; set; }

    public string? Email { get; set; } 
    public string? MatKhau { get; set; } 
    public virtual ICollection<TbHoaDonBan> TbHoaDonBans { get; set; } = new List<TbHoaDonBan>();
}
