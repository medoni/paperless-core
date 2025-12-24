# 03 - Domain für Testumgebung

**Status:** ⏳ To Do
**Meilenstein:** V1
**Priorität:** Hoch

## Anforderung

- Dedizierte Domain für Test/Dev/Staging
- Separate Umgebung von Production

## Entscheidung

- Dev: `dev.paperless-core.squirreldev.de`
- Prod: `prod.paperless-core.squirreldev.de`
- Subdomain-Strategie:
  - API: `api.{env}.paperless-core.squirreldev.de`
  - Frontend: `{env}.paperless-core.squirreldev.de`
- SSL: Let's Encrypt (automatisiert via Terraform)
- IaC: Environment als Terraform Module
