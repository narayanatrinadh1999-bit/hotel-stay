using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelStay.Api.Models;
using HotelStay.Api.Providers;

namespace HotelStay.Api.Services
{
    /// <summary>
    /// Aggregates provider searches, normalizes and merges results.
    /// </summary>
    public class HotelSearchService : IHotelSearchService
    {
        private readonly IEnumerable<IHotelProvider> _providers;

        private static readonly HashSet<string> SupportedDestinations = new(StringComparer.OrdinalIgnoreCase)
        {
            "New Delhi",
            "Hyderabad",
            "London",
            "Paris",
            "Tokyo"
        };

        /// <summary>
        /// Create a new HotelSearchService.
        /// </summary>
        public HotelSearchService(IEnumerable<IHotelProvider> providers)
        {
            _providers = providers ?? throw new ArgumentNullException(nameof(providers));
        }

        /// <inheritdoc />
        public async Task<HotelSearchResponse> SearchAsync(HotelSearchRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.Destination))
            {
                throw new ArgumentException("Missing required parameter: destination");
            }

            if (!SupportedDestinations.Contains(request.Destination))
            {
                throw new KeyNotFoundException($"Destination not recognized. Supported cities: {string.Join(", ", SupportedDestinations)}");
            }

            if (request.CheckOutDate <= request.CheckInDate)
            {
                throw new ArgumentException("CheckOut date must be after CheckIn date");
            }

            var tasks = _providers.Select(p => p.SearchAsync(request.Destination, request.CheckInDate, request.CheckOutDate, request.RoomType));
            var results = await Task.WhenAll(tasks);

            var merged = new List<HotelRoom>();
            foreach (var list in results)
            {
                if (list == null) continue;
                merged.AddRange(list);
            }

            // Filter out unavailable rooms (Available == false)
            var filtered = merged.Where(r => r.Available != false).ToList();

            // Calculate totals
            var nights = (request.CheckOutDate - request.CheckInDate).Days;
            foreach (var room in filtered)
            {
                room.TotalNights = nights;
                // TotalPrice computed property will use PricePerNight * TotalNights
            }

            // Optional filter by room type
            if (request.RoomType.HasValue)
            {
                filtered = filtered.Where(r => r.RoomType == request.RoomType.Value).ToList();
            }

            // Sort by total price ascending
            filtered = filtered.OrderBy(r => r.TotalPrice).ToList();

            var response = new HotelSearchResponse
            {
                Results = filtered,
                TotalCount = filtered.Count,
                Destination = request.Destination,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                NightsCount = nights,
                SortedBy = "TotalPrice_Asc"
            };

            return response;
        }
    }
}
