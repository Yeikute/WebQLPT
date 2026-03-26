using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace WebQLPT.Models
{
    public class ChuTro
    {
        public int Id { get; set; }

        [Display(Name = "Tên chủ trọ")]
        public string TenChuTro { get; set; }


        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }
        public string Email { get; set; }

        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        [ValidateNever]
        public List<PhongTro> PhongTros { get; set; }
    }
}
