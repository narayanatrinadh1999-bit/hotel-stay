namespace HotelStay.Api.Models;

public class HotelSearchRequest
{
    public string Destination { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public RoomType? RoomType { get; set; }
}
