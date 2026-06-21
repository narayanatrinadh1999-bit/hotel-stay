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

*(Prompts will be added here as we develop the application)*

### Prompt #1: Design IHotelProvider Interface
**Date:** 2024-06-21  
**Context:** Starting backend implementation, designing the provider abstraction  
**Prompt:**
> Design an adapter pattern interface for aggregating hotel search results from multiple providers with different response formats. PremierStays returns full details in PascalCase JSON, BudgetNests returns minimal details in snake_case JSON. Both have different cancellation policy representations. How should I structure the IHotelProvider interface and normalization logic to make it extensible for a third provider?

**Response Summary:** Copilot suggested a clean interface with a single SearchAsync method returning a common model, with provider-specific parsing in each implementation.

**Decision:** Adopted the interface-based approach. Each provider handles its own JSON deserialization and mapping to the unified HotelRoom model. This keeps concerns separated and makes testing easier.

**Result:** Clean abstraction that makes adding a third provider straightforward without touching core logic.

---

### Prompt #2: Document Validation Logic
**Date:** 2024-06-21  
**Context:** Implementing ReservationService document validation  
**Prompt:**
> I need to validate documents for hotel reservations. International destinations (London, Paris, Tokyo) require a Passport only. Domestic destinations (New York, Los Angeles) accept either Passport or NationalId. If there's a mismatch, I should return HTTP 422 with a clear message. Where should this logic live and how should I structure it?

**Response Summary:** Suggested a dedicated IDocumentValidator service with clear error messages, and recommended putting destination configuration in a settings or constants class.

**Decision:** Created a separate DocumentValidator service that handles the rules. Used a simple Dictionary to map destinations to their regions (domestic/international). This keeps validation logic testable and decoupled from the reservation service.

**Result:** Validation is easy to test in isolation and can be reused across endpoints.

---

*(More prompts will be documented as development progresses)*

---

## Key Design Decisions

### 1. In-Memory Storage
**Why:** The spec doesn't require persistence, and in-memory storage is simpler, faster, and makes testing deterministic.

### 2. No External Dependencies
**Why:** The requirement states the app must run fully offline on a local machine. No databases, no APIs, no credentials.

### 3. Minimal APIs over Controllers
**Why:** .NET 8 Minimal APIs are lightweight, performant, and reduce boilerplate. Easier to understand in an interview setting.

### 4. Separate Service Layer
**Why:** Keeps business logic (search, normalization, validation) separate from HTTP concerns. Makes unit testing easier and follows SOLID principles.

---

## Reflection on AI Usage

- **Strengths:** Copilot helped with interface design, error handling patterns, and async/await best practices.
- **Weaknesses:** Occasionally suggested overly complex solutions; had to simplify and ask more specific follow-up prompts.
- **Best Practice:** Most effective when asking Copilot "why" and "design" questions before jumping to code. Design first, then code.

---

*(This document will be completed during and after implementation)*
