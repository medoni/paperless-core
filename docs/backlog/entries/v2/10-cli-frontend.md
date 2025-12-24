# 10 - CLI Frontend

**Status:** 🚫 Won't Do (POC)
**Meilenstein:** V2
**Priorität:** Niedrig

## Anforderungen

- Command-Line Interface als alternative Frontend
- Funktionen: Upload, Search, List, Export

## Beispiel-Commands

```bash
plc upload receipt.pdf --category receipts --tags "rewe,2025"
plc search --query "REWE" --year 2025
plc list --category receipts --year 2025
plc export --format csv --year 2024 --category receipts
```

## Technologie

- Node.js CLI (Commander.js, Inquirer)
- Oder Go CLI (Cobra)
