using HotelStay.Api.Models;

namespace HotelStay.Api.Providers;

public class PremierStaysProvider : IHotelProvider
{
    public string ProviderName => "PremierStays";

    public Task<List<HotelRoom>> SearchAsync(
        string destination,
        DateTime checkInDate,
        DateTime checkOutDate,
        RoomType? roomType = null)
    {
        var totalNights = (checkOutDate - checkInDate).Days;

        var rooms = new List<HotelRoom>
        {
            new HotelRoom
            {
                HotelId = "premier_001",
                HotelName = "Grand Plaza Hotel",
                Provider = ProviderName,
                RoomType = RoomType.Deluxe,
                PricePerNight = 250m,
                TotalNights = totalNights,
                CancellationPolicy = new CancellationPolicy
                {
                    Type = CancellationPolicyType.FreeCancellation,
                    HoursBeforeCheckIn = 48
                },
                Amenities = new[] { "WiFi", "Pool", "Spa", "Gym" },
                StarRating = 5,
                Available = true,
                SearchDate = DateTime.UtcNow
            },
            new HotelRoom
            {
                HotelId = "premier_002",
                HotelName = "City Center Inn",
                Provider = ProviderName,
                RoomType = RoomType.Standard,
                PricePerNight = 120m,
                TotalNights = totalNights,
                CancellationPolicy = new CancellationPolicy
                {
                    Type = CancellationPolicyType.NonRefundable,
                    HoursBeforeCheckIn = 0
                },
                Amenities = new[] { "WiFi", "Breakfast" },
                StarRating = 4,
                Available = true,
                SearchDate = DateTime.UtcNow
            },
            new HotelRoom
            {
                HotelId = "premier_003",
                HotelName = "Seaside Suites",
                Provider = ProviderName,
                RoomType = RoomType.Suite,
                PricePerNight = 400m,
                TotalNights = totalNights,
                CancellationPolicy = new CancellationPolicy
                {
                    Type = CancellationPolicyType.FreeCancellation,
                    HoursBeforeCheckIn = 48
                },
                Amenities = new[] { "WiFi", "Pool", "Spa", "Gym", "Ocean View" },
                StarRating = 5,
                Available = true,
                SearchDate = DateTime.UtcNow
            }
        };

        if (roomType.HasValue)
        {
            rooms = rooms.Where(r => r.RoomType == roomType.Value).ToList();
        }

        return Task.FromResult(rooms);
    }
}
