using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace api.Models
{
    [Table("vanbandichitiet")] 
    public class VBDi 
    {
        [Key]
        public string vbid { get; set; }  
        
        public string? file { get; set; }
        public string? files { get; set; }
        public string? sokh { get; set; }
        public string? coquanbh { get; set; }
        public string? ngaybh { get; set; }
        public string? ngaynhap { get; set; }
        public string? trichyeu { get; set; }
        public string? nguoiky { get; set; }
        public string? nguoiduyet { get; set; }
        public string? chuyenvien { get; set; }
        public string? vanbandenname { get; set; }
        public string? vanbandenid { get; set; }
        public string? noinhantext { get; set; }
        public string? loaivanban { get; set; }
        public int? sobanphathanh { get; set; }
        public string? tinhtrang { get; set; }
        public string? filedinhkem { get; set; }
    
    }
}