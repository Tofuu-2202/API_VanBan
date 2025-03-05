using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace api.Helper
{
    public static class JsonHelper
    {
  
        public static string ExtractJson(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Sử dụng regex để tìm chuỗi JSON 
            var regex = new Regex(@"\{(?:[^{}]|(?<open>\{)|(?<-open>\}))+(?(open)(?!))\}", RegexOptions.Compiled);
            var match = regex.Match(text);
            
            if (match.Success)
            {
                return match.Value;
            }
            
            // Nếu không tìm thấy, thử xem text có phải là JSON hợp lệ không
            try
            {
                JObject.Parse(text);
                return text;
            }
            catch
            {
                return text; // Trả về text gốc nếu không phân tích được
            }
        }

        public static Task<string> ExtractJsonAsync(string text)
        {
            return Task.FromResult(ExtractJson(text));
        }
    }
}