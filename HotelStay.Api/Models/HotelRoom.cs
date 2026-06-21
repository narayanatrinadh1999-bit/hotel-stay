namespace HotelStay.Api.Models;

public class HotelRoom
{
    public string HotelId { get; set; } = string.Empty;
    public string HotelName { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;

    public RoomType RoomType { get; set; }
    public decimal PricePerNight { get; set; }
    public int TotalNights { get; set; }

    public decimal TotalPrice => PricePerNight * TotalNights;

    public CancellationPolicy CancellationPolicy { get; set; } = new();

    public string[]? Amenities { get; set; }
    public int StarRating { get; set; }

    public bool Available { get; set; }

    public DateTime SearchDate { get; set; }
}
