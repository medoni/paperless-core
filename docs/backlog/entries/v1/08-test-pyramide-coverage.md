# 08 - Test-Pyramide & Coverage

**Status:** 🚫 Won't Do (POC)
**Meilenstein:** V1
**Priorität:** Mittel

## Anforderungen

- Test-Strategie definieren
- Coverage-Ziele festlegen
- Test-Typen priorisieren

## Entscheidung

- Coverage-Ziel:
  - Geschäftsregeln: 80% (via Behavior Tests oder Integration Tests)
  - Unit Tests: 70% (ausreichend für V1)
  - Bootstrapper/Registration: Nicht erforderlich
- Testing-Frameworks:
  - C#: xUnit + FluentAssertions
  - Node.js: Jest oder Vitest
  - Frontend: Vitest + Playwright (E2E)
- Load Tests (später): k6 + Smoke Tests

## Test-Pyramide

```
         /\
        /  \
       /E2E \         <- Kritische User Flows (Playwright)
      /------\
     /Integr. \       <- API Tests, Business Rules (80%)
    /----------\
   / Unit Tests \     <- Domain Logic (70%)
  /--------------\
```

## Prioritäten

1. Integration/Behavior Tests: Geschäftsregeln, Use Cases (80%)
2. Unit Tests: Domain Entities, Value Objects (70%)
3. E2E Tests: Upload, Search, Classification
4. Smoke Tests: Health Checks, Basic Functionality
5. Load Tests (V2): k6 für Performance-Validierung

## KISS beachten

- Keine Test-Pyramide für Test-Pyramide
- Pragmatisch: Testen was wichtig ist
- Business Value über Coverage-Metriken

## ADR erforderlich

ADR-006: Testing Strategy & Test Pyramid
