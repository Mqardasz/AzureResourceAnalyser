# Azure Resource Analyzer - Przykładowa konfiguracja

## Konfiguracja uwierzytelniania

### Opcja 1: Azure CLI (zalecane dla rozwoju lokalnego)
```bash
# Zaloguj się do Azure
az login

# Opcjonalnie: ustaw domyślną subskrypcję
az account set --subscription "Nazwa-Subskrypcji"
```

### Opcja 2: Service Principal (zalecane dla CI/CD)
```bash
# Windows PowerShell
$env:AZURE_TENANT_ID="twój-tenant-id"
$env:AZURE_CLIENT_ID="twój-client-id"
$env:AZURE_CLIENT_SECRET="twój-client-secret"

# Linux/macOS
export AZURE_TENANT_ID="twój-tenant-id"
export AZURE_CLIENT_ID="twój-client-id"
export AZURE_CLIENT_SECRET="twój-client-secret"
```

### Opcja 3: Managed Identity (dla aplikacji w Azure)
Nie wymaga konfiguracji - automatycznie używane gdy aplikacja działa w Azure
(Azure VM, App Service, Functions, Container Instances, AKS)

## Konfiguracja reguł

Aby dostosować reguły zgodności, edytuj plik:
`AzureResourceAnalyser/Services/RuleManager.cs`

### Przykład: Włączenie wszystkich reguł
```csharp
private void LoadDefaultRules()
{
    // Podstawowe reguły
    _rules.Add(new VmTagRule());
    _rules.Add(new VmSizeRule());
    _rules.Add(new SshRule());
    _rules.Add(new DiskSizeRule());
    _rules.Add(new StorageEncryptionRule());
    
    // Reguły organizacyjne
    _rules.Add(new BillingTagRule());
    _rules.Add(new LocationRule());
    _rules.Add(new NamingConventionRule());
}
```

### Przykład: Dodanie własnej reguły
```csharp
// W Program.cs przed analizą
var ruleManager = new RuleManager();
ruleManager.AddRule(new MyCustomRule());
```

## Dostosowanie dozwolonych wartości

### LocationRule - Dozwolone regiony
Edytuj `Rules/LocationRule.cs`:
```csharp
private readonly HashSet<string> _allowedLocations = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "westeurope",    // Europa Zachodnia
    "northeurope",   // Europa Północna
    "eastus",        // Wschód USA
    "westus",        // Zachód USA
    "polandcentral"  // Polska Środkowa (dodaj własne)
};
```

### BillingTagRule - Wymagane tagi
Edytuj `Rules/BillingTagRule.cs`:
```csharp
private readonly string[] _requiredTags = { 
    "CostCenter",    // Centrum kosztów
    "Project",       // Projekt
    "Owner",         // Właściciel
    "Environment"    // Dodaj własne
};
```

### NamingConventionRule - Konwencja nazewnictwa
Edytuj `Rules/NamingConventionRule.cs`:

Dozwolone prefixy:
```csharp
private readonly HashSet<string> _validPrefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "vm", "disk", "storage", "app", "db", "sql", "web"
};
```

Dozwolone środowiska:
```csharp
private readonly HashSet<string> _validEnvironments = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "prod", "dev", "test", "staging", "uat"
};
```

## Wymagane uprawnienia Azure

Aby aplikacja mogła odczytywać zasoby, wymaga następujących uprawnień w Azure:

### Minimalne uprawnienia (Role RBAC):
- **Reader** - na poziomie subskrypcji lub resource group
  
Lub bardziej restrykcyjne:
- **Virtual Machine Reader** - dla VM
- **Storage Account Reader** - dla Storage
- **Web App Reader** - dla App Services
- **SQL DB Reader** - dla baz danych

### Przyznawanie uprawnień (Azure CLI):
```bash
# Przyznaj uprawnienia Reader dla service principal
az role assignment create \
  --assignee <client-id> \
  --role "Reader" \
  --scope "/subscriptions/<subscription-id>"
```

### Sprawdzenie uprawnień:
```bash
# Pokaż przypisane role
az role assignment list --assignee <client-id> --all
```

## Integracja z CI/CD

### GitHub Actions
```yaml
name: Azure Resource Compliance Check

on:
  schedule:
    - cron: '0 0 * * *'  # Codziennie o północy
  workflow_dispatch:

jobs:
  analyze:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Run Azure Resource Analyzer
        env:
          AZURE_TENANT_ID: ${{ secrets.AZURE_TENANT_ID }}
          AZURE_CLIENT_ID: ${{ secrets.AZURE_CLIENT_ID }}
          AZURE_CLIENT_SECRET: ${{ secrets.AZURE_CLIENT_SECRET }}
        run: |
          cd AzureResourceAnalyser
          dotnet run
      
      - name: Upload reports
        uses: actions/upload-artifact@v3
        with:
          name: compliance-reports
          path: |
            analysis_report.json
            prometheus_metrics.txt
```

### Azure DevOps Pipeline
```yaml
trigger:
- main

schedules:
- cron: "0 0 * * *"
  displayName: Daily compliance check
  branches:
    include:
    - main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: UseDotNet@2
  inputs:
    version: '8.0.x'

- task: DotNetCoreCLI@2
  displayName: 'Run Compliance Check'
  inputs:
    command: 'run'
    projects: 'AzureResourceAnalyser/AzureResourceAnalyser.csproj'
  env:
    AZURE_TENANT_ID: $(AZURE_TENANT_ID)
    AZURE_CLIENT_ID: $(AZURE_CLIENT_ID)
    AZURE_CLIENT_SECRET: $(AZURE_CLIENT_SECRET)

- task: PublishBuildArtifacts@1
  inputs:
    pathToPublish: '$(Build.SourcesDirectory)'
    artifactName: 'compliance-reports'
```

## Integracja z Prometheus/Grafana

1. **Skopiuj metryki do lokalizacji Prometheus:**
```bash
# Linux/macOS
cp prometheus_metrics.txt /var/lib/prometheus/textfile_collector/azure_compliance.prom

# Windows
copy prometheus_metrics.txt "C:\Program Files\Prometheus\textfile_collector\azure_compliance.prom"
```

2. **Skonfiguruj Prometheus** (`prometheus.yml`):
```yaml
scrape_configs:
  - job_name: 'textfile'
    static_configs:
      - targets: ['localhost:9100']
    metric_relabel_configs:
      - source_labels: [__name__]
        regex: 'azure_.*'
        action: keep
```

3. **Przykładowe zapytania Grafana:**
```promql
# Wskaźnik zgodności w procentach
100 * azure_resources_compliant / azure_resources_total

# Niezgodne zasoby
azure_resources_noncompliant

# Zgodność per typ zasobu
azure_resources_compliant_microsoft_compute_virtualmachines / 
  azure_resources_by_type_microsoft_compute_virtualmachines * 100
```

## Rozwiązywanie problemów

### Problem: "Unable to authenticate"
**Rozwiązanie:**
```bash
# Sprawdź czy jesteś zalogowany do Azure CLI
az account show

# Jeśli nie, zaloguj się
az login
```

### Problem: "Forbidden" lub "Unauthorized"
**Rozwiązanie:**
Sprawdź uprawnienia:
```bash
az role assignment list --assignee <your-id> --all
```
Przypisz rolę Reader jeśli jej nie ma.

### Problem: "No subscriptions found"
**Rozwiązanie:**
```bash
# Lista dostępnych subskrypcji
az account list --output table

# Ustaw domyślną subskrypcję
az account set --subscription "<subscription-name-or-id>"
```

### Problem: Brak zasobów w raporcie
**Przyczyny:**
1. Brak uprawnień do odczytu zasobów
2. Zasoby są w innej subskrypcji
3. Resource group jest puste

**Rozwiązanie:**
- Sprawdź logi aplikacji
- Zweryfikuj uprawnienia
- Sprawdź czy zasoby rzeczywiście istnieją w Azure Portal
