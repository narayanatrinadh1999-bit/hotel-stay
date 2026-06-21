using Xunit;
using HotelStay.Api.Providers;
using HotelStay.Api.Models;

namespace HotelStay.Tests.Providers;

public class BudgetNestsProviderTests
{
    private readonly BudgetNestsProvider _provider = new BudgetNestsProvider();
    private readonly DateTime _checkIn = new DateTime(2026, 6, 23);
    private readonly DateTime _checkOut = new DateTime(2026, 6, 27);

    [Fact]
    public async Task SearchAsync_ValidRequest_ReturnsAllThreeRooms()
    {
        var result = await _provider.SearchAsync("London", _checkIn, _checkOut);

        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task SearchAsync_ReturnsCorrectProviderName()
    {
        var result = await _provider.SearchAsync("London", _checkIn, _checkOut);

        Assert.All(result, r => Assert.Equal("BudgetNests", r.Provider));
    }

    [Fact]
    public async Task SearchAsync_ReturnsMinimalDetails()
    {
        var result = await _provider.SearchAsync("London", _checkIn, _checkOut);

        Assert.All(result, r =>
        {
            Assert.Null(r.Amenities);
            Assert.Equal(0, r.StarRating);
        });
    }

    [Fact]
    public async Task SearchAsync_AllRoomsAvailable()
    {
        var result = await _provider.SearchAsync("London", _checkIn, _checkOut);

        Assert.All(result, r => Assert.True(r.Available));
    }

    [Theory]
    [InlineData(RoomType.Suite, 1)]
    [InlineData(RoomType.Standard, 1)]
    [InlineData(RoomType.Deluxe, 1)]
    public async Task SearchAsync_WithRoomTypeFilter_ReturnsOnlyMatchingRooms(RoomType roomType, int expectedCount)
    {
        var result = await _provider.SearchAsync("London", _checkIn, _checkOut, roomType);

        Assert.Equal(expectedCount, result.Count);
        Assert.All(result, r => Assert.Equal(roomType, r.RoomType));
    }

    [Fact]
    public async Task SearchAsync_AnyDestination_ReturnsAllRooms()
    {
        var result = await _provider.SearchAsync("UnknownCity", _checkIn, _checkOut);

        Assert.NotEmpty(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task SearchAsync_ReturnsCorrectCancellationPolicies()
    {
        var result = await _provider.SearchAsync("London", _checkIn, _checkOut);

        var budget001 = result.First(r => r.HotelId == "budget_001");
        Assert.Equal(CancellationPolicyType.Flexible, budget001.CancellationPolicy.Type);
        Assert.Equal(24, budget001.CancellationPolicy.HoursBeforeCheckIn);

        var budget002 = result.First(r => r.HotelId == "budget_002");
        Assert.Equal(CancellationPolicyType.NonRefundable, budget002.CancellationPolicy.Type);
    }

    [Fact]
    public async Task SearchAsync_WithNullRoomType_ReturnsAllRooms()
    {
        var result = await _provider.SearchAsync("London", _checkIn, _checkOut, null);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task SearchAsync_IsAsync()
    {
        var task = _provider.SearchAsync("London", _checkIn, _checkOut);

        Assert.NotNull(task);
        var result = await task;
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SearchAsync_AllRoomsHaveSearchDatePopulated()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var result = await _provider.SearchAsync("London", _checkIn, _checkOut);
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.All(result, r => Assert.InRange(r.SearchDate, before, after));
    }
}
