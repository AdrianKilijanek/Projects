# RekJust LLM Prompt Processor

Mikroserwisowa aplikacja do asynchronicznego przetwarzania promptów przez lokalny model AI.
Użytkownik wysyła pytanie przez interfejs webowy, system przetwarza je w tle przez Ollama (llama3.2)
i wyświetla odpowiedź gdy jest gotowa. Odpowiedź z API wraca w ~200 ms, generowanie przez LLM zajmuje 2–30 sekund.

---

## Stos technologiczny

| Warstwa | Technologia | Rola |
|---------|-------------|------|
| Frontend | Next.js 15 + TanStack Query | UI, polling statusów co 3s |
| API Gateway | Nginx | Reverse proxy, jeden punkt wejścia |
| HTTP API | ASP.NET Core 10 (Minimal API) | Przyjmuje prompty, waliduje, zwraca listę |
| Message Queue | RabbitMQ 3.13 + MassTransit | Asynchroniczna kolejka zadań |
| Worker | .NET 10 Worker Service + Semantic Kernel | Konsumuje kolejkę, wywołuje LLM |
| Model AI | Ollama (llama3.2, lokalnie) | Generuje odpowiedź tekstową |
| Baza danych | PostgreSQL 16 + Entity Framework Core | Przechowuje prompty i wyniki |
| Konteneryzacja | Docker Compose | Orkiestracja całego systemu |

---

## Wymagania

| Program | Wersja | Uwaga |
|---------|--------|-------|
| Docker Desktop | 27.0+ | Musi być uruchomiony przed `docker compose up` |
| .NET SDK | 10.0+ | Wymagany tylko do uruchamiania testów |

**Node.js nie jest wymagany** frontend buduje się wewnątrz Dockera.

**Wymagania sprzętowe:** min. 8 GB RAM (model llama3.2 używa ~4 GB podczas pracy), 8 GB wolnego miejsca na dysku.

---

## Uruchomienie jednym poleceniem

```bash
docker compose up --build
```

Przy **pierwszym uruchomieniu** Docker pobiera obrazy bazowe (~2–4 GB) i buduje serwisy aplikacji.
Zajmuje kilka minut. Przy **kolejnych uruchomieniach** wystarczy `docker compose up` (pomija rebuild).

### Pobranie modelu AI jednorazowo

Po uruchomieniu systemu otwórz drugi terminal i wykonaj:

```bash
docker compose exec ollama ollama pull llama3.2
```

Model (~2 GB) zapisuje się w wolumenie Docker, nie trzeba pobierać ponownie po restarcie.

### Sygnał gotowości

System jest gotowy do użycia gdy w logach pojawią się obie linie:

```
prompt-api-1     | Now listening on: http://[::]:8080
worker-service-1 | Bus started: rabbitmq://rabbitmq/
```

Otwórz przeglądarkę: **http://localhost**

---

## Dostęp do serwisów

| Serwis | Adres | Login / Hasło |
|--------|-------|---------------|
| **Aplikacja (UI)** | http://localhost | — |
| RabbitMQ Management | http://localhost:15672 | `rekjust` / `rekjust123` |
| PromptApi REST | http://localhost:8080/api/prompts | — |
| Ollama API | http://localhost:11434 | — |
| PostgreSQL | `localhost:5432` db: `rekjust` | `rekjust` / `rekjust123` |

---

## Jak korzystać z aplikacji

1. Otwórz **http://localhost** w przeglądarce
2. Wpisz pytanie w pole tekstowe (minimum 3, maksimum 4000 znaków)
3. Kliknij **Wyślij** — prompt pojawi się na liście ze statusem **Oczekuje**
4. Lista odświeża się automatycznie co 3 sekundy

### Statusy promptów

| Status | Znaczenie |
|--------|-----------|
| **Oczekuje** | Zapisany w bazie, czeka w kolejce RabbitMQ |
| **Przetwarza** | Worker podjął zadanie, LLM generuje odpowiedź |
| **Gotowe** | Odpowiedź gotowa — wyświetlona poniżej pytania |
| **Błąd** | LLM nie odpowiedział po kilku próbach — komunikat błędu poniżej |

---

## Architektura - przepływ danych

```
Przeglądarka
    │  POST /api/prompts
    │  GET  /api/prompts  (co 3s)
    ▼
  Nginx :80  (reverse proxy)
    ├── /api/*  ──►  PromptApi :8080
    └──   /*    ──►  Frontend  :3000

PromptApi (ASP.NET Core)
    ├── waliduje dane wejściowe
    ├── INSERT do PostgreSQL  (status: pending)
    └── Publish PromptCreated ──►  RabbitMQ

RabbitMQ
    └── dostarcza wiadomość ──►  WorkerService

WorkerService (.NET Worker)
    ├── UPDATE status → processing  (atomowy, zapobiega podwójnemu przetworzeniu)
    ├── Semantic Kernel ──►  Ollama :11434  (llama3.2)
    └── UPDATE status → completed + wynik

Frontend (Next.js)
    └── polling GET /api/prompts  ──►  wyświetla wyniki
```

---

## Uruchamianie poszczególnych komponentów

```bash
# Tylko infrastruktura (baza danych + kolejka)
docker compose up postgres rabbitmq

# Backend bez frontendu
docker compose up postgres rabbitmq ollama prompt-api worker-service

# Przebuduj i uruchom jeden serwis po zmianach w kodzie
docker compose up --build prompt-api
docker compose up --build worker-service
docker compose up --build frontend

# Restart serwisu bez przebudowania (po zmianie konfiguracji)
docker compose restart worker-service
```

---

## Zarządzanie systemem

```bash
# Stan wszystkich serwisów
docker compose ps

# Logi na żywo
docker compose logs -f
docker compose logs -f worker-service     # tylko Worker
docker compose logs -f prompt-api         # tylko API

# Zatrzymaj (dane w bazie i model AI są zachowane)
docker compose down

# Zatrzymaj i usuń wszystkie dane — pełny reset
docker compose down -v

# Zużycie zasobów (CPU, RAM)
docker stats
```

---

## Konfiguracja

Wszystkie ustawienia środowiskowe znajdują się w `docker-compose.yml` w sekcji `environment`.
Po zmianie tego pliku wystarczy `docker compose restart <serwis>` — nie trzeba przebudowywać obrazów.

### Zmiana modelu AI

```bash
# Pobierz wybrany model (dostępne: llama3.2, llama3.2:1b, mistral, phi3:mini, codellama)
docker compose exec ollama ollama pull mistral

# Lista pobranych modeli
docker compose exec ollama ollama list
```

Edytuj `docker-compose.yml`:

```yaml
worker-service:
  environment:
    Ollama__Model: mistral    # ← zmień nazwę modelu
```

```bash
docker compose restart worker-service
```

### Przełączenie na OpenAI zamiast Ollama

W `WorkerService/Program.cs` zamień:

```csharp
// Usuń:
.AddOllamaChatCompletion("llama3.2", new Uri("http://ollama:11434"))

// Dodaj:
.AddOpenAIChatCompletion("gpt-4o-mini", Environment.GetEnvironmentVariable("OPENAI_KEY"))
```

Następnie w `docker-compose.yml` dodaj zmienną `OpenAI__Key: "sk-..."` i przebuduj:

```bash
docker compose up --build worker-service
```

### Rozmiary modeli AI

| Model | Rozmiar | RAM | Szybkość | Kiedy używać |
|-------|---------|-----|---------|--------------|
| `llama3.2:1b` | 600 MB | ~1 GB | Szybki | Słabszy sprzęt, testy |
| `llama3.2` | 2 GB | ~4 GB | Średni | Domyślny, zalecany |
| `mistral` | 4 GB | ~6 GB | Wolniejszy | Lepsza jakość odpowiedzi |
| `codellama` | 4 GB | ~6 GB | Wolniejszy | Pytania o kod |

---

## Testy

```bash
# Wszystkie testy (wymaga działającego Dockera — TestContainers)
dotnet test

# Szybkie testy bez Dockera (~3 sekundy)
dotnet test --filter "FullyQualifiedName!~ContainerTests"

# Testy tylko jednego projektu
dotnet test PromptApi.Tests
dotnet test WorkerService.Tests
```

20 testów: jednostkowe (xUnit + Moq + SQLite InMemory), integracyjne HTTP (WebApplicationFactory),
kontenerowe (Testcontainers + prawdziwy PostgreSQL).

---

## Rozwiązywanie problemów

### Docker nie odpowiada

```
error: open //./pipe/dockerDesktopLinuxEngine: The system cannot find the file specified
```

Docker Desktop nie jest uruchomiony. Znajdź ikonę wieloryba w zasobniku systemowym
lub uruchom `Docker Desktop` z menu Start. Poczekaj 30–60 sekund.

---

### `docker` nie jest rozpoznawane w terminalu

```powershell
$env:PATH = "C:\Program Files\Docker\Docker\resources\bin;$env:PATH"
```

---

### Prompt utknął na statusie "Oczekuje" lub "Przetwarza"

```bash
# Sprawdź czy Worker jest podłączony
docker compose logs --tail 30 worker-service

# Sprawdź kolejkę w RabbitMQ
# http://localhost:15672 → Queues → prompt-created-consumer

# Restart Workera
docker compose restart worker-service
```

---

### Model AI nie odpowiada, status "Błąd"

```bash
# Sprawdź czy model jest pobrany
docker compose exec ollama ollama list

# Pobierz model jeśli brak
docker compose exec ollama ollama pull llama3.2

# Sprawdź logi Workera — komunikat błędu
docker compose logs --tail 20 worker-service
```

---

### 502 Bad Gateway na http://localhost

Frontend lub API nie skończyły startować. Poczekaj 30–60 sekund po `docker compose up` i odśwież.

```bash
docker compose ps    # sprawdź czy wszystkie serwisy mają status "Up"
```

---

### PromptApi nie startuje po resecie bazy

```bash
# Reset z przebudowaniem — migracje uruchamiają się automatycznie przy starcie
docker compose down -v
docker compose up --build
docker compose exec ollama ollama pull llama3.2
```

---

## Struktura projektu

```
RekJust/
├── docker-compose.yml       # Orkiestracja — uruchamia cały system
├── nginx/
│   └── nginx.conf           # Konfiguracja reverse proxy
├── Contracts/               # Wspólne typy wiadomości kolejkowych (PromptCreated)
├── PromptApi/               # ASP.NET Core — endpointy REST, walidacja, zapis do DB
│   ├── Models/Prompt.cs     # Encja bazy danych
│   ├── Services/            # Logika biznesowa
│   ├── Migrations/          # Migracje EF Core (schemat bazy)
│   └── Dockerfile
├── WorkerService/           # .NET Worker — konsument kolejki, wywołanie LLM
│   ├── Consumers/           # MassTransit Consumer
│   └── Dockerfile
├── frontend/                # Next.js — interfejs użytkownika
│   ├── app/                 # App Router (layout, page)
│   ├── components/          # PromptForm, PromptList
│   └── Dockerfile
├── PromptApi.Tests/         # Testy jednostkowe i integracyjne API
└── WorkerService.Tests/     # Testy jednostkowe i integracyjne Workera
```
