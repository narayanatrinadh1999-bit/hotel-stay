using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelStay.Api.Models;

namespace HotelStay.Api.Providers
{
    /// <summary>
    /// Stub provider returning full-detailed PascalCase data (PremierStays).
    /// Deterministic data for testing.
    /// </summary>
    public class PremierStaysProvider : IHotelProvider
    {
        public string ProviderName => "PremierStays";

        public Task<List<HotelRoom>> SearchAsync(string destination, DateTime checkInDate, DateTime checkOutDate, RoomType? roomType = null)
        {
            // Only return results for supported destinations
            var supported = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "New Delhi",
                "Hyderabad",
                "London",
                "Paris",
                "Tokyo"
            };

            if (!supported.Contains(destination))
            {
                return Task.FromResult(new List<HotelRoom>());
            }

            var now = DateTime.UtcNow;
            var list = new List<HotelRoom>
            {
                new HotelRoom
                {
                    HotelId = "premier_001",
                    HotelName = "Grand Plaza Hotel",
                    Provider = ProviderName,
                    RoomType = RoomType.Deluxe,
                    PricePerNight = 250m,
                    Amenities = new[] { "WiFi", "Pool", "Spa", "Gym" },
                    StarRating = 5,
                    CancellationPolicy = new CancellationPolicy { Type = CancellationPolicyType.FreeCancellation, HoursBeforeCheckIn = 48 },
                    Available = true,
                    SearchDate = now
                },
                new HotelRoom
                {
                    HotelId = "premier_002",
                    HotelName = "City Center Inn",
                    Provider = ProviderName,
                    RoomType = RoomType.Standard,
                    PricePerNight = 120m,
                    Amenities = new[] { "WiFi", "Breakfast" },
                    StarRating = 4,
                    CancellationPolicy = new CancellationPolicy { Type = CancellationPolicyType.NonRefundable, HoursBeforeCheckIn = 0 },
                    Available = true,
                    SearchDate = now
                },
                new HotelRoom
                {
                    HotelId = "premier_003",
                    HotelName = "Seaside Suites",
                    Provider = ProviderName,
                    RoomType = RoomType.Suite,
                    PricePerNight = 400m,
                    Amenities = new[] { "WiFi", "Ocean View", "Breakfast" },
                    StarRating = 5,
                    CancellationPolicy = new CancellationPolicy { Type = CancellationPolicyType.FreeCancellation, HoursBeforeCheckIn = 48 },
                    Available = true,
                    SearchDate = now
                }
            };

            if (roomType.HasValue)
            {
                list = list.FindAll(r => r.RoomType == roomType.Value);
            }

            return Task.FromResult(list);
        }
    }
}
