using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelStay.Api.Models;

namespace HotelStay.Api.Providers
{
    /// <summary>
    /// Adapter interface for hotel providers. Implementations should return provider-specific
    /// results normalized to the unified HotelRoom model. Results may include availability
    /// indicators which the caller must honour.
    /// </summary>
    public interface IHotelProvider
    {
        /// <summary>
        /// Provider display name ("PremierStays" or "BudgetNests").
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Search for available rooms for the specified destination and dates.
        /// Implementations may return rooms with Available==false; the caller should filter them.
        /// </summary>
        Task<List<HotelRoom>> SearchAsync(string destination, DateTime checkInDate, DateTime checkOutDate, RoomType? roomType = null);
    }
}
