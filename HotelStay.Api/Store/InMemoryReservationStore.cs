using HotelStay.Api.Models;

namespace HotelStay.Api.Store;

public class InMemoryReservationStore : IReservationStore
{
    private readonly Dictionary<string, Reservation> _reservations = new();

    public void Save(Reservation reservation)
    {
        _reservations[reservation.ReferenceNumber] = reservation;
    }

    public Reservation? GetByReference(string referenceNumber)
    {
        return _reservations.TryGetValue(referenceNumber, out var res) ? res : null;
    }

    public bool Exists(string referenceNumber)
    {
        return _reservations.ContainsKey(referenceNumber);
    }
}
