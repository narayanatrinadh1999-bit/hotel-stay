# Prompts for GitHub Copilot - Hotel Stay Testing & Completion

## Prompt #1: Create Unit Tests for PremierStaysProvider

**Context:** Backend testing phase. Need comprehensive unit tests for PremierStaysProvider.

**Prompt:**
> I need to create comprehensive unit tests for the PremierStaysProvider class in the Hotel Stay application.
> 
> **Project Setup:**
> - Test Framework: xUnit
> - Mocking: Moq
> - Test File Location: `HotelStay.Tests/Providers/PremierStaysProviderTests.cs`
> - Class Under Test: `HotelStay.Api.Providers.PremierStaysProvider`
> 
> **Provider Details:**
> The PremierStaysProvider is a stub that always returns available=true rooms with full details (amenities, star rating).
> It returns these rooms deterministically for testing:
> - premier_001: Grand Plaza Hotel, Deluxe, $250/night, 5 stars, FreeCancellation (48h)
> - premier_002: City Center Inn, Standard, $120/night, 4 stars, NonRefundable
> - premier_003: Seaside Suites, Suite, $400/night, 5 stars, FreeCancellation (48h)
> 
> **Test Cases Required:**
> 1. SearchAsync returns all 3 rooms when destination is valid
> 2. SearchAsync returns rooms with correct provider name "PremierStays"
> 3. SearchAsync always returns available=true (no filtering needed)
> 4. SearchAsync filters by RoomType when roomType parameter is provided
>    - Example: Search with roomType=Deluxe should return only premier_001 and premier_003
> 5. SearchAsync returns empty list for unknown destination
> 6. SearchAsync returns correct amenities and star ratings
> 7. SearchAsync includes correct cancellation policy details
> 8. SearchAsync returns List<HotelRoom> (not null)
> 9. SearchAsync is async (uses await properly)
> 10. SearchAsync with roomType=null returns all rooms (no filter applied)
> 
> **Test Structure:**
> - Use xUnit [Fact] and [Theory] attributes appropriately
> - Use Assert.NotNull, Assert.Equal, Assert.Contains, Assert.Empty as needed
> - Each test should be isolated and independent
> - Use descriptive test method names following pattern: MethodName_Scenario_ExpectedResult
> - No mocking required (PremierStaysProvider has no dependencies)
> 
> **Expected Test File Content:**
> ```csharp
> using Xunit;
> using HotelStay.Api.Providers;
> using HotelStay.Api.Models;
> 
> namespace HotelStay.Tests.Providers
> {
>     public class PremierStaysProviderTests
>     {
>         private readonly PremierStaysProvider _provider = new PremierStaysProvider();
>         
>         // Test methods here
>     }
> }
> ```
> 
> **Generate the complete test file with all 10 test cases.**

---

## Prompt #2: Create Unit Tests for BudgetNestsProvider

**Context:** Backend testing phase. Need comprehensive unit tests for BudgetNestsProvider.

**Prompt:**
> I need to create comprehensive unit tests for the BudgetNestsProvider class.
> 
> **Test File Location:** `HotelStay.Tests/Providers/BudgetNestsProviderTests.cs`
> **Class Under Test:** `HotelStay.Api.Providers.BudgetNestsProvider`
> 
> **Provider Details:**
> The BudgetNestsProvider is a stub that returns minimal details (no amenities, no star rating) and some rooms marked as unavailable (available=false).
> It returns these rooms deterministically:
> - budget_001: Budget Hostel & Rooms, Standard, $60/night, available=true, Flexible (24h)
> - budget_002: Economy Stay, Deluxe, $95/night, available=true, NonRefundable
> - budget_003: Economy Suite, Suite, $150/night, available=true, Flexible (24h)
> 
> **Test Cases Required:**
> 1. SearchAsync returns all 3 rooms (including unavailable ones - filtering is caller responsibility)
> 2. SearchAsync returns rooms with provider name "BudgetNests"
> 3. SearchAsync returns minimal details (amenities=null, starRating=0)
> 4. SearchAsync may return available=false rooms (caller must filter)
> 5. SearchAsync filters by RoomType when provided
>    - Example: roomType=Suite should return only budget_003
> 6. SearchAsync returns empty list for unknown destination
> 7. SearchAsync includes correct cancellation policy (Flexible, NonRefundable)
> 8. SearchAsync with roomType=null returns all rooms
> 9. SearchAsync is async
> 10. All returned rooms have SearchDate populated
> 
> **Test Structure:**
> - Use xUnit [Fact] and [Theory] attributes
> - Test method naming: MethodName_Scenario_ExpectedResult
> - Each test isolated and independent
> - No mocking required
> 
> **Generate the complete test file with all 10 test cases.**

---

## Prompt #3: Create Unit Tests for DocumentValidator

**Context:** Backend testing phase. Document validation is critical for reservations.

**Prompt:**
> I need to create comprehensive unit tests for the DocumentValidator class.
> 
> **Test File Location:** `HotelStay.Tests/Services/DocumentValidatorTests.cs`
> **Class Under Test:** `HotelStay.Api.Services.DocumentValidator`
> 
> **Document Validation Rules:**
> - International destinations (London, Paris, Tokyo) require Passport only
> - Domestic destinations (New York, Los Angeles, New Delhi, Hyderabad) accept NationalId OR Passport
> - Invalid destinations should be handled appropriately
> 
> **Test Cases Required:**
> 1. ValidateDocument_InternationalDestination_Passport_ReturnsValid
>    - Destination: "London", DocumentType: Passport
>    - Expected: (IsValid=true, ErrorMessage=null)
> 
> 2. ValidateDocument_InternationalDestination_NationalId_ReturnsInvalid
>    - Destination: "London", DocumentType: NationalId
>    - Expected: (IsValid=false, ErrorMessage includes "Passport required")
> 
> 3. ValidateDocument_DomesticDestination_NationalId_ReturnsValid
>    - Destination: "New York", DocumentType: NationalId
>    - Expected: (IsValid=true, ErrorMessage=null)
> 
> 4. ValidateDocument_DomesticDestination_Passport_ReturnsValid
>    - Destination: "New York", DocumentType: Passport
>    - Expected: (IsValid=true, ErrorMessage=null)
> 
> 5. ValidateDocument_AllInternationalDestinations_Passport_ReturnsValid
>    - Test London, Paris, Tokyo with Passport
>    - Use [Theory] with InlineData for each
>    - Expected: all should be valid
> 
> 6. ValidateDocument_AllDomesticDestinations_NationalId_ReturnsValid
>    - Test New York, Los Angeles, New Delhi, Hyderabad with NationalId
>    - Use [Theory] with InlineData for each
>    - Expected: all should be valid
> 
> 7. ValidateDocument_ErrorMessage_IsClear
>    - Test invalid combination
>    - Expected: ErrorMessage includes destination name and requirement
> 
> 8. ValidateDocument_CaseInsensitive_Destination
>    - Test "london" (lowercase), "LONDON" (uppercase), "LoNdOn" (mixed)
>    - Expected: all treated as London, validation works consistently
> 
> **Test Structure:**
> - Use xUnit [Fact] and [Theory] attributes
> - Use Assert.True, Assert.False, Assert.Contains for error messages
> - Document validator needs to be instantiated (no dependencies)
> 
> **Generate the complete test file with all 8 test cases.**

---

## Prompt #4: Create Unit Tests for HotelSearchService

**Context:** Backend testing phase. HotelSearchService normalizes provider responses.

**Prompt:**
> I need to create comprehensive unit tests for the HotelSearchService class.
> 
> **Test File Location:** `HotelStay.Tests/Services/HotelSearchServiceTests.cs`
> **Class Under Test:** `HotelStay.Api.Services.HotelSearchService`
> 
> **Dependencies (Mock These):**
> - IEnumerable<IHotelProvider> - inject PremierStaysProvider and BudgetNestsProvider mocks
> - Or use real providers for integration testing
> 
> **Service Responsibilities:**
> - Query both providers in parallel
> - Filter out unavailable rooms (available=false)
> - Normalize responses to unified HotelRoom model
> - Calculate total nights and total price
> - Return sorted by total price (ascending by default)
> - Validate request (destination, dates)
> - Throw ArgumentException for invalid dates
> - Throw KeyNotFoundException for unknown destination
> 
> **Test Cases Required:**
> 1. SearchAsync_ValidRequest_ReturnsHotelSearchResponse
>    - Input: destination="London", checkIn="2026-06-23", checkOut="2026-06-27"
>    - Expected: response.Results.Count > 0, response.TotalCount > 0
> 
> 2. SearchAsync_ValidRequest_CalculatesTotalNights
>    - Input: checkIn="2026-06-23", checkOut="2026-06-27"
>    - Expected: response.NightsCount = 4
> 
> 3. SearchAsync_ValidRequest_CalculatesTotalPrice
>    - For a room with pricePerNight=$100 and 4 nights
>    - Expected: totalPrice = $400
> 
> 4. SearchAsync_WithRoomTypeFilter_ReturnsOnlyMatchingRoomType
>    - Input: destination="London", roomType=RoomType.Deluxe
>    - Expected: all results have roomType=Deluxe
> 
> 5. SearchAsync_MergesResultsFromBothProviders
>    - Input: destination="London"
>    - Expected: results include hotels from both PremierStays AND BudgetNests
> 
> 6. SearchAsync_FiltersUnavailableRooms
>    - BudgetNests has available=false rooms
>    - Expected: unavailable rooms are filtered out
> 
> 7. SearchAsync_SortsByTotalPrice_Ascending
>    - Expected: results ordered by totalPrice (lowest to highest)
> 
> 8. SearchAsync_InvalidDestination_ThrowsKeyNotFoundException
>    - Input: destination="InvalidCity"
>    - Expected: throws KeyNotFoundException
> 
> 9. SearchAsync_CheckOutBeforeCheckIn_ThrowsArgumentException
>    - Input: checkIn="2026-06-27", checkOut="2026-06-23"
>    - Expected: throws ArgumentException
> 
> 10. SearchAsync_InvalidDateFormat_ThrowsArgumentException
>     - Input: dates that cannot be parsed
>     - Expected: throws ArgumentException
> 
> 11. SearchAsync_NormalizesPascalCaseToUnified_PremierStays
>     - PremierStays returns PascalCase
>     - Expected: response uses camelCase JSON (normalized)
> 
> 12. SearchAsync_ReturnsResponseWithCorrectMetadata
>     - Expected: response includes destination, checkInDate, checkOutDate, NightsCount, SortedBy
> 
> **Test Structure:**
> - Use xUnit [Fact] and [Theory]
> - Use real providers or mock them with Moq
> - Use Assert.NotNull, Assert.NotEmpty, Assert.Throws, Assert.All
> 
> **Generate the complete test file with all 12 test cases.**

---

## Prompt #5: Create Unit Tests for ReservationService

**Context:** Backend testing phase. ReservationService handles document validation and storage.

**Prompt:**
> I need to create comprehensive unit tests for the ReservationService class.
> 
> **Test File Location:** `HotelStay.Tests/Services/ReservationServiceTests.cs`
> **Class Under Test:** `HotelStay.Api.Services.ReservationService`
> 
> **Dependencies (Mock These):**
> - IDocumentValidator documentValidator - mock this
> - IReservationStore store - mock this
> 
> **Service Responsibilities:**
> - Validate document type against destination (using IDocumentValidator)
> - Throw InvalidOperationException if document validation fails (422 error)
> - Generate unique reference number in format "RES-YYYYMMDD-XXXXXX"
> - Store reservation in IReservationStore
> - Return ReservationConfirmation with masked document number
> - Retrieve reservation by reference number
> - Throw KeyNotFoundException if reference not found
> 
> **Test Cases Required:**
> 1. ReserveAsync_ValidRequest_ReturnsConfirmation
>    - Input: valid ReservationRequest with all required fields
>    - Expected: returns ReservationConfirmation with referenceNumber
> 
> 2. ReserveAsync_GeneratesUniqueReferenceNumber
>    - Input: valid request
>    - Expected: referenceNumber matches pattern "RES-YYYYMMDD-XXXXXX"
> 
> 3. ReserveAsync_MasksDocumentNumber
>    - Input: documentNumber="P12345678"
>    - Expected: confirmation.documentNumber="P1234****"
> 
> 4. ReserveAsync_DocumentValidationFails_ThrowsInvalidOperationException
>    - Setup: Mock documentValidator to return (IsValid=false, ErrorMessage="...")
>    - Expected: throws InvalidOperationException with error message
> 
> 5. ReserveAsync_InternationalDestination_NationalId_ThrowsInvalidOperation
>    - Input: destination="London", documentType=NationalId
>    - Expected: throws InvalidOperationException with "Passport required" message
> 
> 6. ReserveAsync_DomesticDestination_NationalId_Succeeds
>    - Input: destination="New York", documentType=NationalId
>    - Expected: reservation succeeds, returns confirmation
> 
> 7. ReserveAsync_StoresReservationInStore
>    - Input: valid request
>    - Expected: store.Save is called with the reservation
> 
> 8. ReserveAsync_CalculatesTotalPrice
>    - Input: pricePerNight=$100, 4 nights
>    - Expected: confirmation.totalPrice=$400
> 
> 9. ReserveAsync_IncludesHotelDetails
>    - Expected: confirmation includes hotelName, provider, roomType, checkInDate, checkOutDate
> 
> 10. GetReservationAsync_ValidReference_ReturnsConfirmation
>     - Setup: Store has a reservation with reference "RES-20240621-ABC123"
>     - Expected: returns confirmation with matching reference
> 
> 11. GetReservationAsync_InvalidReference_ThrowsKeyNotFoundException
>     - Input: reference="RES-00000000-INVALID"
>     - Expected: throws KeyNotFoundException
> 
> 12. ReserveAsync_DifferentDocumentTypes_ReturnsCorrectType
>     - Test with Passport and NationalId separately
>     - Expected: confirmation.documentType matches input
> 
> **Test Structure:**
> - Use xUnit [Fact] and [Theory]
> - Use Moq to mock IDocumentValidator and IReservationStore
> - Use Assert.Throws<InvalidOperationException>, Assert.Throws<KeyNotFoundException>
> - Use Assert.Contains for pattern matching
> 
> **Generate the complete test file with all 12 test cases.**

---

## Prompt #6: Create Integration Tests for API Endpoints

**Context:** Integration testing phase. Test actual HTTP endpoints.

**Prompt:**
> I need to create comprehensive integration tests for the Hotel Stay API endpoints.
> 
> **Test File Location:** `HotelStay.Tests/Endpoints/HotelEndpointsTests.cs`
> **Testing Approach:** Use WebApplicationFactory to run the full application
> 
> **Setup Required:**
> ```csharp
> public class HotelEndpointsTests : IAsyncLifetime
> {
>     private WebApplicationFactory<Program> _factory;
>     private HttpClient _client;
>     
>     public async Task InitializeAsync()
>     {
>         _factory = new WebApplicationFactory<Program>();
>         _client = _factory.CreateClient();
>     }
>     
>     public async Task DisposeAsync()
>     {
>         _client?.Dispose();
>         _factory?.Dispose();
>     }
> }
> ```
> 
> **Test Cases Required:**
> 
> ### Search Endpoint Tests (GET /hotels/search)
> 
> 1. SearchHotels_ValidRequest_Returns200
>    - Request: /hotels/search?destination=London&checkIn=2026-06-23&checkOut=2026-06-27
>    - Expected: status 200, response contains results array
> 
> 2. SearchHotels_ValidRequest_ReturnsCorrectStructure
>    - Expected: response has results, totalCount, destination, checkInDate, checkOutDate, nightsCount
> 
> 3. SearchHotels_WithOptionalRoomType_Returns200
>    - Request: /hotels/search?destination=London&checkIn=2026-06-23&checkOut=2026-06-27&roomType=Deluxe
>    - Expected: status 200, all results have roomType=Deluxe
> 
> 4. SearchHotels_MissingDestination_Returns400
>    - Request: /hotels/search?checkIn=2026-06-23&checkOut=2026-06-27
>    - Expected: status 400, error message includes "destination"
> 
> 5. SearchHotels_MissingCheckIn_Returns400
>    - Request: /hotels/search?destination=London&checkOut=2026-06-27
>    - Expected: status 400, error message includes "checkIn"
> 
> 6. SearchHotels_MissingCheckOut_Returns400
>    - Expected: status 400, error message includes "checkOut"
> 
> 7. SearchHotels_InvalidCheckInFormat_Returns400
>    - Request: checkIn=2026-13-01 (invalid month)
>    - Expected: status 400, error message about date format
> 
> 8. SearchHotels_CheckOutBeforeCheckIn_Returns400
>    - Request: checkIn=2026-06-27&checkOut=2026-06-23
>    - Expected: status 400, error message about dates
> 
> 9. SearchHotels_InvalidDestination_Returns404
>    - Request: destination=InvalidCity
>    - Expected: status 404, error message about destination not found
> 
> 10. SearchHotels_UnknownDestination_Returns404
>     - Request: destination=Tokyo (not in BudgetNests stubs, may be filtered)
>     - Expected: status 404 or empty results depending on implementation
> 
> ### Reservation Endpoint Tests (POST /hotels/reserve)
> 
> 11. ReserveHotel_ValidRequest_Returns200
>     - Body: valid ReservationRequest with all fields
>     - Expected: status 200, response contains referenceNumber
> 
> 12. ReserveHotel_ValidRequest_ReturnsConfirmationStructure
>     - Expected: response has referenceNumber, hotelName, provider, roomType, dates, prices, guestName, documentNumber (masked)
> 
> 13. ReserveHotel_MissingHotelId_Returns400
>     - Body: missing hotelId field
>     - Expected: status 400, error message about required field
> 
> 14. ReserveHotel_MissingGuestName_Returns400
>     - Expected: status 400
> 
> 15. ReserveHotel_MissingDocumentNumber_Returns400
>     - Expected: status 400
> 
> 16. ReserveHotel_InternationalDestination_NationalIdProvided_Returns422
>     - Destination: London, DocumentType: NationalId
>     - Expected: status 422, error message includes "Passport required"
> 
> 17. ReserveHotel_InternationalDestination_PassportProvided_Returns200
>     - Destination: London, DocumentType: Passport
>     - Expected: status 200, reservation succeeds
> 
> 18. ReserveHotel_DomesticDestination_NationalIdProvided_Returns200
>     - Destination: New York, DocumentType: NationalId
>     - Expected: status 200, reservation succeeds
> 
> 19. ReserveHotel_DocumentNumberMasked
>     - Input: documentNumber="P12345678"
>     - Expected: response documentNumber="P1234****"
> 
> ### Get Reservation Endpoint Tests (GET /hotels/reservation/{ref})
> 
> 20. GetReservation_ValidReference_Returns200
>     - First create reservation to get reference
>     - Then GET /hotels/reservation/{reference}
>     - Expected: status 200, returns full confirmation details
> 
> 21. GetReservation_InvalidReference_Returns404
>     - Request: GET /hotels/reservation/RES-00000000-INVALID
>     - Expected: status 404, error message "Reservation not found"
> 
> 22. GetReservation_ReturnsCorrectDetails
>     - Expected: response matches original reservation (hotelName, dates, prices, etc.)
> 
> **Integration Flow Test:**
> 
> 23. EndToEndFlow_SearchReserveAndRetrieve
>     - 1. Search for hotels (London, 2026-06-23 to 2026-06-27)
>     - 2. Extract first result's hotelId
>     - 3. Reserve with valid Passport
>     - 4. Extract referenceNumber from response
>     - 5. Retrieve reservation by reference
>     - 6. Verify all details match
>     - Expected: all steps succeed, data consistency
> 
> **Test Structure:**
> - Use xUnit [Fact] for single-case tests
> - Use [Theory] with [InlineData] for parameterized tests
> - Parse JSON responses properly
> - Use Assert.Equal, Assert.NotNull, Assert.Contains
> 
> **Generate the complete integration test file with all 23 test cases.**

---

## Prompt #7: Create reflection.md Document

**Context:** Project completion. Document what could be improved.

**Prompt:**
> I need to create a reflection.md file documenting lessons learned and potential improvements for the Hotel Stay project.
> 
> **File Location:** `reflection.md`
> 
> **Sections to Include:**
> 
> ### 1. What Went Well
> - List 5-7 things that worked successfully
> - Examples: API design, service layer, testing approach, frontend UX, etc.
> 
> ### 2. Challenges Encountered
> - List 5-7 challenges faced during development
> - For each, explain:
>   - What the challenge was
>   - How it was solved
>   - What was learned
> - Examples: DI container scope issues, enum serialization, frontend-backend integration, etc.
> 
> ### 3. Code Quality & Architecture
> - SOLID Principles: How well they were followed
> - Design Patterns: What patterns were used (Adapter, DI, Service Layer)
> - Code Organization: Structure and clarity
> - Testing Coverage: Current coverage and gaps
> 
> ### 4. Potential Improvements (If More Time)
> 
> #### Backend Improvements
> - [ ] Add persistence layer (database)
> - [ ] Add authentication/authorization
> - [ ] Add logging and monitoring
> - [ ] Add rate limiting for APIs
> - [ ] Add caching for search results
> - [ ] Add comprehensive error handling
> - [ ] Add API versioning
> - [ ] Add more providers
> - [ ] Add price filtering and range search
> - [ ] Add sorting options (by price, rating, amenities)
> 
> #### Frontend Improvements
> - [ ] Add filtering and advanced search
> - [ ] Add favorites/wishlist feature
> - [ ] Add responsive design for mobile
> - [ ] Add dark mode
> - [ ] Add multi-language support
> - [ ] Add reservation history
> - [ ] Migrate to modern framework (React, Vue)
> - [ ] Add PWA support
> - [ ] Add accessibility improvements (WCAG 2.1)
> - [ ] Add client-side caching
> 
> #### Testing Improvements
> - [ ] End-to-end tests with Selenium/Cypress
> - [ ] Performance testing
> - [ ] Load testing
> - [ ] Security testing
> - [ ] API contract testing
> 
> #### DevOps & Deployment
> - [ ] Docker containerization
> - [ ] CI/CD pipeline (GitHub Actions)
> - [ ] Environment configuration (dev/staging/prod)
> - [ ] Azure/AWS deployment
> - [ ] Database migration scripts
> 
> ### 5. Lessons Learned
> - Key takeaways about architecture decisions
> - Testing strategies that worked well
> - Communication between frontend and backend
> - Working with AI assistance (Copilot)
> 
> ### 6. Code Metrics
> - Current test coverage percentage
> - Number of test cases created
> - Code complexity notes
> 
> ### 7. Future Roadmap
> - Prioritized list of next features to implement
> - Estimated effort for each
> - Business value assessment
> 
> **Tone:** Professional, honest, constructive
> **Length:** 2000-3000 words
> **Format:** Markdown with clear sections and bullet points
> 
> **Generate the complete reflection.md file.**

---

## How to Use These Prompts

1. **Copy each prompt individually**
2. **Paste into GitHub Copilot chat or VS Code (Ctrl+I)**
3. **Copilot will generate test files**
4. **Review and adjust as needed**
5. **Commit each test file to git**

---

## Order of Execution

1. ✅ Prompt #1: PremierStaysProvider Tests
2. ✅ Prompt #2: BudgetNestsProvider Tests
3. ✅ Prompt #3: DocumentValidator Tests
4. ✅ Prompt #4: HotelSearchService Tests
5. ✅ Prompt #5: ReservationService Tests
6. ✅ Prompt #6: Integration Tests
7. ✅ Prompt #7: reflection.md

After completing these, run:
```bash
dotnet test --verbosity detailed
```

This will execute all tests and give you coverage metrics.

