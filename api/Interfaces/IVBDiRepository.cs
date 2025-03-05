using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Common;

using api.Interfaces;
using api.Models;
using api.Helper;
using api.DTO;

namespace api.Interfaces
{
    public interface IVBDiRepository
    {
        //Task<List<VBDi>> GetAllAsync(QueryVBDiObject query);
        Task<object> GetAllAsync(QueryVBDiObject query);
        Task<Dictionary<string, List<string>>> GetDistinctColumnValues(string[] columnNames);

        //Task<object> SearchWithScoringAsync(string searchTerm, string[] searchFields, int limit = 20);
        Task<object> SearchWithFieldTermsAsync(Dictionary<string, string> fieldTermMap, int limit);

        Task<object> searchVBDiForAI(string text);
        Task<object> searchVBDiDetailForAI(SearchVBdiByFilterReqDto searchRequest, int limit = 20);
        Task<List<T>> RawSqlQueryAsync<T>(string query, Func<DbDataReader, T> map);
    }
}