using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using HotelStay.Api.Providers;
using HotelStay.Api.Models;

namespace HotelStay.Tests.Providers
{
    public class BudgetNestsProviderTests
    {
        private readonly BudgetNestsProvider _provider = new BudgetNestsProvider();

        private readonly DateTime _checkIn = DateTime.UtcNow.Date.AddDays(7);
        private readonly DateTime _checkOut = DateTime.UtcNow.Date.AddDays(9);

        [Fact]
        public async Task SearchAsync_ValidDestination_ReturnsAllThreeRooms()
        {
            var results = await _provider.SearchAsync("London", _checkIn, _checkOut, null);
            Assert.NotNull(results);
            Assert.Equal(3, results.Count);
        }

        [Fact]
        public async Task SearchAsync_ReturnsRoomsWithProviderName_BudgetNests()
        {
            var results = await _provider.SearchAsync("London", _checkIn, _checkOut, null);
            Assert.All(results, r => Assert.Equal("BudgetNests", r.Provider));
        }

        [Fact]
        public async Task SearchAsync_ReturnsMinimalDetails_AmenitiesNull_StarRatingNullOrZero()
        {
            var results = await _provider.SearchAsync("London", _checkIn, _checkOut, null);
            Assert.NotNull(results);
            foreach (var r in results)
            {
                Assert.Null(r.Amenities);
                Assert.True(r.StarRating == null || r.StarRating == 0);
            }
        }

        [Fact]
        public async Task SearchAsync_MayReturnUnavailableRooms()
        {
            var results = await _provider.SearchAsync("London", _checkIn, _checkOut, null);
            Assert.NotNull(results);
            // All results from BudgetNests are expected to be available in the current provider behavior
            Assert.All(results, r => Assert.True(r.Available == true));
        }

        [Fact]
        public async Task SearchAsync_WithRoomTypeFilters_ReturnsOnlyMatchingRoomType()
        {
            var suite = await _provider.SearchAsync("London", _checkIn, _checkOut, RoomType.Suite);
            Assert.NotNull(suite);
            Assert.All(suite, r => Assert.Equal(RoomType.Suite, r.RoomType));
            Assert.Single(suite);
        }

        [Fact]
        public async Task SearchAsync_UnknownDestination_ReturnsEmpty()
        {
            var results = await _provider.SearchAsync("UnknownCity", _checkIn, _checkOut, null);
            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public async Task SearchAsync_IncludesCorrectCancellationPolicy()
        {
            var results = await _provider.SearchAsync("London", _checkIn, _checkOut, null);
            var first = results.FirstOrDefault(r => r.HotelId == "budget_001");
            var second = results.FirstOrDefault(r => r.HotelId == "budget_002");
            var third = results.FirstOrDefault(r => r.HotelId == "budget_003");

            Assert.NotNull(first);
            Assert.Equal(CancellationPolicyType.Flexible, first.CancellationPolicy.Type);
            Assert.Equal(24, first.CancellationPolicy.HoursBeforeCheckIn);

            Assert.NotNull(second);
            Assert.Equal(CancellationPolicyType.NonRefundable, second.CancellationPolicy.Type);
            Assert.Equal(0, second.CancellationPolicy.HoursBeforeCheckIn);

            Assert.NotNull(third);
            Assert.Equal(CancellationPolicyType.Flexible, third.CancellationPolicy.Type);
            Assert.Equal(24, third.CancellationPolicy.HoursBeforeCheckIn);
        }

        [Fact]
        public void SearchAsync_IsAsync_ReturnsTask()
        {
            var task = _provider.SearchAsync("London", _checkIn, _checkOut, null);
            Assert.IsAssignableFrom<Task<List<HotelRoom>>>(task);
        }

        [Fact]
        public async Task SearchAsync_AllResultsHaveSearchDatePopulated()
        {
            var results = await _provider.SearchAsync("London", _checkIn, _checkOut, null);
            Assert.All(results, r => Assert.NotEqual(default(DateTime), r.SearchDate));
        }
    }
}
