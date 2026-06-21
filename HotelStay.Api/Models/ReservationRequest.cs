namespace HotelStay.Api.Models;

public class ReservationRequest
{
    public string HotelId { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public RoomType RoomType { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }

    public string GuestName { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;

    // Additional fields provided by caller (from search results)
    public string Destination { get; set; } = string.Empty;
    public string HotelName { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public CancellationPolicy? CancellationPolicy { get; set; }
}
