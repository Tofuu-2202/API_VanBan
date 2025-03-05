using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using api.Data;
using api.Models;
using api.Interfaces;
using api.Helper;
using api.Repository;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace api.Controller
{
    [Route("api/VBDichitiet")]
    [ApiController]
    public class VBdiController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IVBDiRepository _VBDiRepo;
        public VBdiController(ApplicationDBContext context, IVBDiRepository VBDiRepo)
        {
            _context = context;
            _VBDiRepo = VBDiRepo;

        }
        [HttpGet]
        [SwaggerOperation(
            Summary = "Văn bản đi chi tiết",
            Description = "Lấy danh sách văn bản đi chi tiết"
        )]
        public async Task<IActionResult> GetAll([FromQuery] QueryVBDiObject query)
        {
            var VBDis = await _VBDiRepo.GetAllAsync(query);
            return Ok(VBDis);
        }
        [HttpPost("filter1")]
        public async Task<IActionResult> Filter1VBDi([FromBody] QueryVBDiObject query)
        {
            var results = await _VBDiRepo.GetAllAsync(query);
            return Ok(results);
        }
        
 
        [HttpPost("filter")]
        public async Task<IActionResult> FilterVBDi([FromBody] dynamic requestBody)
        {
            try
            {
                // Lấy dữ liệu từ request body
                string jsonDataString = requestBody.json_data.ToString();
                JObject jsonObject = JObject.Parse(jsonDataString);
                
                // Kiểm tra CountAll
                bool countAll = false;
                if (jsonObject["CountAll"] != null && jsonObject["CountAll"].Type == JTokenType.Boolean)
                {
                    countAll = jsonObject["CountAll"].ToObject<bool>();
                }
                else if (jsonObject["CountAll"] != null && jsonObject["CountAll"].Type == JTokenType.String)
                {
                    bool.TryParse(jsonObject["CountAll"].ToString(), out countAll);
                }
                
                if (countAll)
                {
                    // Đếm tất cả văn bản trong database
                    int totalCount = await _context.VBDis.CountAsync();
                    return Ok(new { totalCount });
                }
                
                // Xử lý lấy danh sách theo bộ lọc
                QueryVBDiObject query = jsonObject.ToObject<QueryVBDiObject>();
                var results = await _VBDiRepo.GetAllAsync(query);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi xử lý: {ex.Message}");
            }
        }

        [HttpGet("trichyeu")]
        public async Task<IActionResult> GetAllTrichYeu()
        {
            try
            {
                // Lấy tất cả giá trị trichyeu, loại bỏ null và trùng lặp
                var trichyeuValues = await _context.VBDis
                    .Where(v => v.trichyeu != null && v.trichyeu != "")
                    .Select(v => v.trichyeu)
                    .Distinct()
                    .ToListAsync();
                    
                return Ok(trichyeuValues);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
        [HttpPost("columns")]
        public async Task<IActionResult> GetDistinctColumns([FromBody] dynamic requestBody)
        {
            try
            {
                string jsonDataString = requestBody.json_data.ToString();
                string[] columns;
                
                // Thử parse theo format mảng trực tiếp
                try
                {
                    // Kiểm tra xem có phải là mảng không
                    if (jsonDataString.TrimStart().StartsWith("[") && jsonDataString.TrimEnd().EndsWith("]"))
                    {
                        columns = JsonConvert.DeserializeObject<string[]>(jsonDataString);
                    }
                    else
                    {
                        // Thử parse theo format object có thuộc tính columns
                        JObject jsonObject = JObject.Parse(jsonDataString);
                        
                        if (jsonObject["columns"] != null && jsonObject["columns"].Type == JTokenType.Array)
                        {
                            columns = jsonObject["columns"].ToObject<string[]>();
                        }
                        else
                        {
                            // Thử xem có phải là mảng không key không
                            JArray jsonArray = JArray.Parse(jsonDataString);
                            columns = jsonArray.ToObject<string[]>();
                        }
                    }
                }
                catch
                {
                    
                    // Loại bỏ dấu {} và split theo dấu phẩy
                    jsonDataString = jsonDataString.Trim('{', '}', '[', ']', ' ');
                    columns = jsonDataString.Split(',')
                        .Select(s => s.Trim('"', '\''))
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToArray();
                }
                
                if (columns == null || columns.Length == 0)
                {
                    return BadRequest("Cần cung cấp ít nhất một tên cột");
                }
                
                var result = await _VBDiRepo.GetDistinctColumnValues(columns);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }

        }
        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] dynamic requestBody)
        {
            try
            {
                string jsonDataString = requestBody.json_data.ToString();
                JObject jsonObject = JObject.Parse(jsonDataString);
                
                var fieldTermMap = new Dictionary<string, string>();
                
                // Kiểm tra nếu có trường term
                if (jsonObject["term"] != null && !string.IsNullOrEmpty(jsonObject["term"].ToString()))
                {
                    string searchTerm = jsonObject["term"].ToString().Trim();
                    
                    // Phân tích cú pháp tìm kiếm theo định dạng "field: value"
                    if (searchTerm.Contains(":"))
                    {
                        // Tách phần trước và sau dấu ":"
                        int colonIndex = searchTerm.IndexOf(":");
                        string fieldName = searchTerm.Substring(0, colonIndex).Trim().ToLower();
                        string termValue = searchTerm.Substring(colonIndex + 1).Trim();
                        
                        // Thêm vào map
                        fieldTermMap[fieldName] = termValue;
                    }
                    else 
                    {
                        // Tìm kiếm thông thường trong tất cả các trường mặc định
                        string[] defaultFields = { "trichyeu", "nguoiky", "sokh" };
                        foreach (var field in defaultFields)
                        {
                            fieldTermMap[field] = searchTerm;
                        }
                    }
                }
                
                // Kiểm tra các trường khác trực tiếp từ jsonObject
                string[] validFields = { "trichyeu", "nguoiky", "sokh", "coquanbh", "tinhtrang", "loaivanban", "ngaybh", "ngaynhap", "nguoiduyet", "chuyenvien", "vanbandenname", "vanbandenid", "noinhantext", "sobanphathanh" };
                
                foreach (var field in validFields)
                {
                    if (jsonObject[field] != null && !string.IsNullOrEmpty(jsonObject[field].ToString()))
                    {
                        fieldTermMap[field] = jsonObject[field].ToString().Trim();
                    }
                }
                
                // Kiểm tra xem có ít nhất một trường tìm kiếm
                if (fieldTermMap.Count == 0)
                {
                    return BadRequest("Cần cung cấp ít nhất một trường tìm kiếm");
                }
                
                // Số lượng kết quả tối đa
                int limit = 20;
                if (jsonObject["limit"] != null && jsonObject["limit"].Type == JTokenType.Integer)
                {
                    limit = jsonObject["limit"].Value<int>();
                }
                
                var results = await _VBDiRepo.SearchWithFieldTermsAsync(fieldTermMap, limit);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi: {ex.Message}");
            }
        }
        [HttpPost("search-ai")]
        public async Task<IActionResult> SearchWithAI([FromBody] TextSearchRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Text))
                {
                    return BadRequest("Yêu cầu tìm kiếm không hợp lệ. Cần cung cấp nội dung text.");
                }

                var results = await _VBDiRepo.searchVBDiForAI(request.Text);
                
                if (results == null)
                {
                    return NotFound("Không tìm thấy kết quả phù hợp.");
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        public class TextSearchRequest
        {
            public string? Text { get; set; }
        }






    }
}


