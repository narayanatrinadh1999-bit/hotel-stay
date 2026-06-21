namespace HotelStay.Api.Models;

public class ReservationConfirmation
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateTime ReservedAt { get; set; }

    public string HotelName { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public RoomType RoomType { get; set; }

    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NightsCount { get; set; }

    public decimal PricePerNight { get; set; }
    public decimal TotalPrice { get; set; }

    public CancellationPolicy CancellationPolicy { get; set; } = new();

    public string GuestName { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
}
