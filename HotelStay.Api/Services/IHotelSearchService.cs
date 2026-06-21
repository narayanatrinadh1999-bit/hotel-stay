using HotelStay.Api.Models;

namespace HotelStay.Api.Services;

public interface IHotelSearchService
{
    Task<HotelSearchResponse> SearchAsync(HotelSearchRequest request);
}
