namespace HotelStay.Api.Models;

public class Reservation
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public ReservationRequest Request { get; set; } = new();
    public DateTime ReservedAt { get; set; }
    public decimal TotalPrice { get; set; }
    public CancellationPolicy CancellationPolicy { get; set; } = new();
}
