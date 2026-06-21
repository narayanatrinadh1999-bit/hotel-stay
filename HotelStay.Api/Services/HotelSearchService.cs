using HotelStay.Api.Models;
using HotelStay.Api.Providers;

namespace HotelStay.Api.Services;

public class HotelSearchService : IHotelSearchService
{
    private static readonly HashSet<string> KnownDestinations = new(StringComparer.OrdinalIgnoreCase)
    {
        "New York", "Los Angeles", "New Delhi", "Hyderabad",
        "London", "Paris", "Tokyo"
    };

    private readonly IEnumerable<IHotelProvider> _providers;

    public HotelSearchService(IEnumerable<IHotelProvider> providers)
    {
        _providers = providers;
    }

    public async Task<HotelSearchResponse> SearchAsync(HotelSearchRequest request)
    {
        if (request.CheckOutDate <= request.CheckInDate)
        {
            throw new ArgumentException("CheckOut date must be after CheckIn date");
        }

        if (!KnownDestinations.Contains(request.Destination))
        {
            throw new KeyNotFoundException(
                $"Destination not recognized. Supported cities: New York, Los Angeles, New Delhi, Hyderabad, London, Paris, Tokyo");
        }

        var providerTasks = _providers.Select(p =>
            p.SearchAsync(request.Destination, request.CheckInDate, request.CheckOutDate, request.RoomType));

        var providerResults = await Task.WhenAll(providerTasks);

        var allRooms = providerResults
            .SelectMany(r => r)
            .Where(r => r.Available)
            .OrderBy(r => r.TotalPrice)
            .ToList();

        var nightsCount = (request.CheckOutDate - request.CheckInDate).Days;

        return new HotelSearchResponse
        {
            Results = allRooms,
            TotalCount = allRooms.Count,
            Destination = request.Destination,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            NightsCount = nightsCount,
            SortedBy = "TotalPrice_Asc"
        };
    }
}
