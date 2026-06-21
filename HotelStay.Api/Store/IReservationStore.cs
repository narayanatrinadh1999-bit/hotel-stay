using HotelStay.Api.Models;

namespace HotelStay.Api.Store;

public interface IReservationStore
{
    void Save(Reservation reservation);
    Reservation? GetByReference(string referenceNumber);
    bool Exists(string referenceNumber);
}
