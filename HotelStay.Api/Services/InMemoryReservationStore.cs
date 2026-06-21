using System.Collections.Concurrent;
using HotelStay.Api.Models;

namespace HotelStay.Api.Services
{
    /// <summary>
    /// Thread-safe in-memory reservation store for demo purposes.
    /// </summary>
    public class InMemoryReservationStore : IReservationStore
    {
        private readonly ConcurrentDictionary<string, ReservationConfirmation> _store = new();

        /// <inheritdoc />
        public void Save(string referenceNumber, ReservationConfirmation confirmation)
        {
            _store[referenceNumber] = confirmation;
        }

        /// <inheritdoc />
        public ReservationConfirmation GetByReference(string referenceNumber)
        {
            return _store.TryGetValue(referenceNumber, out var conf) ? conf : null;
        }

        /// <inheritdoc />
        public bool Exists(string referenceNumber)
        {
            return _store.ContainsKey(referenceNumber);
        }
    }
}
