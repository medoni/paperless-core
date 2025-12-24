# 04 - Dateinamen & Deep Links

**Status:** ⏳ To Do
**Meilenstein:** V1
**Priorität:** Hoch

## Anforderungen

- Frontend: Lesbare IDs/Slugs (z.B. `rewe-kassenbon-2025-12-22`)
- Intern: UUID für Dokumente
- Deep Links: Direkte Verlinkung zu Dokumenten

## Entscheidung

- URL-Schema: `/documents/{document-type}/{year}-{merchant}-{description}-{short-uuid}`
- Slug-Format dokumenttyp-abhängig, konfigurierbar via YAML
- Kollision: Short-UUID garantiert Eindeutigkeit

## Beispiele

```
Kassenzettel:  /documents/receipt/2025-rewe-groceries-a4f9
Rechnung:      /documents/invoice/2025-telekom-december-b2c3
Lohnzettel:    /documents/salary/2025-12-company-d5e6
Bauunterlagen: /documents/construction/2025-roof-repair-f7g8
```

## Backend

- Document ID: UUID (550e8400-e29b-41d4-a716-446655440000)
- Slug wird generiert aus Metadaten + Short-UUID (erste 4 Zeichen)

## YAML Config

```yaml
subcategories:
  - id: receipts
    slugPattern: "{year}-{merchant}-{description}-{shortUuid}"
```
