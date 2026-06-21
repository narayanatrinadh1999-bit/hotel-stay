using System.Text.Json.Serialization;

namespace HotelStay.Api.Models
{
    /// <summary>
    /// Types of identity documents accepted by the reservation system.
    /// Serialized as string values in JSON.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DocumentType
    {
        /// <summary>
        /// Passport document.
        /// </summary>
        Passport,

        /// <summary>
        /// National identification document.
        /// </summary>
        NationalId
    }
}

