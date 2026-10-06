# High-Level Design (HLD) — Personal Portfolio Website

Reference requirements: [01-Requirement-Analysis.md](01-Requirement-Analysis.md)

## 1. Architecture overview

```
                                   ┌────────────────────────┐
                                   │   GitHub REST API       │
                                   │ (public repos, live)    │
                                   └───────────▲──────────────┘
                                               │ HTTPS (client-side fetch)
┌───────────────┐   static files   ┌───────────┴───────────┐
│ Azure Static    │◄────────────── │  Angular SPA (build)   │
│ Web Apps (Free) │   deploy       │  - VS Code shell UI    │
│                 │                │  - Pages/components    │
│                 │───────────────►│  - Ask AI widget        │
│                 │  serves to    └───────────┬────────────┘
└───────────────┘   browser                    │
                                                 │ HTTPS (fetch)
                                    ┌────────────┴─────────────┐
                                    │  ASP.NET Core Web API      │
                                    │  (Azure App Service, F1)   │
                                    │  - AskAiController          │
                                    │  - ContactController        │
                                    │  - RagService                │
                                    │  - LlmClient (OpenAI/Azure)  │
                                    │  - EmailService               │
                                    └──────┬───────────────┬──────┘
                                           │               │
                              ┌────────────▼───┐   ┌───────▼────────┐
                              │  SQL Server      │   │  LLM Provider   │
                              │  (Azure SQL /    │   │ (Azure OpenAI / │
                              │  local SSMS dev) │   │  OpenAI API)    │
                              │  - ContentChunks │   └─────────────────┘
                              │  - ContactMsgs   │
                              │  - Testimonials  │
                              │  - Experience    │
                              └──────────────────┘
```

## 2. Component breakdown

### 2.1 Frontend — Angular SPA (static)

- **Why Angular**: matches owner's existing expertise; also a genuine differentiator versus
  the React reference site.
- Built as a static site (`ng build --configuration production`) and deployed to a static/edge
  host — **no SSR needed** for v1 (content is not deeply SEO-critical beyond meta tags, and
  SSR adds hosting complexity).
- Talks to two external HTTP APIs only:
  1. GitHub REST API directly (public, no auth needed for public repo listing — subject to
     GitHub's unauthenticated rate limit, acceptable for a low-traffic personal site).
  2. Our own ASP.NET Core Web API, for `ask-ai` and `contact`.
- No secrets of any kind live in this app. Confirmed by the same bundle-inspection technique
  used to audit the reference site (grep the built JS for API keys before every deploy).

### 2.2 Backend — ASP.NET Core Web API

- **Why a custom API instead of n8n** (which the reference site uses): the owner's stack
  strength is .NET, so a minimal API is both cheaper to run than a workflow tool and doubles
  as a portfolio artifact ("I built the AI feature's backend myself").
- Hosting: Azure App Service, F1 (Free) tier — see Docs/06-Deployment-Guide.md for the live
  resource (`portfolio-api-swapnamoy`, East Asia — `eastus`/`eastus2` had zero F1 quota on this
  subscription at creation time).
- Responsibilities:
  - `POST /api/ask-ai` — retrieval-augmented answer generation (see §3.2).
  - `POST /api/contact` — validate, persist, send email notification.
  - `GET /api/health` — simple liveness check for uptime monitoring.
- Cross-cutting: CORS restricted to the portfolio domain, rate limiting (ASP.NET Core's
  built-in `Microsoft.AspNetCore.RateLimiting`), structured logging (Serilog → console /
  Application Insights).

### 2.3 Data — SQL Server

- Managed via SSMS during development; Azure SQL Database (Free tier / Basic) in production.
- EF Core as the ORM/migrations tool from the ASP.NET Core API.
- Tables (detailed schema in LLD): `ContentChunks`, `ExperienceEntries`, `EducationEntries`,
  `Testimonials`, `ContactMessages`.
- No vector database needed at this scale: `ContentChunks` is small (a few dozen paragraphs
  of resume/bio content), so retrieval is done with a lightweight keyword/TF-IDF style match
  or simply sending *all* chunks as context (well within LLM context limits) — this avoids
  standing up a vector store. Documented as a deliberate simplification in §3.2.

### 2.4 External integrations

| Integration | Purpose | Notes |
|---|---|---|
| GitHub REST API | Live project list | Client-side fetch, public/unauthenticated |
| LLM provider (OpenAI or Azure OpenAI) | Ask AI answer generation | Server-side only call, key never leaves backend |
| SMTP / transactional email (e.g., SendGrid free tier, or Azure Communication Services) | Contact form notifications | Server-side only |
| Analytics (e.g., Cloudflare Web Analytics or Plausible) | Page view tracking, no cookies | Client-side, privacy-friendly |

## 3. Key flows

### 3.1 Page navigation flow

1. Browser loads the static Angular bundle from the CDN.
2. Angular Router renders the shell (title bar + tab bar + sidebar) and the initial route
   (Home).
3. Clicking a sidebar/tab item is a client-side route change — no full page reload.
4. Projects route triggers a `GET` to GitHub REST API on activation; other routes render from
   bundled/config data.

### 3.2 Ask AI flow (RAG)

1. User opens the command palette (Ctrl/Cmd+K) and types a question.
2. Frontend `POST`s `{ query }` to `/api/ask-ai`.
3. `AskAiController` calls `RagService.GetAnswer(query)`:
   a. Load all `ContentChunks` rows (small dataset — simple `SELECT *`, cached in memory with
      a short TTL to avoid hitting the DB on every request).
   b. Rank chunks by lexical relevance to the query (simple approach: keyword overlap /
      SQL `CONTAINS` full-text search — no embeddings needed at this content size).
   c. Build a prompt: system instruction ("answer only from the provided context; say you
      don't know otherwise") + top-N relevant chunks + the user's question.
   d. Call the LLM provider's chat/completions endpoint server-side.
   e. Return `{ answer, sources }` (sources = which chunk(s)/section(s) were used).
4. Frontend renders the answer, or a friendly error state if the call fails/times out.
5. Rate limiter caps requests per IP per minute to bound LLM spend.

### 3.3 Contact form flow

1. User fills the form and submits.
2. Frontend `POST`s to `/api/contact`.
3. `ContactController` validates input (server-side, not just client-side), persists a row in
   `ContactMessages`, and calls `EmailService` to notify the owner.
4. Returns 200/4xx to the frontend; frontend shows a success/error toast.

## 4. Hosting & deployment topology

**Finalized** (Docs/06-Deployment-Guide.md has the live resource names/URLs): all-Azure, all
free tier — owner chose to keep the whole stack on one platform rather than split across
Cloudflare + Azure.

| Layer | Choice | Rationale |
|---|---|---|
| Frontend static hosting | Azure Static Web Apps (Free) | All-Azure stack end to end; native GitHub Actions integration generates the deploy workflow; free tier SPA routing handled natively. |
| Backend API hosting | Azure App Service (F1/Free) | .NET-native, matches owner's stack directly. |
| Database | Azure SQL Database, Free limit (serverless, auto-pause on exhaustion) | Matches SSMS/SQL Server experience directly; auto-pause guarantees it never bills even if the monthly free compute is exhausted. |
| CI/CD | GitHub Actions | One workflow per layer, each path-filtered so a UI-only change doesn't redeploy the API and vice versa. |

Note: Cloudflare Pages was the original frontend candidate (see the alternatives this replaced,
kept below for context) — superseded once the owner decided to keep everything on Azure.

## 5. Security model

- Principle: **all secrets server-side only**. LLM API key, SMTP credentials, and the SQL
  connection string live in App Service configuration / environment variables (or Azure Key
  Vault referenced from App Service config) — never in the Angular build output.
- CORS: API only accepts requests from the portfolio's own origin(s).
- Rate limiting: per-IP limits on `ask-ai` and `contact` endpoints.
- Input validation: FluentValidation or DataAnnotations on all DTOs; parameterized
  queries/EF Core (no raw SQL string concatenation) to prevent SQL injection.
- HTTPS enforced end-to-end (hosting platforms provide this by default).

## 6. Alternatives considered

| Decision | Chosen | Alternative | Why chosen wins here |
|---|---|---|---|
| AI backend | Custom ASP.NET Core API | n8n (like the reference site) | Plays to owner's .NET strength; one fewer moving part/service to operate; becomes a portfolio artifact itself |
| Retrieval | Lexical/keyword match over small content table | Vector DB + embeddings | Content volume is tiny (personal bio/resume-scale); a vector store is unjustified complexity/cost at this scale |
| Frontend framework | Angular | React (like reference site) | Matches owner's 3 years of hands-on Angular experience; still achieves the same visual concept |
| Rendering | Client-side rendered SPA | Angular Universal (SSR) | Simpler hosting/cost for v1; SEO need is modest for a personal portfolio; can be revisited later |
