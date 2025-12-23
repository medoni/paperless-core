# ADR-004: OpenTofu vs Terraform for Infrastructure as Code

## Status

Accepted

## Kontext

PaperlessCore benötigt Infrastructure as Code (IaC) für Multi-Cloud Deployments (GCP primär, AWS sekundär, optional K3s). Die Wahl des IaC-Tools ist fundamental für das Projekt.

### Anforderungen

**Funktional:**
- Multi-Cloud Support (GCP, AWS)
- Deklarative Konfiguration
- State Management
- Modulare Struktur
- Provider-Ökosystem (Cloud-Services)

**Non-Funktional:**
- Open Source mit stabiler Lizenz
- Community-getrieben
- Langfristig wartbar
- Keine Vendor Lock-in Risiken
- Terraform-kompatibel (Migration möglich)

### Hintergrund: Terraform Lizenzänderung

**HashiCorp Terraform:**
- Bis Version 1.5.x: Mozilla Public License 2.0 (MPL 2.0, Open Source)
- Ab Version 1.6.0 (August 2023): Business Source License 1.1 (BSL 1.1)

**BSL 1.1 Einschränkungen:**
- Nicht Open Source im klassischen Sinne
- Kommerzielle Nutzung eingeschränkt
- Keine Konkurrenzprodukte auf Basis von Terraform erlaubt
- Nach 4 Jahren Übergang zu MPL 2.0

**Risiken:**
- Unklare langfristige Lizenzstrategie
- Potenzielle weitere Einschränkungen
- Abhängigkeit von HashiCorp-Entscheidungen

### OpenTofu Alternative

**OpenTofu:**
- Fork von Terraform 1.5.x (vor Lizenzänderung)
- Linux Foundation Projekt (seit September 2023)
- Mozilla Public License 2.0 (MPL 2.0, echtes Open Source)
- Community-getrieben (IBM, Spacelift, env0, Harness, u.a.)
- Terraform-kompatibel (Drop-in Replacement)

## Entscheidung

Wir verwenden **OpenTofu** als IaC-Tool für PaperlessCore.

### Begründung

**Lizenz-Sicherheit:**
- MPL 2.0 garantiert Open Source Status
- Keine Einschränkungen für kommerzielle Nutzung
- Community-kontrolliert (Linux Foundation)

**Terraform-Kompatibilität:**
- Drop-in Replacement für Terraform
- Bestehende Terraform-Module nutzbar
- Terraform-Wissen übertragbar
- Migration zurück zu Terraform möglich (falls nötig)

**Community & Zukunftssicherheit:**
- Breite Industrie-Unterstützung
- Aktive Entwicklung
- Provider-Ökosystem wächst
- Langfristige Stabilität durch Foundation

**Technisch:**
- Alle Features von Terraform 1.5.x
- Multi-Cloud Support (GCP, AWS)
- State Management
- Module System
- Provider Registry

### Implementierung

**Installation:**
```bash
# Via package manager
brew install opentofu  # macOS
snap install opentofu  # Linux

# Verifikation
tofu --version
```

**Projektstruktur:**
```
/infrastructure
  /gcp
    /envs
      /dev
        main.tf
        variables.tf
        outputs.tf
      /prod
    /modules
      /storage
      /functions
      /api-gateway
  /aws
    /envs
    /modules
```

**Befehle:**
```bash
tofu init     # statt terraform init
tofu plan     # statt terraform plan
tofu apply    # statt terraform apply
```

**State Backend:**
- GCP: Google Cloud Storage Bucket
- AWS: S3 Bucket
- Verschlüsselt
- Versioniert

## Konsequenzen

### Positiv

- Open Source Lizenz ohne Einschränkungen
- Zukunftssicher durch Community-Governance
- Terraform-kompatibel, einfache Migration
- Breite Industrie-Unterstützung
- Kein Vendor Lock-in Risiko
- Bestehende Terraform-Kenntnisse nutzbar
- Provider-Ökosystem verfügbar

### Negativ

- Relativ junges Projekt (seit 2023)
- Kleinere Community als Terraform
- Potenzielle Divergenz von Terraform in Zukunft
- Manche neue Terraform-Features könnten später kommen

### Risiken

**OpenTofu Development stagniert**

Mitigation: Linux Foundation Backing, breite Industrie-Unterstützung, Migration zurück zu Terraform möglich (kompatibel)

**Provider-Support unvollständig**

Mitigation: Wichtigste Provider (GCP, AWS) voll unterstützt, Community wächst, Terraform-Provider nutzbar

**Divergenz von Terraform**

Mitigation: Akzeptables Risiko, Community entscheidet Features, bei Bedarf Migration zu Terraform

## Referenzen

- OpenTofu: https://opentofu.org/
- OpenTofu GitHub: https://github.com/opentofu/opentofu
- Linux Foundation Announcement: https://www.linuxfoundation.org/press/announcing-opentofu
- Terraform Lizenzänderung: https://www.hashicorp.com/blog/hashicorp-adopts-business-source-license
- OpenTofu Registry: https://github.com/opentofu/registry
