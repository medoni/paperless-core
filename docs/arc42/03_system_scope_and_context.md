# 3. Kontextabgrenzung

## 3.1 Fachlicher Kontext

> TBD - wird nach Architektur-Workshop ausgefüllt

### Externe Systeme und Schnittstellen

```
[User] --> [PaperlessCore] --> [Cloud OCR Service]
                              --> [Object Storage]
                              --> [Database]
                              --> [Search Engine]
```

### Wichtige Schnittstellen
- **Cloud OCR**: Google Document AI / AWS Textract
- **Object Storage**: Google Cloud Storage / AWS S3
- **Database**: Firestore / DynamoDB
- **Search**: Elasticsearch / MongoDB

## 3.2 Technischer Kontext

> TBD - wird nach Architektur-Workshop ausgefüllt

### Deployment-Kontext
- Google Cloud Platform (primär)
- AWS (sekundär)
- K3s (optional, self-hosted)

### Integration
- REST APIs
- Event-driven (Cloud Functions / Lambda)
- Object Storage Events

## 3.3 Externe Schnittstellen

> TBD - detaillierte Schnittstellenbeschreibungen folgen
