using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using HotelStay.Api.Providers;
using HotelStay.Api.Models;

namespace HotelStay.Tests.Providers
{
    public class PremierStaysProviderTests
    {
        private readonly PremierStaysProvider _provider = new PremierStaysProvider();

        private readonly DateTime _checkIn = DateTime.UtcNow.Date.AddDays(7);
        private readonly DateTime _checkOut = DateTime.UtcNow.Date.AddDays(9);

        [Fact]
        public async Task SearchAsync_ValidDestination_ReturnsAllThreeRooms()
        {
            var results = await _provider.SearchAsync("London", _checkIn, _checkOut, null);

            Assert.NotNull(results);
            Assert.Equal(3, results.Count);
            var ids = results.Select(r => r.HotelId).ToList();
            Assert.Contains("premier_001", ids);
            Assert.Contains("premier_002", ids);
            Assert.Contains("premier_003", ids);
        }

        [Fact]
        public async Task SearchAsync_ReturnsRoomsWithProviderName_PremierStays()
        {
            var results = await _provider.SearchAsync("London", _checkIn, _checkOut, null);
            Assert.All(results, r => Assert.Equal("PremierStays", r.Provider));
        }

        [Fact]
        public async Task SearchAsync_AlwaysReturnsAvailableTrue()
        {
            var results = await _provider.SearchAsync("London", _checkIn, _checkOut, null);
            Assert.All(results, r => Assert.True(r.Available == true));
        }

        [Fact]
        public async Task SearchAsync_WithRoomTypeFilters_ReturnsOnlyMatchingRoomType()
        {
            var deluxe = await _provider.SearchAsync("London", _checkIn, _checkOut, RoomType.Deluxe);
            Assert.NotNull(deluxe);
            Assert.All(deluxe, r => Assert.Equal(RoomType.Deluxe, r.RoomType));

            var standard = await _provider.SearchAsync("London", _checkIn, _checkOut, RoomType.Standard);
            Assert.NotNull(standard);
            Assert.All(standard, r => Assert.Equal(RoomType.Standard, r.RoomType));

            var suite = await _provider.SearchAsync("London", _checkIn, _checkOut, RoomType.Suite);
            Assert.NotNull(suite);
            Assert.All(suite, r => Assert.Equal(RoomType.Suite, r.RoomType));
        }

        [Fact]
        public async Task SearchAsync_UnknownDestination_ReturnsEmpty()
        {
            var results = await _provider.SearchAsync("UnknownCity", _checkIn, _checkOut, null);
            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public async Task SearchAsync_ReturnsAmenitiesAndStarRatings()
        {
            var results = await _provider.SearchAsync("London", _checkIn, _checkOut, null);
            var g = results.FirstOrDefault(r => r.HotelId == "premier_001");
            Assert.NotNull(g);
            Assert.NotNull(g.Amenities);
            Assert.Contains("WiFi", g.Amenities);
            Assert.True(g.StarRating.HasValue && g.StarRating.Value >= 4);
        }

        [Fact]
        public async Task SearchAsync_IncludesCorrectCancellationPolicy()
        {
            var results = await _provider.SearchAsync("London", _checkIn, _checkOut, null);
            var first = results.FirstOrDefault(r => r.HotelId == "premier_001");
            var second = results.FirstOrDefault(r => r.HotelId == "premier_002");
            var third = results.FirstOrDefault(r => r.HotelId == "premier_003");

            Assert.NotNull(first);
            Assert.Equal(CancellationPolicyType.FreeCancellation, first.CancellationPolicy.Type);
            Assert.Equal(48, first.CancellationPolicy.HoursBeforeCheckIn);

            Assert.NotNull(second);
            Assert.Equal(CancellationPolicyType.NonRefundable, second.CancellationPolicy.Type);
            Assert.Equal(0, second.CancellationPolicy.HoursBeforeCheckIn);

            Assert.NotNull(third);
            Assert.Equal(CancellationPolicyType.FreeCancellation, third.CancellationPolicy.Type);
            Assert.Equal(48, third.CancellationPolicy.HoursBeforeCheckIn);
        }

        [Fact]
        public async Task SearchAsync_ReturnsListNotNullAndCorrectType()
        {
            var results = await _provider.SearchAsync("London", _checkIn, _checkOut, null);
            Assert.NotNull(results);
            Assert.IsType<List<HotelRoom>>(results);
        }

        [Fact]
        public void SearchAsync_IsAsync_ReturnsTask()
        {
            var task = _provider.SearchAsync("London", _checkIn, _checkOut, null);
            Assert.IsAssignableFrom<Task<List<HotelRoom>>>(task);
        }

        [Fact]
        public async Task SearchAsync_NullRoomType_ReturnsAllRooms()
        {
            var results = await _provider.SearchAsync("London", _checkIn, _checkOut, null);
            Assert.NotNull(results);
            Assert.Equal(3, results.Count);
        }
    }
}
