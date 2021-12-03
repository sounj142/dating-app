using API.Helpers;
using Microsoft.AspNetCore.Http;

namespace API.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse httpResponse, PaginationHeader paginationHeader)
        {
            var headerContent = paginationHeader.SerializeCamelCase();

            httpResponse.Headers.Add("Pagination", headerContent);
            httpResponse.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }

        public static void AddPaginationHeader<T>(this HttpResponse httpResponse, PagedList<T> paginationData)
        {
            AddPaginationHeader(httpResponse,
                new PaginationHeader(
                    currentPage: paginationData.CurrentPage,
                    totalPages: paginationData.TotalPages,
                    pageSize: paginationData.PageSize,
                    totalCount: paginationData.TotalCount)
            );
        }
    }
}
