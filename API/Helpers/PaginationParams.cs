using System;

namespace API.Helpers
{
    public class PaginationParams
    {
        private const int MAX_PAGE_SIZE = 50;

        public int CurrentPage { get; set; } = 1;

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = Math.Min(value, MAX_PAGE_SIZE);
                _pageSize = Math.Max(_pageSize, 1);
            }
        }
    }
}
