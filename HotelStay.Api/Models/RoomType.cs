using System.Text.Json.Serialization;

namespace HotelStay.Api.Models
{
    /// <summary>
    /// Unified enum for room types used across providers and the API.
    /// Matches the values returned by providers after normalization.
    /// Serialized as string values in JSON.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RoomType
    {
        /// <summary>
        /// Budget / standard room.
        /// </summary>
        Standard,

        /// <summary>
        /// Mid-range room with additional amenities.
        /// </summary>
        Deluxe,

        /// <summary>
        /// Premium room or multi-room suite.
        /// </summary>
        Suite
    }
}
