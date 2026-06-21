using HotelStay.Api.Models;

namespace HotelStay.Api.Providers;

public interface IHotelProvider
{
    string ProviderName { get; }

    Task<List<HotelRoom>> SearchAsync(
        string destination,
        DateTime checkInDate,
        DateTime checkOutDate,
        RoomType? roomType = null
    );
}
