using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models;
using api.Interfaces;
using api.Helper;
using api.DTO;
using System.Globalization;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Data.Common;



namespace api.Repository
{
    public class VBDiRepository : IVBDiRepository
    {
        private readonly ApplicationDBContext _context;
        
        public VBDiRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        
        public async Task<object> GetAllAsync(QueryVBDiObject query)
        {
            var VBDiQuery =_context.VBDis.AsQueryable();
            if (!string.IsNullOrEmpty(query.nguoiky))
            {
                VBDiQuery = VBDiQuery.Where(v => v.nguoiky.Trim().ToLower().Contains(query.nguoiky));
            }
            if (!string.IsNullOrEmpty(query.nguoiduyet))
            {
                VBDiQuery = VBDiQuery.Where(v => v.nguoiduyet.Trim().ToLower().Contains(query.nguoiduyet));
            }
            if (!string.IsNullOrEmpty(query.chuyenvien))
            {
                VBDiQuery = VBDiQuery.Where(v => v.chuyenvien.Trim().ToLower().Contains(query.chuyenvien));
            }
            if (!string.IsNullOrEmpty(query.trichyeu))
            {
                VBDiQuery = VBDiQuery.Where(v => v.trichyeu.Trim().ToLower().Contains(query.trichyeu));
            }
            if (!string.IsNullOrEmpty(query.ngaybh))
            {
                VBDiQuery = VBDiQuery.Where(v => v.ngaybh.Trim().ToLower().Contains(query.ngaybh));
            }
            if (!string.IsNullOrEmpty(query.ngaynhap))
            {
                VBDiQuery = VBDiQuery.Where(v => v.ngaynhap.Trim().ToLower().Contains(query.ngaynhap));
            }
            if (!string.IsNullOrEmpty(query.coquanbh))
            {
                VBDiQuery = VBDiQuery.Where(v => v.coquanbh.Trim().ToLower().Contains(query.coquanbh));
            }
            if (!string.IsNullOrEmpty(query.sokh))
            {
                VBDiQuery = VBDiQuery.Where(v => v.sokh.Trim().ToLower().Contains(query.sokh));
            }
            if (!string.IsNullOrEmpty(query.vanbandenname))
            {
                VBDiQuery = VBDiQuery.Where(v => v.vanbandenname.Trim().ToLower().Contains(query.vanbandenname));
            }
            if (!string.IsNullOrEmpty(query.vanbandenid))
            {
                VBDiQuery = VBDiQuery.Where(v => v.vanbandenid.Trim().ToLower().Contains(query.vanbandenid));
            }
            if (!string.IsNullOrEmpty(query.noinhantext))
            {
                VBDiQuery = VBDiQuery.Where(v => v.noinhantext.Trim().ToLower().Contains(query.noinhantext));
            }
            if (!string.IsNullOrEmpty(query.loaivanban))
            {
                VBDiQuery = VBDiQuery.Where(v => v.loaivanban.Trim().ToLower().Contains(query.loaivanban));
            }
            if (!string.IsNullOrEmpty(query.tinhtrang))
            {
                VBDiQuery = VBDiQuery.Where(v => v.tinhtrang.Trim().ToLower().Contains(query.tinhtrang));
            }
            if (query.sobanphathanh != null)
            {
                VBDiQuery = VBDiQuery.Where(v => v.sobanphathanh == query.sobanphathanh);
            }
            if (query.ngaybh_from.HasValue || query.ngaybh_to.HasValue)
            {
                DateTime? fromDate = query.ngaybh_from;
                DateTime? toDate = query.ngaybh_to?.AddDays(1).AddTicks(-1); // Lấy đến cuối ngày

                VBDiQuery = VBDiQuery
                    .AsEnumerable() 
                    .Where(v =>
                        DateTime.TryParseExact(v.ngaybh, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime ngaybhDate) &&
                        (!fromDate.HasValue || ngaybhDate >= fromDate) &&
                        (!toDate.HasValue || ngaybhDate <= toDate))
                    .AsQueryable(); 
            }
           

            return VBDiQuery.ToList();
        }
        public async Task<Dictionary<string, List<string>>> GetDistinctColumnValues(string[] columnNames)
        {
            var result = new Dictionary<string, List<string>>();
            
            foreach (var columnName in columnNames)
            {
                switch (columnName.ToLower())
                {
                    case "trichyeu":
                        result[columnName] = await _context.VBDis
                            .Where(v => !string.IsNullOrEmpty(v.trichyeu))
                            .Select(v => v.trichyeu!)
                            .Distinct()
                            .ToListAsync();
                        break;
                    case "nguoiky":
                        result[columnName] = await _context.VBDis
                            .Where(v => !string.IsNullOrEmpty(v.nguoiky))
                            .Select(v => v.nguoiky!)
                            .Distinct()
                            .ToListAsync();
                        break;
                    case "nguoiduyet":
                        result[columnName] = await _context.VBDis
                            .Where(v => !string.IsNullOrEmpty(v.nguoiduyet))
                            .Select(v => v.nguoiduyet!)
                            .Distinct()
                            .ToListAsync();
                        break;
                    case "loaivanban":
                        result[columnName] = await _context.VBDis
                            .Where(v => !string.IsNullOrEmpty(v.loaivanban))
                            .Select(v => v.loaivanban!)
                            .Distinct()
                            .ToListAsync();
                        break;
                    case "tinhtrang":
                        result[columnName] = await _context.VBDis
                            .Where(v => !string.IsNullOrEmpty(v.tinhtrang))
                            .Select(v => v.tinhtrang!)
                            .Distinct()
                            .ToListAsync();
                        break;
                    case "coquanbh":
                        result[columnName] = await _context.VBDis
                            .Where(v => !string.IsNullOrEmpty(v.coquanbh))
                            .Select(v => v.coquanbh!)
                            .Distinct()
                            .ToListAsync();
                        break;
                    case "chuyenvien":
                        result[columnName] = await _context.VBDis
                            .Where(v => !string.IsNullOrEmpty(v.chuyenvien))
                            .Select(v => v.chuyenvien!)
                            .Distinct()
                            .ToListAsync();
                        break;

                }
            }
            
            return result;
        }
        public async Task<object> SearchWithFieldTermsAsync(Dictionary<string, string> fieldTermMap, int limit)
        {
            if (fieldTermMap == null || !fieldTermMap.Any())
            {
                return new { results = new List<dynamic>(), total = 0 };
            }

            var selectPart = "SELECT sokh, trichyeu, nguoiky, coquanbh, ngaybh, ";

            var scoreExpressions = new List<string>();
            var whereConditions = new List<string>();
            var parameters = new List<object>();
            var paramIndex = 0;

            foreach (var pair in fieldTermMap)
            {
                string field = pair.Key;
                string termValue = pair.Value.ToLower();

                // Tìm kiếm cả cụm từ trước
                scoreExpressions.Add($"CASE WHEN LOWER({field}) LIKE @p{paramIndex} THEN 10 ELSE 0 END");
                parameters.Add($"%{termValue}%");
                whereConditions.Add($"LOWER({field}) LIKE @p{paramIndex}");
                paramIndex++;

                // Tìm kiếm từng từ riêng lẻ
                var words = termValue.Split(new char[] { ' ', ',', '.', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Where(w => w.Length > 1)
                                    .ToArray();

                foreach (var word in words)
                {
                    scoreExpressions.Add($"CASE WHEN LOWER({field}) LIKE @p{paramIndex} THEN 1 ELSE 0 END");
                    parameters.Add($"%{word}%");
                    whereConditions.Add($"LOWER({field}) LIKE @p{paramIndex}");
                    paramIndex++;
                }
            }

            selectPart += $"({string.Join(" + ", scoreExpressions)}) AS score";

            var whereClause = whereConditions.Any() ? $"WHERE {string.Join(" OR ", whereConditions)}" : "";

            string sql = $"{selectPart} FROM vanbandichitiet {whereClause} ORDER BY score DESC LIMIT {limit}";

            var results = new List<dynamic>();

            using (var connection = _context.Database.GetDbConnection())
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sql;

                    for (int i = 0; i < parameters.Count; i++)
                    {
                        var param = command.CreateParameter();
                        param.ParameterName = $"@p{i}";
                        param.Value = parameters[i];
                        command.Parameters.Add(param);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = new ExpandoObject() as IDictionary<string, object>;

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                                item[reader.GetName(i)] = value;
                            }

                            results.Add((ExpandoObject)item);
                        }
                    }
                }
            }

            return new { results = results, total = results.Count };
        }

        public async Task<object> searchVBDiForAI(string text )
        {
            try
            {
                var json = await JsonHelper.ExtractJsonAsync(text);

                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new ArgumentException("Input text does not contain valid JSON.");
                }

                //Console.WriteLine($"JSON: {json}");
                var searchRequest = JsonConvert.DeserializeObject<SearchVBdiByFilterReqDto>(json);
                //Console.WriteLine($"Search request: {searchRequest}");
                Console.WriteLine($"Search Request: {JsonConvert.SerializeObject(searchRequest, Formatting.Indented)}");

                
                // Xây dựng điều kiện tìm kiếm và tính điểm
                var fieldTermMap = new Dictionary<string, string>();
           
                if (!string.IsNullOrEmpty(searchRequest.trichyeu))
                    fieldTermMap["trichyeu"] = searchRequest.trichyeu.Trim();
                    
                if (!string.IsNullOrEmpty(searchRequest.nguoiky))
                    fieldTermMap["nguoiky"] = searchRequest.nguoiky.Trim();
                    
                if (!string.IsNullOrEmpty(searchRequest.coquanbh))
                    fieldTermMap["coquanbh"] = searchRequest.coquanbh.Trim();
                    
                if (!string.IsNullOrEmpty(searchRequest.sokh))
                    fieldTermMap["sokh"] = searchRequest.sokh.Trim();
                    
                if (!string.IsNullOrEmpty(searchRequest.loaivanban))
                    fieldTermMap["loaivanban"] = searchRequest.loaivanban.Trim();
                    
                if (!string.IsNullOrEmpty(searchRequest.tinhtrang))
                    fieldTermMap["tinhtrang"] = searchRequest.tinhtrang.Trim();
                
                
                // Giới hạn số lượng kết quả
                int limit = searchRequest.limit > 0 ? searchRequest.limit : 10;

                
                // Thực hiện tìm kiếm 
                var results = await searchVBDiDetailForAI(searchRequest, limit );
                
                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tìm kiếm văn bản: {ex.Message}");
                return null;
            }
        }

       
        public async Task<object> searchVBDiDetailForAI(SearchVBdiByFilterReqDto searchRequest, int limit )
        {

            var scoreExpressions = new List<string>();
            var whereConditions = new List<string>();
            var dateConditions = new List<string>(); 
            var parameters = new List<object>();
            var paramIndex = 0;
            
            if (searchRequest.countAll)
            {
                var countWhereClause = whereConditions.Any() ? $"WHERE {string.Join(" AND ", whereConditions)}" : "";
                var countSql = $"SELECT COUNT(*) FROM vanbandichitiet {countWhereClause}";

                
                Console.WriteLine($" SQL COUNT_ALL: {countSql}");

                var countResult = await RawSqlQueryAsync<int>(countSql, reader => {
                    
                    return Convert.ToInt32(reader[0]);
                });
                
                if (!countResult.Any())
                {
                    Console.WriteLine("LỖI: Không có dữ liệu trong countResult!");
                }
                else
                {
                    Console.WriteLine($"Tổng số bản ghi: {countResult.First()}");
                }
                return new { totalCount = countResult.Any() ? countResult.First() : -1 };  // Trả về -1 nếu không có dữ liệu

                
            }
            var selectPart = "SELECT sokh, trichyeu, nguoiky, nguoiduyet, coquanbh, ngaybh, loaivanban, ";
            
            // Điểm trichyeu
            if (!string.IsNullOrEmpty(searchRequest.trichyeu))
            {
                string termValue = searchRequest.trichyeu.ToLower();
                
                
                scoreExpressions.Add($"CASE WHEN LOWER(trichyeu) LIKE @p{paramIndex} THEN 8 ELSE 0 END");
                parameters.Add($"%{termValue}%");
                whereConditions.Add($"LOWER(trichyeu) LIKE @p{paramIndex}");
                paramIndex++;
                
                
                var words = termValue.Split(new char[] { ' ', ',', '.', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                .Where(w => w.Length > 1)
                                .ToArray();
                
                foreach (var word in words)
                {
                    scoreExpressions.Add($"CASE WHEN LOWER(trichyeu) LIKE @p{paramIndex} THEN 1 ELSE 0 END");
                    parameters.Add($"%{word}%");
                    whereConditions.Add($"LOWER(trichyeu) LIKE @p{paramIndex}");
                    paramIndex++;
                }
            }
            
            // Điểm nguoiky
            if (!string.IsNullOrEmpty(searchRequest.nguoiky))
            {
                string termValue = searchRequest.nguoiky.ToLower();
                
                scoreExpressions.Add($"CASE WHEN LOWER(nguoiky) LIKE @p{paramIndex} THEN 1 ELSE 0 END");
                parameters.Add($"%{termValue}%");
                whereConditions.Add($"LOWER(nguoiky) LIKE @p{paramIndex}");
                paramIndex++;
            }
            // Điểm nguoiduyet
            if (!string.IsNullOrEmpty(searchRequest.nguoiduyet))
            {
                string termValue = searchRequest.nguoiduyet.ToLower();
                scoreExpressions.Add($"CASE WHEN LOWER(nguoiduyet) LIKE @p{paramIndex} THEN 1 ELSE 0 END");
                parameters.Add($"%{termValue}%");
                whereConditions.Add($"LOWER(nguoiduyet) LIKE @p{paramIndex}");
                paramIndex++;
            }
            //  Điểm chuyenvien
            if (!string.IsNullOrEmpty(searchRequest.chuyenvien))
            {
                string termValue = searchRequest.chuyenvien.ToLower();
                scoreExpressions.Add($"CASE WHEN LOWER(chuyenvien) LIKE @p{paramIndex} THEN 1 ELSE 0 END");
                parameters.Add($"%{termValue}%");
                whereConditions.Add($"LOWER(chuyenvien) LIKE @p{paramIndex}");
                paramIndex++;
            }   
            // Điểm coquanbh        
            if (!string.IsNullOrEmpty(searchRequest.coquanbh))
            {
                string termValue = searchRequest.coquanbh.ToLower();
                
                scoreExpressions.Add($"CASE WHEN LOWER(coquanbh) LIKE @p{paramIndex} THEN 1 ELSE 0 END");
                parameters.Add($"%{termValue}%");
                whereConditions.Add($"LOWER(coquanbh) LIKE @p{paramIndex}");
                paramIndex++;
            }
            // Điểm sokh
            if (!string.IsNullOrEmpty(searchRequest.sokh))
            {
                string termValue = searchRequest.sokh.ToLower();
                
                scoreExpressions.Add($"CASE WHEN LOWER(sokh) LIKE @p{paramIndex} THEN 5 ELSE 0 END");
                parameters.Add($"%{termValue}%");
                whereConditions.Add($"LOWER(sokh) LIKE @p{paramIndex}");
                paramIndex++;
            }
            
            // Điểm loaivanban
            if (!string.IsNullOrEmpty(searchRequest.loaivanban))
            {
                string termValue = searchRequest.loaivanban.ToLower();
            
                scoreExpressions.Add($"CASE WHEN LOWER(loaivanban) LIKE @p{paramIndex} THEN 5 ELSE 0 END");
                parameters.Add($"%{termValue}%");
                whereConditions.Add($"LOWER(loaivanban) LIKE @p{paramIndex}");
                paramIndex++;
            }
              
            if (searchRequest.ngaybh_from.HasValue && searchRequest.ngaybh_to.HasValue)
            {
                
                string fromDateStr = searchRequest.ngaybh_from.Value.ToString("yyyy-MM-dd HH:mm:ss");
                string toDateStr = searchRequest.ngaybh_to.Value.AddDays(1).AddTicks(-1).ToString("yyyy-MM-dd HH:mm:ss");
                
                // Chuyển ngaybh thành DATETIME và so sánh với 2 giá trị
                string dateCondition = $"STR_TO_DATE(ngaybh, '%d/%m/%Y') BETWEEN '{fromDateStr}' AND '{toDateStr}'";
                
                // Thêm vào điều kiện ngày
                dateConditions.Add(dateCondition);
            }


            if (scoreExpressions.Count == 0)
            {
                
                scoreExpressions.Add("1");
            }
            
            selectPart += $"({string.Join(" + ", scoreExpressions)}) AS score";
            // Where clause
            string whereClause = "";

            // Nếu có điều kiện tìm kiếm
            if (whereConditions.Any())
            {
                whereClause = $"WHERE ({string.Join(" OR ", whereConditions)})";
                
                // Nếu có điều kiện ngày, thêm vào bằng AND
                if (dateConditions.Any())
                {
                    whereClause += $" AND ({string.Join(" AND ", dateConditions)})";
                }
            }
            // Nếu chỉ có điều kiện ngày
            else if (dateConditions.Any())
            {
                whereClause = $"WHERE {string.Join(" AND ", dateConditions)}";
            }
            
            string limitClause = "";
            // Áp dụng limit chỉ khi cả hai countAll và countCondition đều là false
            if (searchRequest.applyLimit && !searchRequest.countAll && !searchRequest.countCondition)
            {
                limitClause = $" LIMIT {limit}";
            }
            // Trường hợp đếm bản ghi thỏa mãn điều kiện
            if (searchRequest.countCondition)
            {
                string countSql = $"SELECT COUNT(*) FROM vanbandichitiet {whereClause}";
                
                // Thay thế các tham số trong SQL
                for (int i = 0; i < parameters.Count; i++)
                {
                    var paramValue = parameters[i]?.ToString() ?? "";
                    paramValue = paramValue.Replace("'", "''");
                    countSql = countSql.Replace($"@p{i}", $"'{paramValue}'");
                }
                
                var countResult = await RawSqlQueryAsync<int>(countSql, reader => {
                    return Convert.ToInt32(reader[0]);                    
                });
                Console.WriteLine($"SQL COUNT_Condition: {countSql}");
                
                return new { matchCount = countResult.FirstOrDefault() };
            }
            
            var sql = $"{selectPart} FROM vanbandichitiet {whereClause} ORDER BY score DESC {limitClause}";
            //Console.WriteLine($"SQL: {sql}");

            for (int i = 0; i < parameters.Count; i++)
            {
                var paramValue = parameters[i]?.ToString() ?? "";
                // Escape các ký tự đặc biệt trong SQL
                paramValue = paramValue.Replace("'", "''");
                    
                sql = sql.Replace($" @p{i} ", $" '{paramValue}' ")   // Thay thế khi có dấu cách
                        .Replace($" @p{i},", $" '{paramValue}',")   // Thay thế khi có dấu phẩy
                        .Replace($"(@p{i} ", $"('{paramValue}' ")   // Thay thế sau dấu mở ngoặc
                        .Replace($"(@p{i},", $"('{paramValue},")    // Thay thế sau dấu mở ngoặc với phẩy
                        .Replace($" @p{i})", $" '{paramValue}')")   // Thay thế trước dấu đóng ngoặc
                        .Replace($",@p{i} ", $",'{paramValue}' ")   // Thay thế khi có dấu phẩy trước
                        .Replace($",@p{i})", $",'{paramValue}')")   // Thay thế với phẩy trước và đóng ngoặc
                        .Replace($" @p{i}=", $" '{paramValue}'=")   // Thay thế khi có dấu bằng
                        .Replace($"=@p{i} ", $"='{paramValue}' ");  // Thay thế khi có dấu bằng trước
                
                // Xử lý trường hợp tham số ở đầu hoặc cuối câu
                if (sql.StartsWith($"@p{i} "))
                    sql = $"'{paramValue}' " + sql.Substring(($"@p{i} ").Length);
                
                if (sql.EndsWith($" @p{i}"))
                    sql = sql.Substring(0, sql.Length - ($" @p{i}").Length) + $" '{paramValue}'";
            }

            // Kiểm tra nếu có tham số chưa được thay thế (trường hợp đặc biệt)
            for (int i = 0; i < parameters.Count; i++)
            {
                if (sql.Contains($"@p{i}"))
                {
                    var paramValue = parameters[i]?.ToString() ?? "";
                    paramValue = paramValue.Replace("'", "''");
                    
                    // Thay thế tất cả các tham số còn sót
                    sql = sql.Replace($"@p{i}", $"'{paramValue}'");
                }
            }

            Console.WriteLine($"SQL sau khi thay tham số: {sql}");
            var results = await RawSqlQueryAsync<dynamic>(sql, reader => {
                var item = new ExpandoObject() as IDictionary<string, object>;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    item[reader.GetName(i)] = value;
                }
                return item;
            });
            
            return new { results = results, total = results.Count };
                    
        }
        public async Task<List<VBDiDTO>> RawSqlQueryAsync<VBDiDTO>(string query, Func<DbDataReader, VBDiDTO> map)
        {
            try
            {
                using (var command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = query;
                    command.CommandType = System.Data.CommandType.Text;

                    if (_context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
                    {
                        await _context.Database.OpenConnectionAsync();
                    }

                    using (var result = await command.ExecuteReaderAsync())
                    {
                        var entities = new List<VBDiDTO>();

                        while (await result.ReadAsync())
                        {
                            entities.Add(map(result));
                        }

                        return entities;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi thực thi SQL: {ex.Message}");
                throw;
            }
        }

    }


}


