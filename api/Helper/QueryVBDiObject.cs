using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using Microsoft.AspNetCore.Identity;

namespace api.Helper 
{
    public class QueryVBDiObject
    {
        public string? sokh { get; set; } = null;
        public string? coquanbh { get; set; }= null;
        public string? ngaybh { get; set; } = null;
        public string? ngaynhap { get; set; } = null;
        public string? trichyeu { get; set; } = null;
        public string? nguoiky { get; set; } = null;
        public string? nguoiduyet { get; set; } = null;
        public string? chuyenvien { get; set; } = null;
        public string? vanbandenname { get; set; } = null;
        public string? vanbandenid { get; set; } = null;
        public string? noinhantext { get; set; } = null;
        public string? loaivanban { get; set; } = null;
        public int? sobanphathanh { get; set; } = null;
        public string? tinhtrang { get; set; } = null;



        public DateTime? ngaybh_from { get; set; }
        public DateTime? ngaybh_to { get; set; }
        public int pageNumber { get; set; } = 1;
        public int pageSize { get; set; } = 10;

        public bool countAll { get; set; } = false;
        public bool countCondition { get; set; } = false;
    }



}