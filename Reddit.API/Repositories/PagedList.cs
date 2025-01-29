using Microsoft.EntityFrameworkCore;

namespace Reddit.Repositories
{
    public class PagedList<T>
    {
        private PagedList(List<T> items, int page, int pageSize, int count, bool hasNextPage, bool hasPreviousPage)
        {
            Items = items;
            PageNumber = page;
            PageSize = pageSize;
            TotalCount = count;
            HasNextPage = hasNextPage;
            HasPreviousPage = hasPreviousPage;
        }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<T> Items { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> items, int pageNumber, int pageSize)
        {
            Validate(pageNumber, pageSize);

            var pagedItems = await items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var totalCount = await items.CountAsync();
            var hasNextPage = (pageNumber * pageSize) < totalCount;
            var hasPreviousPage = pageNumber > 1;

            return new PagedList<T>(pagedItems, pageNumber, pageSize, totalCount, hasNextPage, hasPreviousPage);
        }

        private static void Validate(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageNumber),"pageNumber and pageSize must be greater than 0");

            if(pageSize <= 0 || pageSize > 50) 
                throw new ArgumentOutOfRangeException(nameof(pageSize),"pageSize must be greater than 0 and less than 50");
        }
    }
}
