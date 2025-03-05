public async Task<List<ProductVariantResForAIDto>> searchProductDetailForAI(OwnerParameters ownerParameters, SearchProductByFillterReqForAIDto searchProductByFillterRequestDto)
    {
        // Tạo biểu thức tính điểm cho từng thành phần

        // 1. Category score: Với mỗi category id hợp lệ, nếu nó có trong p.listCategoryValueId thì cộng 1 điểm.
        string categoryScoreExpression = "0";
        if (searchProductByFillterRequestDto.CategoryId != null &&
            searchProductByFillterRequestDto.CategoryId.Any(id => id > 0))
        {
            var validCategoryIds = searchProductByFillterRequestDto.CategoryId.Where(id => id > 0);
            categoryScoreExpression = string.Join(" + ", validCategoryIds
                .Select(id => $"CASE WHEN FIND_IN_SET('{id}', p.listCategoryValueId) > 0 THEN 1 ELSE 0 END"));
        }

        // 2. Label score: Với mỗi label id hợp lệ, nếu nó có trong p.listLableProductId thì cộng 1 điểm.
        string labelScoreExpression = "0";
        if (searchProductByFillterRequestDto.LabelId != null &&
            searchProductByFillterRequestDto.LabelId.Any(id => id > 0))
        {
            var validLabelIds = searchProductByFillterRequestDto.LabelId.Where(id => id > 0);
            labelScoreExpression = string.Join(" + ", validLabelIds
                .Select(id => $"CASE WHEN FIND_IN_SET('{id}', p.listLableProductId) > 0 THEN 1 ELSE 0 END"));
        }

        // 3. Attribute score: Với mỗi attribute id hợp lệ, nếu nó có trong p.listAttributeValueId thì cộng 1 điểm.
        string attributeScoreExpression = "0";
        if (searchProductByFillterRequestDto.ListAttributeId != null &&
            searchProductByFillterRequestDto.ListAttributeId.Any(id => id > 0))
        {
            var validAttributeIds = searchProductByFillterRequestDto.ListAttributeId.Where(id => id > 0);
            attributeScoreExpression = string.Join(" + ", validAttributeIds
                .Select(id => $"CASE WHEN FIND_IN_SET('{id}', p.listAttributeValueId) > 0 THEN 1 ELSE 0 END"));
        }

        // 4. Keyword score: Tách từ và với mỗi từ, nếu xuất hiện trong một trong các cột thì cộng 1 điểm.
        string keywordScoreExpression = "0";
        if (!string.IsNullOrEmpty(searchProductByFillterRequestDto.Keyword))
        {
            var words = searchProductByFillterRequestDto.Keyword
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Any())
            {
                keywordScoreExpression = string.Join(" + ", words.Select(word =>
                    $"CASE WHEN (p.skuProduct LIKE '%{word}%' OR p.nameProduct LIKE '%{word}%' OR p.skuAttributeGroup LIKE '%{word}%' OR p.nameAttributeGroup LIKE '%{word}%' OR p.variantCode LIKE '%{word}%' OR p.descriptionProduct LIKE '%{word}%' OR p.shortDescriptionProduct LIKE '%{word}%') THEN 1 ELSE 0 END"));
            }
        }

        // Tổng hợp match score từ 4 thành phần
        string matchScoreExpression = $"({categoryScoreExpression} + {labelScoreExpression} + {attributeScoreExpression} + {keywordScoreExpression})";

        // Xây dựng câu truy vấn cơ bản
        // Chú ý: Nếu sử dụng WITH RECURSIVE cho category, ta vẫn tính điểm từ p.listCategoryValueId
        string sql = $"SELECT p.*, {matchScoreExpression} AS match_score FROM product_variant AS p ";

        // Nếu có filter category thì thêm WITH RECURSIVE để mở rộng cây category
        if (searchProductByFillterRequestDto.CategoryId != null &&
            searchProductByFillterRequestDto.CategoryId.Any(id => id > 0))
        {
            var validCategoryIds = searchProductByFillterRequestDto.CategoryId.Where(id => id > 0);
            var categoryIds = string.Join(",", validCategoryIds);
            sql = "WITH RECURSIVE CategorySubtree AS ( " +
                  "SELECT id, parentId FROM category_value WHERE id IN (" + categoryIds + ") AND id > 0 " +
                  "UNION ALL " +
                  "SELECT cv.id, cv.parentId FROM category_value cv " +
                  "INNER JOIN CategorySubtree cs ON cv.parentId = cs.id " +
                  ") " + sql;

            // Dùng join để đảm bảo sản phẩm có thuộc một trong các category mở rộng
            sql += " JOIN CategorySubtree cs ON FIND_IN_SET(cs.id, p.listCategoryValueId) > 0 ";
        }

        // Các điều kiện bổ sung:
        // Ví dụ: điều kiện về giá, isDeleted,...
        sql += " WHERE (p.isDeleted = 0 OR p.isDeleted IS NULL) ";

        if (searchProductByFillterRequestDto.FromMoney > 0)
        {
            sql += " AND ((p.salePrice >= " + searchProductByFillterRequestDto.FromMoney +
                   ") OR (p.regularPrice >= " + searchProductByFillterRequestDto.FromMoney + "))";
        }

        if (searchProductByFillterRequestDto.ToMoney > 0)
        {
            sql += " AND ((p.salePrice <= " + searchProductByFillterRequestDto.ToMoney +
                   ") OR (p.regularPrice <= " + searchProductByFillterRequestDto.ToMoney + "))";
        }

        // Vì join CategorySubtree có thể nhân bản các bản ghi, ta group lại theo các trường định danh
        sql += " GROUP BY p.attributeGroupId, p.variantCode, p.id ";

        // Chỉ lấy những sản phẩm có match_score > 0 (có nghĩa là có ít nhất 1 điều kiện nào thỏa)
        sql += " HAVING match_score >= 0 ";

        // Sắp xếp theo match_score giảm dần, sau đó theo p.row và p.updateAt DESC
        sql += " ORDER BY match_score DESC, p.row, p.updateAt DESC; ";

        // Thực hiện truy vấn và mapping kết quả
        var dbProductVariants = RawSqlQuery<ProductVariantResForAIDto>(sql,x => new ProductVariantResForAIDto
            {
                ProductId = x["productId"] as int?,
                NameProduct = x["nameProduct"]?.ToString(),
                DescriptionProduct = x["descriptionProduct"]?.ToString(),
                ShortDescriptionProduct = x["shortDescriptionProduct"]?.ToString(),
                AttributeGroupId = x["attributeGroupId"] as int?,
                CategoryTypeValue = x["categoryTypeValue"]?.ToString(),
                SkuProduct = x["skuProduct"]?.ToString(),
                SkuAttributeGroup = x["skuAttributeGroup"]?.ToString(),
                NameAttributeGroup = x["nameAttributeGroup"]?.ToString(),
                ListLabelName = x["listLabelName"]?.ToString(),
                Price = (((x["salePrice"] as double?) ?? 0) > 0)
                            ? (x["salePrice"] as double?)
                            : (x["regularPrice"] as double?),
                PriceUnit = x["priceUnit"]?.ToString(),
                Quantity = x["totalQuantityInAllWarehouse"] as int?,
                Warranty = x["warranty"]?.ToString(),
                VariantCode = x["variantCode"]?.ToString(),
                LengthTypeAndValue = x["lengthTypeAndValue"]?.ToString(),
                AttributeValue = x["attributeValue"]?.ToString(),
            });

        return dbProductVariants;
    }


public async Task<object> SearchProductForAI(string text)
{
    try
    {
        var json = await ExtractJson(text);

        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("Input text does not contain valid JSON.");
        }

        var req = JsonConvert.DeserializeObject<SearchProductByFillterReqForAIDto>(json);
        var newOwnerParam = new OwnerParameters
        {
            pageIndex = 0,
            pageSize = 5
        };

        var product = await _productService.searchProductDetailForAI(newOwnerParam, req);
        return product;
    }
    catch
    {
        return null;
    }
}




public List<T> RawSqlQuery<T>(string query, Func<DbDataReader, T> map)
{
    try
    {
        using (var context = new DataContext(_configuration))
        {
            using (var command = context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                context.Database.OpenConnection();

                using (var result = command.ExecuteReader())
                {
                    var entities = new List<T>();

                    while (result.Read())
                    {
                        entities.Add(map(result));
                    }

                    return entities;
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
        throw;
    }
}