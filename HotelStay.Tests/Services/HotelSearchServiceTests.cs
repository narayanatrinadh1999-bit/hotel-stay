using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using HotelStay.Api.Services;
using HotelStay.Api.Providers;
using HotelStay.Api.Models;

namespace HotelStay.Tests.Services
{
    public class HotelSearchServiceTests
    {
        private IHotelSearchService CreateWithRealProviders()
        {
            var providers = new List<IHotelProvider>
            {
                new PremierStaysProvider(),
                new BudgetNestsProvider()
            };
            return new HotelSearchService(providers);
        }

        [Fact]
        public async Task SearchAsync_ValidRequest_ReturnsHotelSearchResponse()
        {
            var svc = CreateWithRealProviders();
            var req = new HotelSearchRequest
            {
                Destination = "London",
                CheckInDate = new DateTime(2026,6,23),
                CheckOutDate = new DateTime(2026,6,27)
            };

            var resp = await svc.SearchAsync(req);

            Assert.NotNull(resp);
            Assert.NotEmpty(resp.Results);
            Assert.True(resp.TotalCount > 0);
        }

        [Fact]
        public async Task SearchAsync_ValidRequest_CalculatesTotalNights()
        {
            var svc = CreateWithRealProviders();
            var req = new HotelSearchRequest
            {
                Destination = "London",
                CheckInDate = new DateTime(2026,6,23),
                CheckOutDate = new DateTime(2026,6,27)
            };

            var resp = await svc.SearchAsync(req);
            Assert.Equal(4, resp.NightsCount);
            Assert.All(resp.Results, r => Assert.Equal(4, r.TotalNights));
        }

        [Fact]
        public async Task SearchAsync_ValidRequest_CalculatesTotalPrice()
        {
            // Create a test provider that returns a single room priced at 100
            var provider = new TestPriceProvider();
            var svc = new HotelSearchService(new[] { provider });

            var req = new HotelSearchRequest
            {
                Destination = "London",
                CheckInDate = new DateTime(2026,6,23),
                CheckOutDate = new DateTime(2026,6,27)
            };

            var resp = await svc.SearchAsync(req);
            Assert.Single(resp.Results);
            var room = resp.Results[0];
            Assert.Equal(100m, room.PricePerNight);
            Assert.Equal(4, room.TotalNights);
            Assert.Equal(400m, room.TotalPrice);
        }

        [Fact]
        public async Task SearchAsync_WithRoomTypeFilter_ReturnsOnlyMatchingRoomType()
        {
            var svc = CreateWithRealProviders();
            var req = new HotelSearchRequest
            {
                Destination = "London",
                CheckInDate = new DateTime(2026,6,23),
                CheckOutDate = new DateTime(2026,6,27),
                RoomType = RoomType.Deluxe
            };

            var resp = await svc.SearchAsync(req);
            Assert.NotEmpty(resp.Results);
            Assert.All(resp.Results, r => Assert.Equal(RoomType.Deluxe, r.RoomType));
        }

        [Fact]
        public async Task SearchAsync_MergesResultsFromBothProviders()
        {
            var svc = CreateWithRealProviders();
            var req = new HotelSearchRequest
            {
                Destination = "London",
                CheckInDate = new DateTime(2026,6,23),
                CheckOutDate = new DateTime(2026,6,27)
            };

            var resp = await svc.SearchAsync(req);
            var providers = resp.Results.Select(r => r.Provider).Distinct().ToList();
            Assert.Contains("PremierStays", providers);
            Assert.Contains("BudgetNests", providers);
        }

        [Fact]
        public async Task SearchAsync_FiltersUnavailableRooms()
        {
            var svc = CreateWithRealProviders();
            var req = new HotelSearchRequest
            {
                Destination = "London",
                CheckInDate = new DateTime(2026,6,23),
                CheckOutDate = new DateTime(2026,6,27)
            };

            var resp = await svc.SearchAsync(req);
            // Ensure no result has Available == false
            Assert.All(resp.Results, r => Assert.True(r.Available != false));
        }

        [Fact]
        public async Task SearchAsync_SortsByTotalPrice_Ascending()
        {
            var svc = CreateWithRealProviders();
            var req = new HotelSearchRequest
            {
                Destination = "London",
                CheckInDate = new DateTime(2026,6,23),
                CheckOutDate = new DateTime(2026,6,27)
            };

            var resp = await svc.SearchAsync(req);
            var prices = resp.Results.Select(r => r.TotalPrice).ToList();
            var sorted = prices.OrderBy(x => x).ToList();
            Assert.Equal(sorted, prices);
        }

        [Fact]
        public async Task SearchAsync_InvalidDestination_ThrowsKeyNotFoundException()
        {
            var svc = CreateWithRealProviders();
            var req = new HotelSearchRequest
            {
                Destination = "InvalidCity",
                CheckInDate = new DateTime(2026,6,23),
                CheckOutDate = new DateTime(2026,6,27)
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.SearchAsync(req));
        }

        [Fact]
        public async Task SearchAsync_CheckOutBeforeCheckIn_ThrowsArgumentException()
        {
            var svc = CreateWithRealProviders();
            var req = new HotelSearchRequest
            {
                Destination = "London",
                CheckInDate = new DateTime(2026,6,27),
                CheckOutDate = new DateTime(2026,6,23)
            };

            await Assert.ThrowsAsync<ArgumentException>(() => svc.SearchAsync(req));
        }

        [Fact]
        public async Task SearchAsync_InvalidDateFormat_ThrowsArgumentException()
        {
            // Simulate invalid/unspecified dates by using default(DateTime)
            var svc = CreateWithRealProviders();
            var req = new HotelSearchRequest
            {
                Destination = "London",
                CheckInDate = default,
                CheckOutDate = default
            };

            await Assert.ThrowsAsync<ArgumentException>(() => svc.SearchAsync(req));
        }

        [Fact]
        public async Task SearchAsync_NormalizesPascalCaseToUnified_PremierStays()
        {
            var svc = CreateWithRealProviders();
            var req = new HotelSearchRequest
            {
                Destination = "London",
                CheckInDate = new DateTime(2026,6,23),
                CheckOutDate = new DateTime(2026,6,27)
            };

            var resp = await svc.SearchAsync(req);
            var premier = resp.Results.FirstOrDefault(r => r.Provider == "PremierStays");
            Assert.NotNull(premier);
            Assert.NotNull(premier.Amenities);
            Assert.True(premier.StarRating.HasValue);
        }

        [Fact]
        public async Task SearchAsync_ReturnsResponseWithCorrectMetadata()
        {
            var svc = CreateWithRealProviders();
            var req = new HotelSearchRequest
            {
                Destination = "London",
                CheckInDate = new DateTime(2026,6,23),
                CheckOutDate = new DateTime(2026,6,27)
            };

            var resp = await svc.SearchAsync(req);
            Assert.Equal(req.Destination, resp.Destination);
            Assert.Equal(req.CheckInDate, resp.CheckInDate);
            Assert.Equal(req.CheckOutDate, resp.CheckOutDate);
            Assert.Equal((req.CheckOutDate - req.CheckInDate).Days, resp.NightsCount);
            Assert.Equal("TotalPrice_Asc", resp.SortedBy);
        }

        // Helper provider for price/total price assertion
        private class TestPriceProvider : IHotelProvider
        {
            public string ProviderName => "TestPrice";

            public Task<List<HotelRoom>> SearchAsync(string destination, DateTime checkInDate, DateTime checkOutDate, RoomType? roomType = null)
            {
                var now = DateTime.UtcNow;
                var room = new HotelRoom
                {
                    HotelId = "test_100",
                    HotelName = "Test Hotel",
                    Provider = ProviderName,
                    RoomType = roomType ?? RoomType.Standard,
                    PricePerNight = 100m,
                    Available = true,
                    SearchDate = now
                };
                return Task.FromResult(new List<HotelRoom> { room });
            }
        }
    }
}
