# 01 - API-First Approach

**Status:** ⏳ To Do
**Meilenstein:** V1
**Priorität:** Hoch

## Anforderung

- API-Spec gemeinsam mit Frontend und Backend definieren (OpenAPI/Swagger)
- Erst Definition, dann parallele Implementierung
- Mocks für parallele Entwicklung

## Nächste Schritte

- OpenAPI Spec für Upload, Documents, Search erstellen
- Review mit Product Owner
- Mock Server aufsetzen

## Entscheidung

- Tool: Swagger Editor mit VS Code Extension
- KI-unterstützt (z.B. via Claude Code statt manuell YAML fummeln)
- Versioning: `/api/v1/...`
- OpenAPI 3.x Spec im Repo versioniert

## ADR erforderlich

ADR-004: API Design & Versioning Strategy
