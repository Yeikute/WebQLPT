using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebQLPT.Models
{
    public class PhongTro
    {
        public int Id { get; set; }

        [Display(Name = "Tên phòng")]
        public string TenPhong { get; set; }

        [Display(Name = "Giá phòng")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Gia { get; set; }

        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; }

        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Display(Name = "Chủ trọ")]
        public int ChuTroId { get; set; }

        [ValidateNever]
        public ChuTro ChuTro { get; set; }

        [ValidateNever]
        public List<KhachThue> KhachThues { get; set; }
    }
}
