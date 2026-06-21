using Xunit;
using HotelStay.Api.Services;
using HotelStay.Api.Models;
using HotelStay.Api.Providers;

namespace HotelStay.Tests.Services;

public class HotelSearchServiceTests
{
    private readonly HotelSearchService _service;
    private readonly DateTime _checkIn = new DateTime(2026, 6, 23);
    private readonly DateTime _checkOut = new DateTime(2026, 6, 27);

    public HotelSearchServiceTests()
    {
        var providers = new List<IHotelProvider>
        {
            new PremierStaysProvider(),
            new BudgetNestsProvider()
        };
        _service = new HotelSearchService(providers);
    }

    [Fact]
    public async Task SearchAsync_ValidRequest_ReturnsHotelSearchResponse()
    {
        var request = new HotelSearchRequest
        {
            Destination = "London",
            CheckInDate = _checkIn,
            CheckOutDate = _checkOut
        };

        var response = await _service.SearchAsync(request);

        Assert.NotNull(response);
        Assert.True(response.Results.Count > 0);
        Assert.True(response.TotalCount > 0);
    }

    [Fact]
    public async Task SearchAsync_ValidRequest_CalculatesTotalNights()
    {
        var request = new HotelSearchRequest
        {
            Destination = "London",
            CheckInDate = _checkIn,
            CheckOutDate = _checkOut
        };

        var response = await _service.SearchAsync(request);

        Assert.Equal(4, response.NightsCount);
    }

    [Fact]
    public async Task SearchAsync_ValidRequest_CalculatesTotalPrice()
    {
        var request = new HotelSearchRequest
        {
            Destination = "London",
            CheckInDate = _checkIn,
            CheckOutDate = _checkOut,
            RoomType = RoomType.Standard
        };

        var response = await _service.SearchAsync(request);

        var standardRoom = response.Results.FirstOrDefault(r => r.Provider == "BudgetNests");
        if (standardRoom != null)
        {
            Assert.Equal(standardRoom.PricePerNight * response.NightsCount, standardRoom.TotalPrice);
        }
    }

    [Fact]
    public async Task SearchAsync_WithRoomTypeFilter_ReturnsOnlyMatchingRoomType()
    {
        var request = new HotelSearchRequest
        {
            Destination = "London",
            CheckInDate = _checkIn,
            CheckOutDate = _checkOut,
            RoomType = RoomType.Deluxe
        };

        var response = await _service.SearchAsync(request);

        Assert.All(response.Results, r => Assert.Equal(RoomType.Deluxe, r.RoomType));
    }

    [Fact]
    public async Task SearchAsync_MergesResultsFromBothProviders()
    {
        var request = new HotelSearchRequest
        {
            Destination = "London",
            CheckInDate = _checkIn,
            CheckOutDate = _checkOut
        };

        var response = await _service.SearchAsync(request);

        var providers = response.Results.Select(r => r.Provider).Distinct().ToList();
        Assert.Contains("PremierStays", providers);
        Assert.Contains("BudgetNests", providers);
    }

    [Fact]
    public async Task SearchAsync_FiltersUnavailableRooms()
    {
        var request = new HotelSearchRequest
        {
            Destination = "London",
            CheckInDate = _checkIn,
            CheckOutDate = _checkOut
        };

        var response = await _service.SearchAsync(request);

        Assert.All(response.Results, r => Assert.True(r.Available));
    }

    [Fact]
    public async Task SearchAsync_SortsByTotalPrice_Ascending()
    {
        var request = new HotelSearchRequest
        {
            Destination = "London",
            CheckInDate = _checkIn,
            CheckOutDate = _checkOut
        };

        var response = await _service.SearchAsync(request);

        var prices = response.Results.Select(r => r.TotalPrice).ToList();
        for (int i = 1; i < prices.Count; i++)
        {
            Assert.True(prices[i] >= prices[i - 1]);
        }
    }

    [Fact]
    public async Task SearchAsync_InvalidDestination_ThrowsKeyNotFoundException()
    {
        var request = new HotelSearchRequest
        {
            Destination = "InvalidCity",
            CheckInDate = _checkIn,
            CheckOutDate = _checkOut
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.SearchAsync(request));
    }

    [Fact]
    public async Task SearchAsync_CheckOutBeforeCheckIn_ThrowsArgumentException()
    {
        var request = new HotelSearchRequest
        {
            Destination = "London",
            CheckInDate = new DateTime(2026, 6, 27),
            CheckOutDate = new DateTime(2026, 6, 23)
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.SearchAsync(request));
    }

    [Fact]
    public async Task SearchAsync_CheckOutEqualsCheckIn_ThrowsArgumentException()
    {
        var request = new HotelSearchRequest
        {
            Destination = "London",
            CheckInDate = _checkIn,
            CheckOutDate = _checkIn
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.SearchAsync(request));
    }

    [Fact]
    public async Task SearchAsync_NormalizesPremierStaysToUnifiedModel()
    {
        var request = new HotelSearchRequest
        {
            Destination = "London",
            CheckInDate = _checkIn,
            CheckOutDate = _checkOut
        };

        var response = await _service.SearchAsync(request);

        var premierRoom = response.Results.FirstOrDefault(r => r.Provider == "PremierStays");
        Assert.NotNull(premierRoom);
        Assert.NotEmpty(premierRoom!.HotelId);
        Assert.NotEmpty(premierRoom.HotelName);
    }

    [Fact]
    public async Task SearchAsync_ReturnsResponseWithCorrectMetadata()
    {
        var request = new HotelSearchRequest
        {
            Destination = "London",
            CheckInDate = _checkIn,
            CheckOutDate = _checkOut
        };

        var response = await _service.SearchAsync(request);

        Assert.Equal("London", response.Destination);
        Assert.Equal(_checkIn, response.CheckInDate);
        Assert.Equal(_checkOut, response.CheckOutDate);
        Assert.Equal(4, response.NightsCount);
        Assert.Equal("TotalPrice_Asc", response.SortedBy);
    }
}
