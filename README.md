<div align="center">
  <h1>
    <font style="font-weight: bold;">InLap</font>
    <font size="5"> - AI Motorsport / Sim Racing Journalist</font>
  </h1>

  A full-stack system that transforms raw sim racing weekend data into a deterministic, fact-only weekend summary and a polished journalist-style article.

  <p>
    <img alt="Backend" src="https://img.shields.io/badge/Backend-.NET%208-blueviolet.svg?style=for-the-badge&logo=dotnet"/>
    <img alt="Frontend" src="https://img.shields.io/badge/Frontend-Angular-DD0031.svg?style=for-the-badge&logo=angular"/>
    <img alt="AI" src="https://img.shields.io/badge/AI-OpenAI%20Compatible-4CAF50.svg?style=for-the-badge&logo=openai"/>
    <img alt="Packaging" src="https://img.shields.io/badge/Packaging-Docker-2496ED.svg?style=for-the-badge&logo=docker"/>
  </p>

</div>

---

## About InLap

Upload a CSV exported from `simresults.net` and receive:
- A structured “weekend summary” JSON.
- A clean, journalist-style article suitable for Discord and websites.
- A minimal web UI to upload and read the generated report.

InLap is ideal for league admins, moderators, and race directors who want reliable, on-brand weekend recaps—without spending hours writing.

---

## Highlights

- **Deterministic, fact-only summary** from CSV (no hallucinations).
- **Journalistic article generation** with an OpenAI-compatible LLM.
- **Minimal, focused API** for uploads and fetching reports.
- **Angular client** with modern UX: upload, progress, side summary, results tables, copy-to-clipboard.

---

## End-to-End Flow

1. **Upload**: A CSV file exported from simresults.net is uploaded via the web UI.
2. **Parse**: The backend parses the file into a domain `RaceWeekend` object, including all sessions and metadata.
3. **Summarize**: `SummaryComposer.Compose()` produces a deterministic `WeekendSummaryDto` with sessions and finishers in a consistent order.
4. **Generate**: The summary is serialized and sent to an OpenAI-compatible model with a strict, facts-only system prompt.
5. **Persist**: The LLM's response is cleaned, validated, and saved alongside the deterministic summary.
6. **Render**: The client polls for the report status and, upon completion, renders the full article, a summary card, and detailed race results tables.

---

## System Architecture

The project is organized into a clean, decoupled backend and a modern frontend application.

### Repository Structure
```
backend/
  InLap.Api/              # ASP.NET Core minimal Web API
  InLap.App/              # Domain, DTOs, summary composition, use cases
  InLap.Infrastructure/   # File storage, LLM client, configuration
  InLap.sln               # Solution
client/
  in-lap/                 # Angular app (standalone components)
samples/                  # Sample weekend CSVs
.env.example              # Environment template
docker-compose.yml        # Optional local orchestration
```

### Key Backend Files:
- `backend/InLap.App/UseCases/ProcessUploadUseCase.cs` – Orchestrates storage, parsing, summary composition, LLM completion, and persistence.
- `backend/InLap.App/Summary/SummaryComposer.cs` – Converts the domain weekend into a `WeekendSummaryDto` with deterministic ordering and sorted finishers.

---

## Tech Stack

| Component      | Technology                                    |
|----------------|-----------------------------------------------|
| **Backend**    | C#, .NET 8, ASP.NET Core Web API              |
| **Frontend**   | Angular (standalone components), TypeScript, SCSS |
| **AI**         | OpenAI-compatible LLM Client                  |
| **Packaging**  | Docker (optional)                             |

---

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- Angular CLI (optional but recommended)
- Optional: Docker Desktop
- An OpenAI-compatible API endpoint + key (e.g., OpenAI, Azure OpenAI, local compatible server)

### 1. Configure Environment

Copy `.env.example` to `.env` and fill in your LLM provider details:

```
OPENAI_API_KEY=sk-...
OPENAI_BASE_URL=https://api.openai.com/v1
OPENAI_MODEL=gpt-4o-mini
```

### 2. Run Backend (Local)

From the `backend/` directory:

```bash
dotnet build InLap.sln
dotnet run --project InLap.Api/InLap.Api.csproj
```
The API will start on a local Kestrel port. Ensure your client's CORS origin (e.g., http://localhost:4200) is allowed.

### 3. Run Client (Local)

From the `client/in-lap/` directory:

```bash
npm install
ng serve
```
Visit `http://localhost:4200` and ensure the environment files in `src/environments/` point to your correct API base URL.

---

## License

This project is licensed under the Apache 2.0 License — see the `LICENSE` file for details.
