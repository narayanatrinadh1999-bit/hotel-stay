using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HotelStay.Api.Models;

namespace HotelStay.Api.Services
{
    /// <summary>
    /// Handles creating and retrieving reservations.
    /// </summary>
    public class ReservationService : IReservationService
    {
        private readonly IDocumentValidator _validator;
        private readonly IReservationStore _store;

        /// <summary>
        /// Create a new instance of the ReservationService.
        /// </summary>
        public ReservationService(IDocumentValidator validator, IReservationStore store)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        /// <inheritdoc />
        public Task<ReservationConfirmation> GetReservationAsync(string referenceNumber)
        {
            var conf = _store.GetByReference(referenceNumber);
            if (conf == null)
            {
                throw new KeyNotFoundException($"Reservation not found: {referenceNumber}");
            }

            return Task.FromResult(conf);
        }

        /// <inheritdoc />
        public Task<ReservationConfirmation> ReserveAsync(ReservationRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Destination must be supplied on the request to perform document validation.
            if (string.IsNullOrWhiteSpace(request.Destination))
            {
                throw new ArgumentException("Missing required field: destination");
            }

            var (isValid, error) = _validator.ValidateDocument(request.Destination, request.DocumentType);
            if (!isValid)
            {
                // Return a domain-level error by throwing an InvalidOperationException with message.
                // The endpoint will translate this into a 422 response.
                throw new InvalidOperationException(error);
            }

            // Generate unique reference: RES-YYYYMMDD-XXXXXX (hex)
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var randomHex = GenerateRandomHex(6);
            var reference = $"RES-{date}-{randomHex}";

            var nights = (request.CheckOutDate - request.CheckInDate).Days;

            var masked = MaskDocumentNumber(request.DocumentNumber);

            var confirmation = new ReservationConfirmation
            {
                ReferenceNumber = reference,
                ReservedAt = DateTime.UtcNow,
                HotelName = request.HotelId, // HotelName not available in request; store HotelId to help lookup
                Provider = request.Provider,
                RoomType = request.RoomType,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                NightsCount = nights,
                PricePerNight = 0m, // Unknown without provider pricing; caller may enrich if needed
                TotalPrice = 0m,
                CancellationPolicy = new CancellationPolicy { Type = CancellationPolicyType.NonRefundable, HoursBeforeCheckIn = 0 },
                GuestName = request.GuestName,
                DocumentType = request.DocumentType,
                DocumentNumber = masked
            };

            _store.Save(reference, confirmation);

            return Task.FromResult(confirmation);
        }

        private static string MaskDocumentNumber(string doc)
        {
            if (string.IsNullOrEmpty(doc)) return "";
            if (doc.Length <= 4) return doc + "****";
            return doc.Substring(0, 4) + "****";
        }

        private static string GenerateRandomHex(int length)
        {
            var bytes = new byte[(length + 1) / 2];
            RandomNumberGenerator.Fill(bytes);
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            var hex = sb.ToString();
            return hex.Substring(0, length);
        }
    }
}
