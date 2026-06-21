using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelStay.Api.Models;

namespace HotelStay.Api.Providers
{
    /// <summary>
    /// Stub provider returning minimal snake_case-like data (BudgetNests).
    /// Some results may be unavailable and must be filtered by the caller.
    /// </summary>
    public class BudgetNestsProvider : IHotelProvider
    {
        public string ProviderName => "BudgetNests";

        public Task<List<HotelRoom>> SearchAsync(string destination, DateTime checkInDate, DateTime checkOutDate, RoomType? roomType = null)
        {
            var now = DateTime.UtcNow;
            var list = new List<HotelRoom>
            {
                new HotelRoom
                {
                    HotelId = "budget_001",
                    HotelName = "Budget Hostel & Rooms",
                    Provider = ProviderName,
                    RoomType = RoomType.Standard,
                    PricePerNight = 60m,
                    Amenities = null,
                    StarRating = null,
                    CancellationPolicy = new CancellationPolicy { Type = CancellationPolicyType.Flexible, HoursBeforeCheckIn = 24 },
                    Available = true,
                    SearchDate = now
                },
                new HotelRoom
                {
                    HotelId = "budget_002",
                    HotelName = "Economy Stay",
                    Provider = ProviderName,
                    RoomType = RoomType.Deluxe,
                    PricePerNight = 95m,
                    Amenities = null,
                    StarRating = null,
                    CancellationPolicy = new CancellationPolicy { Type = CancellationPolicyType.NonRefundable, HoursBeforeCheckIn = 0 },
                    Available = false, // unavailable, caller must filter
                    SearchDate = now
                },
                new HotelRoom
                {
                    HotelId = "budget_003",
                    HotelName = "Economy Suite",
                    Provider = ProviderName,
                    RoomType = RoomType.Suite,
                    PricePerNight = 150m,
                    Amenities = null,
                    StarRating = null,
                    CancellationPolicy = new CancellationPolicy { Type = CancellationPolicyType.Flexible, HoursBeforeCheckIn = 24 },
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
