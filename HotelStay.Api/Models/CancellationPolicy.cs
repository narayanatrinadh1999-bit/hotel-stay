namespace HotelStay.Api.Models;

public enum CancellationPolicyType
{
    FreeCancellation,
    Flexible,
    NonRefundable
}

public class CancellationPolicy
{
    public CancellationPolicyType Type { get; set; }
    public int HoursBeforeCheckIn { get; set; }
}
