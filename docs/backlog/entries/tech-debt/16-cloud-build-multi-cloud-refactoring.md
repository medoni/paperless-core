# 16 - Cloud Build Configuration: Multi-Cloud Refactoring

**Status:** ⏳ To Do
**Meilenstein:** Tech Debt
**Priorität:** Mittel

## Problem

- `cloudbuild.yaml` ist GCP-spezifisch und liegt aktuell im Anwendungscode
- Bei AWS-Migration muss `cloudbuild.yaml` durch `buildspec.yml` ersetzt werden
- Cloud-Provider-spezifische Build-Konfiguration sollte environment-spezifisch sein

## Refactoring-Aufgabe

Bei AWS-Migration müssen `cloudbuild.yaml` Dateien nach `src/infrastructure/gcp/` verschoben werden:

```
src/infrastructure/gcp/build-configs/
  ├── api.cloudbuild.yaml
  └── frontend.cloudbuild.yaml
src/infrastructure/aws/build-configs/
  ├── api.buildspec.yml
  └── frontend.buildspec.yml
```

## Betroffene Dateien

- `src/frontend/cloudbuild.yaml` → verschieben nach `src/infrastructure/gcp/build-configs/`
- `src/backend/PLC.Api/cloudbuild.yaml` → verschieben nach `src/infrastructure/gcp/build-configs/`
- Terraform container-build Modul: Pfad zu cloudbuild.yaml anpassen
