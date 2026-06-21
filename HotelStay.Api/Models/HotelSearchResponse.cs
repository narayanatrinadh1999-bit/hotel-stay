namespace HotelStay.Api.Models;

public class HotelSearchResponse
{
    public List<HotelRoom> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public string Destination { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NightsCount { get; set; }
    public string SortedBy { get; set; } = string.Empty;
}
