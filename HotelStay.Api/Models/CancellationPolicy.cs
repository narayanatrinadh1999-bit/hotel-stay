using System.Text.Json.Serialization;

namespace HotelStay.Api.Models
{
    /// <summary>
    /// Types of cancellation policies supported by the unified model.
    /// Serialized as string values in JSON.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CancellationPolicyType
    {
        /// <summary>
        /// Free cancellation up to a configured number of hours before check-in.
        /// </summary>
        FreeCancellation,

        /// <summary>
        /// Flexible cancellation up to a configured number of hours before check-in.
        /// </summary>
        Flexible,

        /// <summary>
        /// No refunds allowed.
        /// </summary>
        NonRefundable
    }

    /// <summary>
    /// Represents a unified cancellation policy used in search results and reservation confirmations.
    ///
    /// Examples:
    /// - FreeCancellation, 48  => "Free cancellation up to 48 hours before check-in"
    /// - Flexible, 24          => "Flexible cancellation up to 24 hours before check-in"
    /// - NonRefundable, 0      => "Non-refundable"
    /// </summary>
    public class CancellationPolicy
    {
        /// <summary>
        /// The policy type (FreeCancellation, Flexible, or NonRefundable).
        /// </summary>
        public CancellationPolicyType Type { get; set; }

        /// <summary>
        /// Number of hours before check-in that the policy applies. For NonRefundable this should be 0.
        /// Example: 48 = "Free cancellation up to 48h before check-in".
        /// </summary>
        public int HoursBeforeCheckIn { get; set; }

        /// <summary>
        /// Returns a human-readable description of the cancellation policy.
        /// </summary>
        public override string ToString()
        {
            return Type == CancellationPolicyType.NonRefundable
                ? "Non-refundable"
                : $"{Type} up to {HoursBeforeCheckIn} hours before check-in";
        }
    }
}

