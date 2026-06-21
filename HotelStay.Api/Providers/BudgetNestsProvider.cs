using HotelStay.Api.Models;

namespace HotelStay.Api.Providers;

public class BudgetNestsProvider : IHotelProvider
{
    public string ProviderName => "BudgetNests";

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
                HotelId = "budget_001",
                HotelName = "Budget Hostel & Rooms",
                Provider = ProviderName,
                RoomType = RoomType.Standard,
                PricePerNight = 60m,
                TotalNights = totalNights,
                CancellationPolicy = new CancellationPolicy
                {
                    Type = CancellationPolicyType.Flexible,
                    HoursBeforeCheckIn = 24
                },
                Amenities = null,
                StarRating = 0,
                Available = true,
                SearchDate = DateTime.UtcNow
            },
            new HotelRoom
            {
                HotelId = "budget_002",
                HotelName = "Economy Stay",
                Provider = ProviderName,
                RoomType = RoomType.Deluxe,
                PricePerNight = 95m,
                TotalNights = totalNights,
                CancellationPolicy = new CancellationPolicy
                {
                    Type = CancellationPolicyType.NonRefundable,
                    HoursBeforeCheckIn = 0
                },
                Amenities = null,
                StarRating = 0,
                Available = true,
                SearchDate = DateTime.UtcNow
            },
            new HotelRoom
            {
                HotelId = "budget_003",
                HotelName = "Economy Suite",
                Provider = ProviderName,
                RoomType = RoomType.Suite,
                PricePerNight = 150m,
                TotalNights = totalNights,
                CancellationPolicy = new CancellationPolicy
                {
                    Type = CancellationPolicyType.Flexible,
                    HoursBeforeCheckIn = 24
                },
                Amenities = null,
                StarRating = 0,
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
