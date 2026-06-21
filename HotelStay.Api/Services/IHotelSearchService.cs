using System.Threading.Tasks;
using HotelStay.Api.Models;

namespace HotelStay.Api.Services
{
    /// <summary>
    /// Service that aggregates searches across hotel providers and returns a unified response.
    /// </summary>
    public interface IHotelSearchService
    {
        /// <summary>
        /// Search for hotel rooms according to the request.
        /// </summary>
        Task<HotelSearchResponse> SearchAsync(HotelSearchRequest request);
    }
}
