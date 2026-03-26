using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace WebQLPT.Models
{
    public class DangTin
    {
        public int Id { get; set; }

        [Display(Name = "Tiêu đề")]
        public string TieuDe { get; set; }

        [Display(Name = "Nội dung")]
        public string NoiDung { get; set; }

        [Display(Name = "Giá")]
        public decimal Gia { get; set; }

        [Display(Name = "Hình ảnh")]
        public string HinhAnh { get; set; }

        [Display(Name = "Ngày đăng")]
        public DateTime NgayDang { get; set; } = DateTime.Now;


        public int PhongTroId { get; set; }

        [ValidateNever]
        public PhongTro PhongTro { get; set; }

        public int ChuTroId { get; set; }

        [ValidateNever]
        public ChuTro ChuTro { get; set; }
    }
}
