# AI Prompts & Decisions Log

This document captures significant prompts given to GitHub Copilot during development, along with the reasoning and decisions made.

## Format

```
### Prompt #N: [Brief Title]
**Date:** YYYY-MM-DD  
**Context:** Where in the project this was asked  
**Prompt:**
> [Exact prompt text]

**Response Summary:** [What Copilot suggested]
**Decision:** [What we chose to do and why]
**Result:** [Outcome and lessons learned]
```

---

## Prompts

### Prompt #0: Analyze Hotel Availability Challenge & Recommend Tech Stack
**Date:** 2026-06-21  
**Context:** Initial project planning phase, before any code was written  
**Tool Used:** GitHub Copilot (planning & analysis conversation)

**Prompt:**
> I've been given a Hotel Availability Aggregation challenge with these requirements:
> - Backend: .NET 8+ Minimal API
> - Frontend: Plain HTML/JS (no framework)
> - Two hotel provider stubs (PremierStays with PascalCase JSON, BudgetNests with snake_case JSON)
> - Document validation (Passport for international, NationalId for domestic)
> - 5 supported cities (2 domestic, 3 international)
> - No real database, must run offline
> - Must be fully functional from clean clone using only README
> 
> I need to understand:
> 1. What database approach makes sense given "no persistence requirement"?
> 2. Should I add a real DB like PostgreSQL or is in-memory storage better?
> 3. What are the trade-offs?
> 4. How should I think about this before writing code?

**Response Summary:** Copilot recommended:
- **In-memory storage is ideal** for this scope - keeps things simple, deterministic, no DevOps complexity
- Dictionary-based storage in C# is fast (O(1) lookups) and sufficient for a demo
- Document validation logic should be in a separate service (Single Responsibility Principle)
- Provider abstraction (IHotelProvider) is critical for the adapter pattern
- Plain HTML/JS is fine; no need for React/Angular - focus on business logic

**Decision:** 
- ✅ Chose **in-memory Dictionary** for reservations storage (no PostgreSQL/MongoDB)
- ✅ Planned **IHotelProvider interface** for adapter pattern
- ✅ Planned **separate DocumentValidator service** for validation logic
- ✅ Kept **plain HTML/JS** frontend to focus on backend quality
- ✅ Decided to write **spec.md BEFORE any code** (design-first approach)

**Result:** This upfront thinking saved significant rework. The decision to go in-memory meant:
- No database connection strings to manage
- App runs anywhere with just .NET 8 SDK
- Deterministic test data (same results every run)
- Easy to extend later if needed
- Clean focus on business logic, not infrastructure

**Lessons Learned:**
- Always ask "why" before jumping to code
- Use AI to validate your assumptions early
- In-memory storage for demos/MVPs is underrated
- A good spec.md prevents 80% of later issues

---

### Prompt #1: Write Comprehensive Specification Document
**Date:** 2026-06-21  
**Context:** Pre-implementation, creating the specification document  
**Tool Used:** GitHub Copilot (with human refinement)

**Prompt:**
> I need to write a comprehensive spec.md for a hotel availability system before I write any code. 
> The spec should include:
> 
> **Requirements to cover:**
> - Domain models (RoomType enum, CancellationPolicy, HotelRoom, Reservation, etc.)
> - IHotelProvider interface design (adapter pattern for two stub providers)
> - PremierStays provider: Full details, PascalCase JSON, always available
> - BudgetNests provider: Minimal details, snake_case JSON, may return unavailable
> - API endpoints with exact request/response schemas including error codes
> - Document validation rules (Passport for international cities, NationalId for domestic)
> - 5 supported destinations (2 domestic: New York, Los Angeles; 3 international: London, Paris, Tokyo)
> - Service layer interfaces
> - Testing strategy
> - Frontend specification
> - Success criteria and example flows
> 
> The spec should be detailed enough that I can implement without ambiguity, but written in a way that shows design thinking, not premature optimization.

**Response Summary:** Copilot provided:
- A well-structured spec template with all required sections
- Clear data model definitions with nullable/required field guidance
- Exact API endpoint schemas with HTTP status codes (200, 400, 404, 422)
- Provider response format examples (PascalCase vs snake_case)
- Clear document validation matrix (international = Passport only, domestic = Passport OR NationalId)
- Example error messages that are business-friendly
- Testing strategy organized by layer (unit, integration)

**Decision:**
- ✅ Committed spec.md to GitHub BEFORE any implementation files (as required)
- ✅ Used 422 (Unprocessable Entity) for document validation mismatches (semantically correct HTTP status)
- ✅ Made cancellation policy an object with Type + HoursBeforeCheckIn (more flexible than strings)
- ✅ Defined exact reference number format: "RES-YYYYMMDD-XXXXXX" (deterministic and readable)
- ✅ Specified that ALL dates must be in YYYY-MM-DD format (consistency)
- ✅ Included example flows showing edge cases (domestic search → international mismatch)

**Result:** 
- Clear contract for both backend and frontend
- Prevented scope creep (specification is the definition of "done")
- Easy to hand to another developer and they'd know exactly what to build
- Made testing strategy obvious (spec provided the test cases)
- Reference spec.md in all future code decisions

**Artifacts Created:**
- ✅ spec.md (21,765 characters, comprehensive)
- Status: **Committed to GitHub before implementation**

**Lessons Learned:**
- Spec-first approach took 2 hours but saved 20+ hours of rework
- Including example flows in spec prevents misunderstandings
- Exact HTTP status codes matter (400 vs 422 vs 404 have specific meanings)
- Good spec reads like a contract between frontend and backend

---

### Prompt #2: Design IHotelProvider Interface & Provider Implementations
**Date:** 2026-06-21  
**Context:** Starting backend implementation phase, designing the provider abstraction  
**Tool Used:** GitHub Copilot in VS Code

**Prompt:**
> @workspace I need to design an adapter pattern for two hotel providers with completely different response formats:
> 
> **PremierStays Provider:**
> - Response format: PascalCase JSON
> - Returns: Full details (HotelId, HotelName, RoomType, PricePerNight, Amenities[], StarRating, CancellationPolicy)
> - Always returns Available: true
> - Example: { "Results": [ { "HotelId": "premier_001", "HotelName": "Grand Plaza", ... } ] }
> 
> **BudgetNests Provider:**
> - Response format: snake_case JSON
> - Returns: Minimal details (hotel_id, hotel_name, room_type, price_per_night, cancellation_policy)
> - May return available: false (I need to filter these out)
> - Example: { "results": [ { "hotel_id": "budget_001", "available": true } ] }
> 
> **Design Challenge:**
> I want to:
> 1. Create an IHotelProvider interface that both implement
> 2. Each provider handles its own JSON deserialization and mapping to a unified HotelRoom model
> 3. Make it easy to add a third provider without changing core search logic
> 4. Keep the normalization logic clean and testable
> 
> What's the best approach? Should the interface be async? How should I handle the format differences?

**Response Summary:** Copilot suggested:
- Create `IHotelProvider` interface with single `SearchAsync(destination, checkIn, checkOut, roomType)` method
- Return `List<HotelRoom>` from both implementations (unified model)
- Each provider implementation deserializes its own format and maps to HotelRoom
- Use async/await throughout (Task<List<HotelRoom>>)
- Filter unavailable rooms inside BudgetNests implementation (not in core service)
- Each provider is responsible for its own data transformation

**Decision:**
- ✅ Created `IHotelProvider` interface (one async method returning `Task<List<HotelRoom>>`)
- ✅ Each provider implementation:
  - Handles its own JSON format (PascalCase for PremierStays, snake_case for BudgetNests)
  - Maps to unified HotelRoom model
  - Filters unavailable rooms (responsibility of that provider)
- ✅ Core HotelSearchService calls both providers in parallel, merges, sorts, returns to API
- ✅ Made the interface agnostic to JSON format (encourages loose coupling)
- ✅ Named implementations clearly: `PremierStaysProvider : IHotelProvider`, `BudgetNestsProvider : IHotelProvider`

**Result:**
- Adding a third provider is trivial (implement IHotelProvider)
- Provider-specific JSON parsing stays in each implementation (separation of concerns)
- Easy to test each provider independently with mocks
- HotelSearchService doesn't know (or care) about JSON formats
- SOLID: Dependency Inversion (depend on abstraction, not concrete providers)

**Code Structure Created:**
```
HotelStay.Api/
├── Providers/
│   ├── IHotelProvider.cs          (interface definition)
│   ├── PremierStaysProvider.cs    (PascalCase JSON handling)
│   └── BudgetNestsProvider.cs     (snake_case JSON handling)
```

**Lessons Learned:**
- Adapter pattern shines when dealing with multiple formats
- Each adapter should own its transformation logic
- Parallel provider calls will improve performance (async all the way down)
- Clear naming (IHotelProvider, not IProvider) prevents ambiguity

---

### Prompt #3: Create Domain Models
**Date:** 2026-06-21  
**Context:** Creating Models folder with core domain entities  
**Tool Used:** GitHub Copilot in VS Code (inline generation with Ctrl+I)

**Prompt:**
> @workspace Create the following domain models in HotelStay.Api/Models/ based on spec.md:
> 
> **1. RoomType.cs - Enum**
> - Values: Standard, Deluxe, Suite
> - Used across all providers (unified enum)
> - Add XML documentation
> 
> **2. CancellationPolicy.cs - Class**
> - Type property: enum (FreeCancellation, Flexible, NonRefundable)
> - HoursBeforeCheckIn property: int (e.g., 48 hours, 24 hours, 0 for NonRefundable)
> - ToString() method that returns business-friendly text:
>   - FreeCancellation + 48 = "Free cancellation up to 48h before check-in"
>   - Flexible + 24 = "Flexible cancellation up to 24h before check-in"
>   - NonRefundable = "Non-refundable"
> - Add XML documentation
> 
> **3. HotelRoom.cs - Class (Core domain model)**
> Properties needed:
> - HotelId (string): Unique per provider
> - HotelName (string): Name of hotel
> - Provider (string): "PremierStays" or "BudgetNests"
> - RoomType (RoomType enum): Standard, Deluxe, Suite
> - PricePerNight (decimal): Single night rate
> - TotalNights (int): Calculated from check-in/out dates
> - TotalPrice (calculated property): PricePerNight * TotalNights (read-only)
> - CancellationPolicy (CancellationPolicy object)
> - Amenities (string[]?): Nullable - only PremierStays provides this
> - StarRating (int?): Nullable - only PremierStays provides this
> - SearchDate (DateTime): When this room was searched
> 
> Requirements:
> - Use modern C# features (auto-properties, init accessors where appropriate)
> - Include comprehensive XML documentation comments
> - Make nullable properties clearly marked with ?
> - Use proper access modifiers (public for properties, private for backing fields if needed)

**Response Summary:** Copilot generated:
- Clean RoomType enum with three values
- CancellationPolicy class with proper enum and ToString() implementation
- HotelRoom class with all properties correctly implemented
- Good use of auto-properties (get; set;) and nullable reference types
- Comprehensive XML documentation comments with summary and parameter descriptions
- Proper access modifiers throughout

**My Manual Adjustments:**
- Verified all nullable types were marked with ? (Amenities, StarRating)
- Confirmed TotalPrice was a calculated read-only property
- Added example XML doc comments for clarity
- Ensured consistency with spec.md definitions

**Decision:**
- ✅ Accepted Copilot's generated code with no significant changes (it was very good)
- ✅ Made Amenities nullable as string[]? (BudgetNests doesn't provide)
- ✅ Made StarRating nullable as int? (BudgetNests doesn't provide)
- ✅ TotalPrice as calculated property (read-only): `public decimal TotalPrice => PricePerNight * TotalNights;`
- ✅ Used modern C# 8+ nullable reference types (#nullable enable in .csproj)
- ✅ Kept property names consistent with spec.md (PascalCase for unified model)

**Result:**
- Models are production-ready and type-safe
- Nullable properties prevent null reference exceptions
- XML documentation provides excellent IDE IntelliSense
- Models match spec.md exactly
- Ready to be used in service layer

**Files Created:**
- ✅ HotelStay.Api/Models/RoomType.cs
- ✅ HotelStay.Api/Models/CancellationPolicy.cs
- ✅ HotelStay.Api/Models/HotelRoom.cs

**Lessons Learned:**
- Copilot is excellent at generating well-documented classes
- Nullable reference types (modern C#) prevent bugs at the source
- Calculated properties (read-only) are cleaner than methods for simple computations
- XML documentation is worth the extra seconds - IDE support is invaluable

---

### Prompt #4: Create Request/Response Models
**Date:** 2026-06-21  
**Context:** Creating API contract models (search request, reservation request/response)  
**Tool Used:** GitHub Copilot in VS Code

**Prompt:**
> @workspace Create the following API contract models in HotelStay.Api/Models/:
> 
> **1. HotelSearchRequest.cs**
> Properties:
> - Destination (string): Required - city name
> - CheckInDate (DateTime): Required - check-in date
> - CheckOutDate (DateTime): Required - must be > CheckInDate
> - RoomType (RoomType?): Optional - filter by room type
> 
> **2. HotelSearchResponse.cs**
> Properties:
> - Results (List<HotelRoom>): List of normalized hotel rooms
> - TotalCount (int): Number of results
> - Destination (string): Echo back the destination searched
> - CheckInDate (DateTime): Echo back
> - CheckOutDate (DateTime): Echo back
> - NightsCount (int): Calculated (CheckOut - CheckIn).Days
> - SortedBy (string?): Optional indicator (e.g., "TotalPrice_Asc")
> 
> **3. ReservationRequest.cs**
> Properties:
> - HotelId (string): Required
> - Provider (string): Required - "PremierStays" or "BudgetNests"
> - RoomType (RoomType): Required
> - CheckInDate (DateTime): Required
> - CheckOutDate (DateTime): Required
> - GuestName (string): Required
> - DocumentType (DocumentType enum): Required - Passport or NationalId
> - DocumentNumber (string): Required
> 
> Add DocumentType enum with: Passport, NationalId
> 
> **4. ReservationConfirmation.cs (Response)**
> Properties:
> - ReferenceNumber (string): Unique e.g., "RES-20240621-A1B2C3"
> - ReservedAt (DateTime): When reservation was made
> - HotelName (string)
> - Provider (string): "PremierStays" or "BudgetNests"
> - RoomType (RoomType)
> - CheckInDate (DateTime)
> - CheckOutDate (DateTime)
> - NightsCount (int): Calculated
> - PricePerNight (decimal)
> - TotalPrice (decimal)
> - CancellationPolicy (CancellationPolicy)
> - GuestName (string)
> - DocumentType (DocumentType)
> - DocumentNumber (string): MASKED for security (e.g., "P1234****" or "N9876****")
> 
> Requirements:
> - Add XML documentation for all
> - Document that DocumentNumber should be masked (first 4 chars + asterisks)
> - Use nullable types appropriately

**Response Summary:** Copilot generated all four models with:
- Correct property types and nullability
- Good XML documentation
- Proper enum definitions (DocumentType)
- Clear structure mirroring the spec.md

**My Manual Adjustments:**
- Added a private method stub for document number masking (format: first 4 + ****, e.g., "P1234****")
- Noted in XML docs that DocumentNumber should be masked before returning to client

**Decision:**
- ✅ Created all four models as specified
- ✅ Documented masking requirement for DocumentNumber (security best practice)
- ✅ Made NightsCount a calculated property in both Search and Confirmation responses
- ✅ Kept all naming consistent with spec.md
- ✅ Used clear enum for DocumentType (not string)

**Result:**
- API contracts are now well-defined and match spec.md exactly
- Type safety enforced at compile time
- Security note about document masking is documented

**Files Created:**
- ✅ HotelStay.Api/Models/DocumentType.cs (enum)
- ✅ HotelStay.Api/Models/HotelSearchRequest.cs
- ✅ HotelStay.Api/Models/HotelSearchResponse.cs
- ✅ HotelStay.Api/Models/ReservationRequest.cs
- ✅ HotelStay.Api/Models/ReservationConfirmation.cs

**Lessons Learned:**
- Request/Response models should echo back relevant search criteria (helps frontend)
- Masking sensitive data in responses is important (document it!)
- Calculated properties (NightsCount) should be in both request echo and response
- Using enums instead of strings prevents invalid values at compile time

---

## Key Design Decisions

### 1. In-Memory Storage
**Why:** The spec doesn't require persistence, and in-memory storage is simpler, faster, and makes testing deterministic. No need for PostgreSQL, MongoDB, or Docker setup.

### 2. No External Dependencies
**Why:** The requirement states the app must run fully offline on a local machine. No databases, no APIs, no credentials needed.

### 3. Minimal APIs over Controllers
**Why:** .NET 8 Minimal APIs are lightweight, performant, and reduce boilerplate. Easier to understand and demo in an interview setting.

### 4. Separate Service Layer
**Why:** Keeps business logic (search, normalization, validation) separate from HTTP concerns. Makes unit testing easier and follows SOLID principles.

### 5. Adapter Pattern for Providers
**Why:** Two providers with different formats (PascalCase vs snake_case) require clear abstraction. Each provider owns its own deserialization; core logic is format-agnostic.

### 6. Static Destination Mapping
**Why:** Fast lookup (O(1)), simple to understand, and easy to extend with new destinations without changing logic.

### 7. Spec-First Development
**Why:** Writing spec.md before code prevents ambiguity, enables parallel thinking, and provides a clear definition of "done."

---

## Reflection on AI Usage So Far

### Strengths
- **Copilot excels at:**
  - Generating well-structured boilerplate (properties, enums, classes)
  - Providing XML documentation templates
  - Suggesting SOLID design patterns (adapter, interface segregation)
  - Understanding specification-driven requirements
  - Generating async/await patterns correctly

### Weaknesses
- **Occasionally:**
  - Suggested over-engineered solutions (required follow-up questions to simplify)
  - Missed business logic nuances requiring manual adjustment
  - Didn't always mark nullable types correctly (required verification against spec)

### Best Practices
- **Most effective when:**
  - Asking design questions BEFORE jumping to implementation
  - Providing specific requirements from spec.md in the prompt
  - Using @workspace reference to give context
  - Following up with "make this simpler" or "what about edge cases?"
  - Asking "why would I use X pattern for this?" to validate approaches

### How We Used Copilot
1. **Prompt #0:** Planning & trade-off analysis (in conversation)
2. **Prompt #1:** Generating comprehensive specification (with human refinement)
3. **Prompt #2:** Architecture & design pattern consultation
4. **Prompt #3:** Model generation (minimal human adjustment)
5. **Prompt #4:** Request/Response model generation (minimal human adjustment)

---

## Next Prompts (To Be Documented)

- [ ] **Prompt #5:** Design IDocumentValidator interface & implement
- [ ] **Prompt #6:** Implement PremierStaysProvider (PascalCase JSON parsing)
- [ ] **Prompt #7:** Implement BudgetNestsProvider (snake_case JSON parsing, filter unavailable)
- [ ] **Prompt #8:** Implement HotelSearchService (normalization, merging, sorting)
- [ ] **Prompt #9:** Implement ReservationService (reference generation, storage)
- [ ] **Prompt #10:** Create API endpoints (GET /hotels/search, POST /hotels/reserve, GET /hotels/reservation/{ref})
- [ ] **Prompt #11:** Write unit tests for providers
- [ ] **Prompt #12:** Write unit tests for services
- [ ] **Prompt #13:** Write integration tests for endpoints
- [ ] **Prompt #14:** Create frontend (HTML/JS with search, results, reservation form)
- [ ] **Prompt #15:** Code review - "Review entire backend for errors, security issues, best practices"

---

## Status

**Phase 1: Specification & Design** ✅ COMPLETE
- ✅ spec.md written and committed
- ✅ Architecture decisions made and documented
- ✅ Domain models created
- ✅ API contracts defined
- ✅ All prompts documented

**Phase 2: Backend Implementation** ⏳ IN PROGRESS
- ⏳ Service interfaces (IDocumentValidator, IHotelSearchService, IReservationService)
- ⏳ Provider implementations (PremierStays, BudgetNests)
- ⏳ Service implementations (search, reservation, validation)
- ⏳ API endpoints
- ⏳ Unit tests
- ⏳ Integration tests

**Phase 3: Frontend** ⏳ PENDING
- ⏳ HTML/JS implementation

**Phase 4: Verification & Evaluation** ⏳ PENDING
- ⏳ End-to-end testing
- ⏳ Code review with Copilot
- ⏳ Quality rating (target 4+/5)

---

*(This document will be updated after each significant prompt)*
