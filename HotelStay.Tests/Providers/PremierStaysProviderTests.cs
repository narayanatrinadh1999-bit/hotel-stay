using Xunit;
using HotelStay.Api.Providers;
using HotelStay.Api.Models;

namespace HotelStay.Tests.Providers;

public class PremierStaysProviderTests
{
    private readonly PremierStaysProvider _provider = new PremierStaysProvider();
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

        Assert.All(result, r => Assert.Equal("PremierStays", r.Provider));
    }

    [Fact]
    public async Task SearchAsync_AllRoomsAlwaysAvailable()
    {
        var result = await _provider.SearchAsync("London", _checkIn, _checkOut);

        Assert.All(result, r => Assert.True(r.Available));
    }

    [Theory]
    [InlineData(RoomType.Deluxe, 1)]
    [InlineData(RoomType.Standard, 1)]
    [InlineData(RoomType.Suite, 1)]
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
    public async Task SearchAsync_ReturnsAmenitiesAndStarRatings()
    {
        var result = await _provider.SearchAsync("London", _checkIn, _checkOut);

        Assert.All(result, r =>
        {
            Assert.NotNull(r.Amenities);
            Assert.True(r.StarRating > 0);
        });
    }

    [Fact]
    public async Task SearchAsync_ReturnsCorrectCancellationPolicies()
    {
        var result = await _provider.SearchAsync("London", _checkIn, _checkOut);

        var grandPlaza = result.First(r => r.HotelId == "premier_001");
        Assert.Equal(CancellationPolicyType.FreeCancellation, grandPlaza.CancellationPolicy.Type);
        Assert.Equal(48, grandPlaza.CancellationPolicy.HoursBeforeCheckIn);

        var cityCenter = result.First(r => r.HotelId == "premier_002");
        Assert.Equal(CancellationPolicyType.NonRefundable, cityCenter.CancellationPolicy.Type);
    }

    [Fact]
    public async Task SearchAsync_ReturnsNotNull()
    {
        var result = await _provider.SearchAsync("London", _checkIn, _checkOut);

        Assert.IsType<List<HotelRoom>>(result);
        Assert.NotNull(result);
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
    public async Task SearchAsync_WithNullRoomType_ReturnsAllRooms()
    {
        var result = await _provider.SearchAsync("London", _checkIn, _checkOut, null);

        Assert.Equal(3, result.Count);
    }
}
