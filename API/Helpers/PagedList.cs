using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class PagedList<T> : List<T>
    {
        public PagedList(IEnumerable<T> items, int totalCount, int currentPage, int pageSize) : base(items)
        {
            CurrentPage = currentPage;
            TotalPages = (int)Math.Ceiling(totalCount / (decimal)pageSize);
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int currentPage, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedList<T>(items: items, totalCount: count, currentPage: currentPage, pageSize: pageSize);
        }
    }
}
