# PaperlessCore API - Examples

Beispiele für manuelle Tests der PLC API.

## 📁 Dateien

- **`upload-document.http`** - HTTP-Requests für VS Code REST Client / IntelliJ
- **`curl-examples.sh`** - Bash-Script mit cURL-Beispielen
- **`README.md`** - Diese Datei

## 🚀 Quick Start

### Option 1: HTTP-Datei (VS Code)

1. Installiere die [REST Client Extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)
2. Öffne `upload-document.http`
3. Klicke auf "Send Request" über dem Request

### Option 2: cURL Script

```bash
# Health Check
./curl-examples.sh health

# Upload Document
./curl-examples.sh upload

# Get Document (verwendet ID vom letzten Upload)
./curl-examples.sh get

# Poll Status für 10 Sekunden
./curl-examples.sh poll

# Upload zu lokalem Server
./curl-examples.sh upload-local
```

## 📝 Beispiel-Workflow

### 1. Health Check
```bash
curl https://plc-api-bx7xdsbeoa-ew.a.run.app/health | jq .
```

Response:
```json
{
  "status": "healthy",
  "timestamp": "2025-12-24T16:00:00Z",
  "version": "0.1.0-dev",
  "environment": "Development"
}
```

### 2. Document Upload
```bash
./curl-examples.sh upload
```

Response:
```json
{
  "uploadId": "upload-550e8400-e29b-41d4-a716-446655440000",
  "documents": [{
    "id": "doc-550e8400-e29b-41d4-a716-446655440001",
    "fileName": "20250117 Rechnung Brille.pdf",
    "fileSize": 555881,
    "mimeType": "application/pdf",
    "status": "Processing",
    "uploadedAt": "2025-12-24T16:00:00Z"
  }]
}
```

### 3. Check Document Status
```bash
# Ersetze <documentId> mit der ID aus dem Upload
curl https://plc-api-bx7xdsbeoa-ew.a.run.app/api/v1/documents/<documentId> | jq .
```

Response (während Processing):
```json
{
  "id": "doc-550e8400-e29b-41d4-a716-446655440001",
  "fileName": "20250117 Rechnung Brille.pdf",
  "status": "Processing",
  "category": null,
  "slug": null,
  "tags": ["2025", "brille", "garantie"],
  "assignedTo": ["Micha"]
}
```

Response (nach Classification - **noch nicht implementiert!**):
```json
{
  "id": "doc-550e8400-e29b-41d4-a716-446655440001",
  "fileName": "20250117 Rechnung Brille.pdf",
  "status": "Finished",
  "category": "everyday-consumption",
  "subcategory": "receipts",
  "classificationConfidence": 0.95,
  "slug": "2025-rechnung-brille-550e",
  "tags": ["2025", "brille", "garantie"],
  "assignedTo": ["Micha"]
}
```

## ⚠️ Aktueller Status

### ✅ Funktioniert bereits:
- Document Upload zu Google Cloud Storage
- Metadata-Speicherung in Firestore
- Pub/Sub Event Publishing
- Status-Abfrage per API

### ❌ Noch nicht implementiert:
- **Document Classification** (Classifier Worker fehlt!)
- **Slug Generation**
- Status-Übergang von `Processing` → `Finished`

**Was passiert aktuell:**
1. ✅ Upload → Storage + Firestore → Status: `Processing`
2. ✅ Pub/Sub Event wird published
3. ❌ **Kein Worker hört auf das Event!**
4. ❌ Dokument bleibt in `Processing` Status

**Nächster Schritt:**
Node.js Classifier Worker implementieren!

## 🔧 Konfiguration

### URLs
- **Production API:** `https://plc-api-bx7xdsbeoa-ew.a.run.app`
- **Local API:** `http://localhost:8080`

### Beispiel-Datei
- **Pfad:** `/mnt/c/Users/Micha/iCloudDrive/Dokumente3/Kassenzettel_Garantie/2025/20250117 Rechnung Brille.pdf`
- **Größe:** ~556 KB
- **Typ:** PDF

### Erlaubte Formate
- PDF (`.pdf`)
- JPEG (`.jpg`, `.jpeg`)
- PNG (`.png`)

### Limits
- **Max File Size:** 50 MB
- **Max Batch Size:** 100 Dateien

## 🐛 Troubleshooting

### "File not found" Error
```bash
# Prüfe ob Datei existiert
ls -la "/mnt/c/Users/Micha/iCloudDrive/Dokumente3/Kassenzettel_Garantie/2025/20250117 Rechnung Brille.pdf"

# Falls nicht, passe den Pfad in den Beispieldateien an
```

### "INVALID_FILE_FORMAT" Error
Nur PDF, JPG, JPEG, PNG sind erlaubt.

### "FILE_TOO_LARGE" Error
Datei überschreitet 50 MB Limit.

### Dokument bleibt in "Processing"
Das ist normal! Der Classifier Worker ist noch nicht deployed.
Das Dokument wartet auf die Klassifizierung.

## 📚 Weitere Informationen

- **API Documentation:** Swagger UI unter `https://plc-api-bx7xdsbeoa-ew.a.run.app/swagger`
- **OpenAPI Spec:** `src/api/openapi.yaml`
- **Backend README:** `src/backend/README.md`
