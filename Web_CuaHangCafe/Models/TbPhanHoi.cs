using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web_CuaHangCafe.Models;

public partial class TbPhanHoi
{
    public int MaPhanHoi { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
    [MaxLength(50)]
    public string TieuDe { get; set; } = null!;

    [MaxLength(10)]
    public string? SoDienThoai { get; set; }

    public string? NoiDung { get; set; }

    public Guid? KhachHangId { get; set; }  // ← thêm dòng này
}