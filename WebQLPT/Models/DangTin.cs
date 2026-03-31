using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace WebQLPT.Models
{
    public class DangTin
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [Display(Name = "Tiêu đề")]
        public string TieuDe { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        [Display(Name = "Nội dung")]
        public string NoiDung { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải là số dương")]
        [Display(Name = "Giá")]
        public decimal Gia { get; set; }

        [Display(Name = "Hình ảnh")]
        public string HinhAnh { get; set; }

        [Display(Name = "Ngày đăng")]
        public DateTime NgayDang { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn phòng trọ")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn phòng trọ hợp lệ")]
        public int PhongTroId { get; set; }

        [ValidateNever]
        public PhongTro PhongTro { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chủ trọ")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn chủ trọ hợp lệ")]
        public int ChuTroId { get; set; }

        [ValidateNever]
        public ChuTro ChuTro { get; set; }
    }
}
