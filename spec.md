# Hotel Stay Availability Specification

## Overview
The Hotel Availability feature enables travelers to search for available hotel rooms across multiple providers, view normalized results, and make reservations with document validation. The system acts as an aggregator, querying two stub providers (PremierStays and BudgetNests), normalizing responses, and presenting a unified interface.

---

## 1. Domain Models

### 1.1 Room Type (Enum)
Unified enum for room types across all providers.

```csharp
public enum RoomType
{
    Standard,  // Budget rooms
    Deluxe,    // Mid-range rooms
    Suite      // Premium rooms
}
```

**Mappings:**
- PremierStays: Uses exact names (Standard, Deluxe, Suite)
- BudgetNests: Uses exact names (Standard, Deluxe, Suite)

---

### 1.2 Cancellation Policy (Enum)
Unified cancellation policy enum across providers.

```csharp
public enum CancellationPolicyType
{
    FreeCancellation,   // Free cancellation up to specified hours before check-in
    Flexible,           // Flexible cancellation up to specified hours before check-in
    NonRefundable       // No refunds allowed
}

public class CancellationPolicy
{
    public CancellationPolicyType Type { get; set; }
    public int HoursBeforeCheckIn { get; set; }  // e.g., 48 for "48 hours before check-in"
    
    // Examples:
    // FreeCancellation, 48 = "Free cancellation up to 48h before check-in"
    // Flexible, 24 = "Flexible cancellation up to 24h before check-in"
    // NonRefundable, 0 = "Non-refundable"
}
```

**Provider Mappings:**

**PremierStays (Full detail):**
- `"FreeCancellation"` + `48` hours → `CancellationPolicyType.FreeCancellation, 48`
- `"NonRefundable"` → `CancellationPolicyType.NonRefundable, 0`

**BudgetNests (Minimal):**
- `"Flexible"` + `24` hours → `CancellationPolicyType.Flexible, 24`
- `"NonRefundable"` → `CancellationPolicyType.NonRefundable, 0`

---

### 1.3 Hotel Room (Unified Model)

```csharp
public class HotelRoom
{
    public string HotelId { get; set; }              // Unique per provider
    public string HotelName { get; set; }            // Normalized from provider
    public string Provider { get; set; }             // "PremierStays" or "BudgetNests"
    
    public RoomType RoomType { get; set; }           // Standard, Deluxe, Suite
    public decimal PricePerNight { get; set; }       // Single night rate
    public int TotalNights { get; set; }             // Calculated from check-in/check-out
    
    // Calculated on frontend/response: TotalPrice = PricePerNight * TotalNights
    public decimal TotalPrice => PricePerNight * TotalNights;
    
    public CancellationPolicy CancellationPolicy { get; set; }
    
    // Optional details (PremierStays only; null for BudgetNests)
    public string[] Amenities { get; set; }          // e.g., ["WiFi", "Pool", "Gym"]
    public int StarRating { get; set; }              // e.g., 5 (null/0 for BudgetNests)
    
    public DateTime SearchDate { get; set; }         // When this room was searched
}
```

---

### 1.4 Search Request

```csharp
public class HotelSearchRequest
{
    public string Destination { get; set; }          // Required: city name
    public DateTime CheckInDate { get; set; }        // Required: YYYY-MM-DD
    public DateTime CheckOutDate { get; set; }       // Required: YYYY-MM-DD, must be > CheckInDate
    public RoomType? RoomType { get; set; }          // Optional: filter by room type
}
```

**Validation Rules:**
- `Destination`: Required, non-empty
- `CheckInDate`: Required, valid date
- `CheckOutDate`: Required, must be strictly greater than CheckInDate
- `RoomType`: Optional; if provided, filter results to that type only

---

### 1.5 Search Response

```csharp
public class HotelSearchResponse
{
    public List<HotelRoom> Results { get; set; }
    public int TotalCount { get; set; }
    public string Destination { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NightsCount { get; set; }             // Calculated: (CheckOut - CheckIn).Days
    
    // Client-side sorting indicators
    public string SortedBy { get; set; }              // e.g., "TotalPrice_Asc"
}
```

---

### 1.6 Reservation Request

```csharp
public class ReservationRequest
{
    public string HotelId { get; set; }              // Hotel to reserve
    public string Provider { get; set; }             // "PremierStays" or "BudgetNests"
    public RoomType RoomType { get; set; }           // Room type being reserved
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    
    public string GuestName { get; set; }            // Required
    public DocumentType DocumentType { get; set; }   // Passport or NationalId
    public string DocumentNumber { get; set; }       // Required, non-empty
}

public enum DocumentType
{
    Passport,
    NationalId
}
```

---

### 1.7 Reservation Response

```csharp
public class ReservationConfirmation
{
    public string ReferenceNumber { get; set; }      // Unique: e.g., "RES-20240621-001A2B"
    public DateTime ReservedAt { get; set; }
    
    public string HotelName { get; set; }
    public string Provider { get; set; }
    public RoomType RoomType { get; set; }
    
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NightsCount { get; set; }
    
    public decimal PricePerNight { get; set; }
    public decimal TotalPrice { get; set; }
    
    public CancellationPolicy CancellationPolicy { get; set; }
    
    public string GuestName { get; set; }
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; }       // Masked: "P123****" or "N987****"
}
```

---

### 1.8 Reservation (Internal Storage)

```csharp
public class Reservation
{
    public string ReferenceNumber { get; set; }
    public ReservationRequest Request { get; set; }
    public DateTime ReservedAt { get; set; }
    public decimal TotalPrice { get; set; }
    public CancellationPolicy CancellationPolicy { get; set; }
}
```

---

## 2. Provider Interface

### 2.1 IHotelProvider Interface

```csharp
public interface IHotelProvider
{
    string ProviderName { get; }  // "PremierStays" or "BudgetNests"
    
    /// <summary>
    /// Search for available rooms.
    /// </summary>
    /// <param name="destination">City name</param>
    /// <param name="checkInDate">Check-in date</param>
    /// <param name="checkOutDate">Check-out date</param>
    /// <param name="roomType">Optional: filter by room type</param>
    /// <returns>List of available rooms. May include availability=false (caller must filter).</returns>
    Task<List<HotelRoom>> SearchAsync(
        string destination,
        DateTime checkInDate,
        DateTime checkOutDate,
        RoomType? roomType = null
    );
}
```

---

### 2.2 PremierStays Provider

**Response Format:** PascalCase JSON

**Example Raw Response:**

```json
{
  "Results": [
    {
      "HotelId": "premier_001",
      "HotelName": "Grand Plaza Hotel",
      "RoomType": "Deluxe",
      "PricePerNight": 250,
      "Amenities": ["WiFi", "Pool", "Spa", "Gym"],
      "StarRating": 5,
      "CancellationPolicy": "FreeCancellation",
      "Available": true
    },
    {
      "HotelId": "premier_002",
      "HotelName": "City Center Inn",
      "RoomType": "Standard",
      "PricePerNight": 120,
      "Amenities": ["WiFi", "Breakfast"],
      "StarRating": 4,
      "CancellationPolicy": "NonRefundable",
      "Available": true
    }
  ]
}
```

**Characteristics:**
- **Full details:** Amenities, StarRating, detailed cancellation policy
- **Format:** PascalCase keys
- **Availability:** Always returns `"Available": true` (always available)
- **Cancellation:** "FreeCancellation" (48h), "NonRefundable"
- **Room types:** Standard, Deluxe, Suite

---

### 2.3 BudgetNests Provider

**Response Format:** snake_case JSON

**Example Raw Response:**

```json
{
  "results": [
    {
      "hotel_id": "budget_001",
      "hotel_name": "Budget Hostel & Rooms",
      "room_type": "Standard",
      "price_per_night": 60,
      "cancellation_policy": "Flexible",
      "available": true
    },
    {
      "hotel_id": "budget_002",
      "hotel_name": "Economy Stay",
      "room_type": "Deluxe",
      "price_per_night": 95,
      "cancellation_policy": "NonRefundable",
      "available": false
    }
  ]
}
```

**Characteristics:**
- **Minimal details:** No amenities or star rating
- **Format:** snake_case keys
- **Availability:** May return `"available": false` (caller must filter)
- **Cancellation:** "Flexible" (24h), "NonRefundable"
- **Room types:** Standard, Deluxe, Suite

---

## 3. Supported Destinations & Document Requirements

### 3.1 Document Rules

**International Destinations:**
- Require **Passport**
- Reject NationalId with 422 error

**Domestic Destinations:**
- Accept **NationalId** OR **Passport**

### 3.2 Destination Mapping

**Domestic Cities (accept NationalId or Passport):**
1. `"New Delhi"` (USA)
2. `"Hyderabad"` (USA)

**International Cities (require Passport):**
1. `"London"` (UK)
2. `"Paris"` (France)
3. `"Tokyo"` (Japan)

---

## 4. API Endpoints

### 4.1 Search Hotels

**Request:**
```
GET /hotels/search?destination={city}&checkIn={date}&checkOut={date}&roomType={type}
```

**Query Parameters:**
- `destination` (required): City name (case-insensitive)
- `checkIn` (required): Date in YYYY-MM-DD format
- `checkOut` (required): Date in YYYY-MM-DD format
- `roomType` (optional): Standard, Deluxe, or Suite

**Success Response (200):**
```json
{
  "results": [
    {
      "hotelId": "premier_001",
      "hotelName": "Grand Plaza Hotel",
      "provider": "PremierStays",
      "roomType": "Deluxe",
      "pricePerNight": 250,
      "totalNights": 3,
      "totalPrice": 750,
      "cancellationPolicy": {
        "type": "FreeCancellation",
        "hoursBeforeCheckIn": 48
      },
      "amenities": ["WiFi", "Pool", "Spa"],
      "starRating": 5
    }
  ],
  "totalCount": 1,
  "destination": "London",
  "checkInDate": "2024-06-25",
  "checkOutDate": "2024-06-28",
  "nightsCount": 3
}
```

**Error Responses:**

**400 - Missing Required Parameter:**
```json
{
  "error": "Missing required parameter: destination"
}
```

**400 - Invalid Date Format:**
```json
{
  "error": "Invalid date format. Use YYYY-MM-DD"
}
```

**400 - CheckOut Not After CheckIn:**
```json
{
  "error": "CheckOut date must be after CheckIn date"
}
```

**404 - Destination Not Found:**
```json
{
  "error": "Destination not recognized. Supported cities: New Delhi, Hyderabad, London, Paris, Tokyo"
}
```

---

### 4.2 Reserve Hotel

**Request:**
```
POST /hotels/reserve
Content-Type: application/json
```

**Body:**
```json
{
  "hotelId": "premier_001",
  "provider": "PremierStays",
  "roomType": "Deluxe",
  "checkInDate": "2024-06-25",
  "checkOutDate": "2024-06-28",
  "guestName": "John Doe",
  "documentType": "Passport",
  "documentNumber": "P12345678"
}
```

**Success Response (200):**
```json
{
  "referenceNumber": "RES-20240621-A1B2C3",
  "reservedAt": "2024-06-21T14:30:00Z",
  "hotelName": "Grand Plaza Hotel",
  "provider": "PremierStays",
  "roomType": "Deluxe",
  "checkInDate": "2024-06-25",
  "checkOutDate": "2024-06-28",
  "nightsCount": 3,
  "pricePerNight": 250,
  "totalPrice": 750,
  "cancellationPolicy": {
    "type": "FreeCancellation",
    "hoursBeforeCheckIn": 48
  },
  "guestName": "John Doe",
  "documentType": "Passport",
  "documentNumber": "P1234****"
}
```

**Error Responses:**

**422 - Document Mismatch (International destination, NationalId provided):**
```json
{
  "error": "London is an international destination. Passport is required. NationalId is not accepted."
}
```

**422 - Document Mismatch (Domestic destination, wrong document):**
```json
{
  "error": "New Delhi is a domestic destination. Passport or NationalId accepted."
}
```

**400 - Missing Required Field:**
```json
{
  "error": "Missing required field: documentNumber"
}
```

**404 - Hotel Not Found:**
```json
{
  "error": "Hotel not found in search results"
}
```

---

### 4.3 Get Reservation Details

**Request:**
```
GET /hotels/reservation/{referenceNumber}
```

**Path Parameters:**
- `referenceNumber` (required): Reservation reference (e.g., "RES-20240621-A1B2C3")

**Success Response (200):**
```json
{
  "referenceNumber": "RES-20240621-A1B2C3",
  "reservedAt": "2024-06-21T14:30:00Z",
  "hotelName": "Grand Plaza Hotel",
  "provider": "PremierStays",
  "roomType": "Deluxe",
  "checkInDate": "2024-06-25",
  "checkOutDate": "2024-06-28",
  "nightsCount": 3,
  "pricePerNight": 250,
  "totalPrice": 750,
  "cancellationPolicy": {
    "type": "FreeCancellation",
    "hoursBeforeCheckIn": 48
  },
  "guestName": "John Doe",
  "documentType": "Passport",
  "documentNumber": "P1234****"
}
```

**Error Responses:**

**404 - Reservation Not Found:**
```json
{
  "error": "Reservation not found"
}
```

---

## 5. Service Layer

### 5.1 IHotelSearchService

```csharp
public interface IHotelSearchService
{
    /// <summary>
    /// Search across all providers, normalize results, and return unified list.
    /// </summary>
    Task<HotelSearchResponse> SearchAsync(HotelSearchRequest request);
}
```

**Responsibilities:**
- Validate request (destination, dates)
- Query both providers in parallel
- Filter out unavailable rooms (BudgetNests)
- Normalize responses (PascalCase → unified model, snake_case → unified model)
- Calculate total nights and total price
- Sort results by total price (ascending, default)
- Return unified response

---

### 5.2 IReservationService

```csharp
public interface IReservationService
{
    /// <summary>
    /// Validate documents and create a reservation.
    /// </summary>
    Task<ReservationConfirmation> ReserveAsync(ReservationRequest request);
    
    /// <summary>
    /// Retrieve reservation by reference number.
    /// </summary>
    Task<ReservationConfirmation> GetReservationAsync(string referenceNumber);
}
```

**Responsibilities:**
- Validate document requirements per destination
- Generate unique reference number (format: RES-YYYYMMDD-XXXXXX)
- Store reservation in-memory
- Return confirmation with masked document number

---

### 5.3 IDocumentValidator

```csharp
public interface IDocumentValidator
{
    /// <summary>
    /// Validate document type against destination.
    /// </summary>
    /// <returns>Tuple of (IsValid, ErrorMessage)</returns>
    (bool IsValid, string ErrorMessage) ValidateDocument(
        string destination,
        DocumentType documentType
    );
}
```

**Logic:**
- If destination is international (London, Paris, Tokyo) → require Passport
- If destination is domestic (New Delhi, Hyderabad) → accept NationalId or Passport
- Return clear error message on mismatch

---

## 6. Dependency Injection & Architecture

### 6.1 Service Registration (Program.cs)

```csharp
builder.Services.AddScoped<IHotelProvider, PremierStaysProvider>();
builder.Services.AddScoped<IHotelProvider, BudgetNestsProvider>();
builder.Services.AddScoped<IHotelSearchService, HotelSearchService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IDocumentValidator, DocumentValidator>();

// In-memory storage
builder.Services.AddSingleton<IReservationStore, InMemoryReservationStore>();
```

### 6.2 In-Memory Storage

```csharp
public interface IReservationStore
{
    void Save(Reservation reservation);
    Reservation GetByReference(string referenceNumber);
    bool Exists(string referenceNumber);
}

public class InMemoryReservationStore : IReservationStore
{
    private readonly Dictionary<string, Reservation> _reservations = new();

    public void Save(Reservation reservation)
    {
        _reservations[reservation.ReferenceNumber] = reservation;
    }

    public Reservation GetByReference(string referenceNumber)
    {
        return _reservations.TryGetValue(referenceNumber, out var res) ? res : null;
    }

    public bool Exists(string referenceNumber)
    {
        return _reservations.ContainsKey(referenceNumber);
    }
}
```

---

## 7. Testing Strategy

### 7.1 Unit Tests

**PremierStaysProviderTests:**
- ✅ Search returns rooms with full details
- ✅ Always returns available=true
- ✅ Filters by room type when provided
- ✅ Returns empty list for unknown destination

**BudgetNestsProviderTests:**
- ✅ Search returns rooms with minimal details
- ✅ Filters out available=false rooms
- ✅ Filters by room type when provided
- ✅ Returns empty list for unknown destination

**DocumentValidatorTests:**
- ✅ Passport accepted for international destinations
- ✅ NationalId rejected for international destinations
- ✅ Passport accepted for domestic destinations
- ✅ NationalId accepted for domestic destinations
- ✅ Clear error messages

**HotelSearchServiceTests:**
- ✅ Normalizes PascalCase (PremierStays) to unified model
- ✅ Normalizes snake_case (BudgetNests) to unified model
- ✅ Merges results from both providers
- ✅ Filters by optional room type
- ✅ Calculates total price correctly
- ✅ Returns 400 if dates invalid
- ✅ Returns 404 if destination not recognized

**ReservationServiceTests:**
- ✅ Creates reservation with unique reference
- ✅ Rejects reservation on document mismatch (422)
- ✅ Masks document number in response
- ✅ Retrieves reservation by reference
- ✅ Returns 404 if reference not found

### 7.2 Integration Tests

**HotelEndpointsTests:**
- ✅ GET /hotels/search with all parameters
- ✅ GET /hotels/search with optional roomType
- ✅ GET /hotels/search returns 400 for missing destination
- ✅ GET /hotels/search returns 400 for invalid dates
- ✅ POST /hotels/reserve with valid data
- ✅ POST /hotels/reserve returns 422 for document mismatch
- ✅ GET /hotels/reservation/{ref} returns 200 for valid reference
- ✅ GET /hotels/reservation/{ref} returns 404 for invalid reference

---

## 8. Frontend Specification (Plain HTML/JS)

### 8.1 Pages

**index.html - Search & Results**
- Search form: destination, check-in, check-out, room type (optional)
- Results table: provider badge, room type, per-night rate, total price, cancellation policy
- Sortable by total price (ascending/descending)
- Loading state, error messages, empty state
- "Reserve" button for each room

**reservation.html - Reservation Form**
- Hidden form (shown via modal/overlay)
- Guest name, document type (dropdown), document number
- Validation (client-side)
- Submit button → calls POST /hotels/reserve
- Error display (422, 400, etc.)

**confirmation.html - Confirmation Page**
- Reference number (prominent)
- Hotel details, dates, total price, cancellation policy
- "Back to Search" button
- Printable layout

### 8.2 Client-Side Validation

- Destination: required, non-empty
- Check-in/Check-out: valid dates, check-out > check-in
- Guest name: required, non-empty
- Document number: required, non-empty
- Document type: required

### 8.3 API Client (JavaScript)

```javascript
async function searchHotels(destination, checkIn, checkOut, roomType = null)
async function reserveHotel(hotelId, provider, roomType, checkIn, checkOut, guestName, docType, docNumber)
async function getReservation(referenceNumber)
```

---

## 9. Assumptions & Constraints

1. **No Persistence:** Reservations stored in-memory only; lost on app restart
2. **No Real Providers:** Both providers are stubs returning fixed datasets
3. **No Authentication:** No user login or auth required
4. **No Payment Processing:** No real payment gateway
5. **No Email Confirmation:** No email notifications
6. **Offline:** Fully runs on a local machine; no external dependencies
7. **Deterministic Stubs:** Provider stubs return consistent data for testing
8. **Date Format:** All dates in YYYY-MM-DD format
9. **Timezone:** All timestamps in UTC

---

## 10. Success Criteria

- ✅ All API endpoints respond with correct status codes
- ✅ Search normalizes and merges provider responses correctly
- ✅ Reservation validates documents per destination rules
- ✅ Document mismatch returns 422 with clear error message
- ✅ Frontend reflects all states: search, results, empty, error, confirmation
- ✅ Unit tests cover core business logic (>80% critical path)
- ✅ Integration tests verify full search → reserve → confirmation flow
- ✅ Application runs from clean clone using only README instructions
- ✅ Code is clean, readable, and follows SOLID principles
- ✅ Spec.md committed before implementation files

---

## Appendix: Example Flows

### Flow 1: Domestic Search → International Mismatch
1. User searches London (international) → gets results
2. User tries to reserve with NationalId
3. API returns 422: "London is international. Passport required."
4. User selects Passport instead
5. Reservation succeeds

### Flow 2: Multiple Providers → Merged Results
1. Search "London" for 3 nights
2. PremierStays returns: Grand Plaza (Deluxe, $250/night) + City Center (Standard, $120/night)
3. BudgetNests returns: Budget Stay (Standard, $60/night) + Economy (Deluxe, $95/night) [but filters "available=false"]
4. Merged result: 3 rooms sorted by total price
5. User reserves from any provider

### Flow 3: Reference Retrieval
1. User completes reservation → gets reference "RES-20240621-A1B2C3"
2. User closes browser, later revisits app
3. User enters reference number in a "View Reservation" form
4. GET /hotels/reservation/RES-20240621-A1B2C3
5. System returns full confirmation details

---

**END OF SPECIFICATION**

*This document defines the contract. Implementation should not modify this specification; instead, propose changes and document them in prompts.md and reflection.md.*
