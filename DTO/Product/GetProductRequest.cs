namespace Ecommerce.DTO.Product;

public class GetProductRequest
{
    public Guid? CategoryId { get; set; }
    public string? Search { get; set; } = string.Empty;
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}