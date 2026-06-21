# Hotel Stay Availability System

A hotel availability aggregation platform that searches across multiple providers, normalizes results, and manages reservations with document validation.

## Tech Stack

- **Backend:** .NET 8 Minimal API (C#)
- **Frontend:** Plain HTML/CSS/JavaScript
- **Database:** In-Memory (no external dependencies)
- **Testing:** xUnit + Moq

## Project Structure

```
hotel-stay/
├── README.md                          # This file
├── spec.md                            # Complete specification
├── prompts.md                         # AI prompts used during development
├── reflection.md                      # What could be improved
│
├── hotel-stay.sln                     # Solution file
├── HotelStay.Api/                     # Backend API
│   ├── Program.cs
│   ├── Models/                        # Domain models
│   ├── Providers/                     # IHotelProvider implementations
│   ├── Services/                      # Business logic services
│   ├── Endpoints/                     # API endpoint handlers
│   └── HotelStay.Api.csproj
│
├── HotelStay.Tests/                   # Unit & integration tests
│   ├── Providers/
│   ├── Services/
│   ├── Endpoints/
│   └── HotelStay.Tests.csproj
│
└── hotel-stay-ui/                     # Frontend
    ├── index.html                     # Search & results page
    ├── reservation.html               # Reservation form
    ├── confirmation.html              # Confirmation page
    ├── css/
    │   └── style.css
    └── js/
        ├── app.js
        ├── api.js
        └── validator.js
```

## Prerequisites

- .NET 8 SDK or later ([download](https://dotnet.microsoft.com/download))
- A modern web browser (Chrome, Firefox, Safari, Edge)
- Git

## Setup & Run

### 1. Clone the Repository

```bash
git clone https://github.com/narayanatrinadh1999-bit/hotel-stay.git
cd hotel-stay
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Run the API

```bash
cd HotelStay.Api
dotnet run
```

The API will start at `http://localhost:5000`

### 4. Run the Frontend

Open a new terminal:

```bash
cd hotel-stay-ui
python -m http.server 8000
```

Or use any static server:
```bash
# Using Node.js http-server
npx http-server

# Using VS Code Live Server extension
Open index.html and click "Go Live"
```

Then open your browser to `http://localhost:8000` (or whatever port your server uses)

### 5. Run Tests

```bash
cd HotelStay.Tests
dotnet test
```

Or from the root:

```bash
dotnet test
```

## API Endpoints

### Search Hotels

```
GET /hotels/search?destination={city}&checkIn={YYYY-MM-DD}&checkOut={YYYY-MM-DD}&roomType={type}
```

**Parameters:**
- `destination` (required): City name (e.g., "London", "New Delhi")
- `checkIn` (required): Check-in date in YYYY-MM-DD format
- `checkOut` (required): Check-out date in YYYY-MM-DD format
- `roomType` (optional): "Standard", "Deluxe", or "Suite"

**Example:**
```
http://localhost:5000/hotels/search?destination=London&checkIn=2024-06-25&checkOut=2024-06-28
```

### Reserve Hotel

```
POST /hotels/reserve
Content-Type: application/json

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

### Get Reservation Details

```
GET /hotels/reservation/{referenceNumber}
```

**Example:**
```
http://localhost:5000/hotels/reservation/RES-20240621-A1B2C3
```

## Supported Destinations

### Domestic (Accept NationalId or Passport)
- New Delhi (India)
- Hyderabad (India)

### International (Require Passport)
- London (UK)
- Paris (France)
- Tokyo (Japan)

## Document Validation

- **International destinations:** Passport required (NationalId rejected)
- **Domestic destinations:** Passport or NationalId accepted

Mismatched documents return HTTP 422 with a clear error message.

## Supported Room Types

- Standard
- Deluxe
- Suite

## Features

✅ Search across two providers (PremierStays, BudgetNests)
✅ Normalize PascalCase (PremierStays) and snake_case (BudgetNests) responses
✅ Filter unavailable rooms (BudgetNests)
✅ Calculate per-night and total-stay pricing
✅ Document type validation based on destination
✅ Reservation confirmation with reference number
✅ Retrieve reservation by reference
✅ Client-side form validation
✅ Server-side input validation
✅ Error handling with meaningful messages
✅ Fully offline (no external APIs or credentials)

## Notes

- **In-Memory Storage:** Reservations are stored in memory and lost on app restart. This is intentional for the demo.
- **Stub Providers:** Both PremierStays and BudgetNests are stubs returning deterministic test data.
- **No Authentication:** No user login or authentication required for this demo.
- **No Persistence:** Data is not persisted to disk.

## Testing

Run the full test suite:

```bash
dotnet test --verbosity normal
```

Run tests with coverage:

```bash
dotnet test /p:CollectCoverage=true
```

## Demo Flow

1. **Search:** Enter destination (e.g., "London"), check-in/check-out dates, optional room type
2. **Results:** View merged results from both providers, sorted by total price
3. **Reserve:** Click "Reserve" on a room, fill guest name and document details
4. **Validate:** System validates document type against destination
5. **Confirm:** Receive confirmation with reference number
6. **Retrieve:** Use reference number to view reservation details

## Assumptions

- All dates are in YYYY-MM-DD format
- All timestamps are in UTC
- No user authentication required
- No real payment processing
- Application runs fully offline
- Provider data is deterministic (same results each run)

## Future Improvements

See `reflection.md` for detailed thoughts on what could be improved with more time.

## License

This is a demonstration project for recruitment purposes.
