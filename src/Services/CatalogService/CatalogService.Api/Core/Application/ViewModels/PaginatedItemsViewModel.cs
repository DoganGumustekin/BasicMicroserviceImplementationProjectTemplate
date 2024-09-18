namespace CatalogService.Api.Core.Application.ViewModels
{
    public class PaginatedItemsViewModel<TEntity> where TEntity : class //ilk sayafada 10 tane gelsin ikinci sayfada 20 tane. 
    {
        public int PageIndex { get; set; } //hangi sayfadayız.
        public int PageSize { get; set; } //bu sayfada kaç ürün listeleniyor.
        public long Count { get; set; } //toplam ürün sayısı.
        public IEnumerable<TEntity> Data { get; set; } //o sayfadaki data.
        public PaginatedItemsViewModel(int pageIndex, int pageSize, long count, IEnumerable<TEntity> data)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            Count = count;
            Data = data;
        }
    }
}
