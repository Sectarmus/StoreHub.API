namespace StoreHub.API.Params;

public class ProductParams
{
    private const int MaxPageSize = 50; // Bir seferde maksimum ne kadar veri verelim? Güvenlik sınırı.
    public int PageNumber { get; set; } = 1; // Sayfa numarası (Varsayılan 1)

    private int _pageSize = 10; // Varsayılan 10 kayıt
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    // Filtreleme için ekleyelim:
    public string? Search { get; set; } // Ürün isminde arama yapmak için
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
