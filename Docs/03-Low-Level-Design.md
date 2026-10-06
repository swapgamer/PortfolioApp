# Low-Level Design (LLD) — Personal Portfolio Website

Reference: [01-Requirement-Analysis.md](01-Requirement-Analysis.md),
[02-High-Level-Design.md](02-High-Level-Design.md)

## 1. Repository / solution structure

```
Portfolio/
├── Docs/                          # this documentation
├── frontend/                      # Angular workspace
│   └── portfolio-app/
│       ├── src/app/
│       │   ├── shell/             # title bar, tab bar, sidebar, status bar
│       │   ├── pages/
│       │   │   ├── home/
│       │   │   ├── about/
│       │   │   ├── experience/
│       │   │   ├── education/
│       │   │   ├── projects/
│       │   │   ├── testimonials/
│       │   │   ├── contact/
│       │   │   └── blog/
│       │   ├── ask-ai/            # command palette widget (standalone component)
│       │   ├── core/
│       │   │   ├── services/      # github.service.ts, ask-ai.service.ts, contact.service.ts
│       │   │   └── models/        # TS interfaces/DTOs
│       │   ├── theme/             # tailwind + VS Code color tokens
│       │   └── app.routes.ts
│       ├── tailwind.config.js
│       └── angular.json
├── backend/                       # ASP.NET Core solution
│   └── Portfolio.Api/
│       ├── Controllers/
│       │   ├── AskAiController.cs
│       │   ├── ContactController.cs
│       │   └── HealthController.cs
│       ├── Services/
│       │   ├── IRagService.cs / RagService.cs
│       │   ├── ILlmClient.cs / OpenAiClient.cs
│       │   └── IEmailService.cs / EmailService.cs
│       ├── Data/
│       │   ├── PortfolioDbContext.cs
│       │   ├── Entities/
│       │   └── Migrations/
│       ├── Dtos/
│       ├── Middleware/            # rate limiting, error handling
│       ├── appsettings.json
│       └── Program.cs
└── Images/                        # existing reference screenshots
```

## 2. Frontend — component & service detail

### 2.1 Shell (VS Code chrome)

- `ShellComponent` (root layout): renders `TitleBarComponent`, `TabBarComponent`,
  `SidebarComponent` (Explorer tree), `<router-outlet>`, optional `StatusBarComponent`.
- `TabBarComponent` and `SidebarComponent` both derive their item list from a single shared
  `NAV_ITEMS` constant (`{ path, label, icon }[]`) so the two stay in sync automatically —
  avoids the duplication bug class of "tab says X, sidebar says Y".
- Active tab/sidebar highlighting driven by Angular Router's `routerLinkActive`.
- Mobile breakpoint (Tailwind `sm:`): sidebar collapses behind a hamburger toggle; tab bar
  becomes horizontally scrollable.

### 2.2 Routing table

| Path | Component | Data source |
|---|---|---|
| `/` | HomeComponent | static/config |
| `/about` | AboutComponent | static/config |
| `/experience` | ExperienceComponent | `GET /api/experience` or bundled JSON (see §2.4) |
| `/education` | EducationComponent | bundled JSON |
| `/projects` | ProjectsComponent | GitHub REST API (client-side) |
| `/testimonials` | TestimonialsComponent | `GET /api/testimonials` or bundled JSON |
| `/contact` | ContactComponent | `POST /api/contact` |
| `/blog` | BlogComponent | external link tab, or `GET /api/blog-posts` |

All routes are lazy-loaded standalone components (`loadComponent`) to keep the initial bundle
small.

### 2.3 Ask AI widget

- `AskAiComponent` (standalone, injected once in `ShellComponent`):
  - Global `keydown` listener for `Ctrl/Cmd+K` to toggle a modal dialog (`role="dialog"`,
    `aria-modal="true"`, focus-trapped).
  - State (Angular signals): `open`, `query`, `loading`, `answer`, `sources`, `error`.
  - On submit: calls `AskAiService.ask(query)`.
- `AskAiService`:
  ```ts
  ask(query: string): Observable<{ answer: string; sources: string[] }> {
    return this.http.post<AskAiResponse>(`${environment.apiBaseUrl}/api/ask-ai`, { query });
  }
  ```
  `environment.apiBaseUrl` is the only "configuration" the frontend needs — a plain base URL,
  not a secret.

### 2.4 Content sourcing decision

- **Experience / Education / Testimonials**: for v1, ship as versioned JSON files inside
  `core/assets/content/*.json`, imported directly by the components. This satisfies NFR-5
  ("update without full code change") loosely — content changes are a JSON edit + redeploy,
  which is acceptable for a personal portfolio update cadence (infrequent).
  - *Upgrade path (v1.1)*: move these to DB-backed `GET` endpoints once the owner wants to
    edit content without redeploying at all — the API/DB schema for this already exists for
    Ask AI's `ContentChunks`, so it's a small extension, not a redesign.
- **Projects**: always live from GitHub API — never duplicated in the DB.
- **Ask AI content chunks**: DB-backed from day one, because the AI feature is the whole point
  of exercising the backend/DB skills.

## 3. Backend — detail

### 3.0 Data access pattern: direct `DbContext`, not Repository pattern

**Decision**: controllers/services (`RagService`, `ContactController`, etc.) inject
`ApplicationDbContext` directly and query/write through its `DbSet<T>` properties. There is
no `IContentChunkRepository`/`IRepository<T>` abstraction layer in between.

**Why**: `DbContext` + `DbSet<T>` is already an implementation of the Repository and Unit of
Work patterns — each `DbSet<T>` behaves as a per-entity repository, and `SaveChangesAsync()`
is the unit of work. A hand-rolled repository interface on top would, for this project, mostly
forward calls 1:1 to the same `DbSet`, adding a layer of indirection without adding real
behavior.

A repository layer earns its cost when at least one of these is actually true:
- The persistence technology might be swapped out later (not the case — SQL Server is a fixed
  requirement here, not a placeholder).
- Tests need to mock data access without hitting a real database — EF Core's `InMemory` or
  SQLite-in-memory providers already cover this adequately at this project's scale (see §7).
- A strict Clean/Onion Architecture boundary requires the domain layer to have zero knowledge
  that EF Core exists.

None of those apply to a 5-entity, solo-maintained portfolio API, so the extra abstraction was
deliberately skipped rather than added by default/habit.

**Trade-off, honestly stated**: this is a genuine 50/50 debate in .NET projects, and skipping
the pattern does cost something:
- Swapping ORMs later (unlikely, but not impossible) would mean touching every service that
  calls `DbContext` directly, instead of just swapping one repository implementation.
- Unit tests for services are coupled to EF Core's testing surface (`InMemory`/SQLite) rather
  than a plain mockable interface.
- It's a common interview question ("why no repository pattern?") — answered here explicitly
  so the choice reads as a reasoned trade-off, not an oversight.

If a future phase needs it (e.g., demonstrating repository pattern specifically for a
different interview/skill-building goal), it can be introduced incrementally per entity
without a full rewrite, since callers only ever go through `ApplicationDbContext` today.

### 3.1 `AskAiController`

```csharp
[ApiController]
[Route("api/ask-ai")]
[EnableRateLimiting("ask-ai")]
public class AskAiController : ControllerBase
{
    private readonly IRagService _ragService;

    [HttpPost]
    public async Task<ActionResult<AskAiResponse>> Ask([FromBody] AskAiRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length > 500)
            return BadRequest("Query must be 1-500 characters.");

        var result = await _ragService.GetAnswerAsync(request.Query, ct);
        return Ok(result);
    }
}

public record AskAiRequest(string Query);
public record AskAiResponse(string Answer, IReadOnlyList<string> Sources);
```

### 3.2 `RagService`

```csharp
public class RagService : IRagService
{
    private readonly PortfolioDbContext _db;
    private readonly ILlmClient _llm;
    private readonly IMemoryCache _cache;

    public async Task<AskAiResponse> GetAnswerAsync(string query, CancellationToken ct)
    {
        var chunks = await _cache.GetOrCreateAsync("content-chunks", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _db.ContentChunks.AsNoTracking().ToListAsync(ct);
        });

        var top = RankByKeywordOverlap(chunks!, query, take: 5);

        var prompt = PromptBuilder.Build(query, top);
        var completion = await _llm.CompleteAsync(prompt, ct);

        return new AskAiResponse(completion, top.Select(c => c.Section).Distinct().ToList());
    }
}
```

- `RankByKeywordOverlap`: tokenize query + chunk text (lowercase, strip punctuation, **stop
  words removed**), score by overlapping token count; ties broken by chunk recency/order.
  Deliberately simple — no ML dependency — matches the "no vector DB needed at this scale" HLD
  decision. Stop-word filtering (`a`, `the`, `is`, `on`, …) is not optional: without it, common
  words match almost any English sentence and a genuinely off-topic question never scores 0,
  so the no-match fallback below would never trigger.
- `PromptBuilder.Build`: fixed system instruction + numbered context chunks + user question;
  instructs the model to answer **only** from context and say "I don't have that information"
  otherwise (mirrors the reference site's "answers come only from site excerpts" constraint).
- `ILlmClient` is an interface so the concrete provider (OpenAI vs Azure OpenAI) is swappable
  via DI without touching `RagService`.
- **Local fallback when the LLM is unavailable**: `GetAnswerAsync` wraps the `_llm.CompleteAsync`
  call in a try/catch for `LlmUnavailableException`. On failure (no API key configured yet, or
  the provider is down), it returns the top-ranked chunk's raw `Text` as the answer instead of
  failing the request — or a fixed "I don't have specific information about that…" message with
  empty `sources` when the best match scored 0 (genuinely off-topic question). This means the
  feature is fully usable end-to-end before an LLM key is ever configured, and degrades
  gracefully rather than erroring if OpenAI has an outage later — no feature flag needed, since
  the real LLM path is simply tried first every time.

### 3.3 `ContactController`

```csharp
[ApiController]
[Route("api/contact")]
[EnableRateLimiting("contact")]
public class ContactController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] ContactRequest request, CancellationToken ct)
    {
        // FluentValidation: Name required, Email valid format, Message 10-2000 chars
        var entity = new ContactMessage { Name = request.Name, Email = request.Email, Message = request.Message, CreatedUtc = DateTime.UtcNow };
        _db.ContactMessages.Add(entity);
        await _db.SaveChangesAsync(ct);
        await _emailService.NotifyOwnerAsync(entity, ct);
        return Ok();
    }
}
```

### 3.4 Rate limiting policies (`Program.cs`)

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("ask-ai", o => { o.PermitLimit = 10; o.Window = TimeSpan.FromMinutes(1); });
    options.AddFixedWindowLimiter("contact", o => { o.PermitLimit = 5; o.Window = TimeSpan.FromMinutes(1); });
});
```

### 3.5 CORS

```csharp
builder.Services.AddCors(o => o.AddPolicy("portfolio", p =>
    p.WithOrigins("https://<your-domain>", "http://localhost:4200")
     .AllowAnyHeader()
     .WithMethods("GET", "POST")));
```

## 4. Database schema (SQL Server, EF Core migrations)

```sql
CREATE TABLE ContentChunks (
    Id            INT IDENTITY PRIMARY KEY,
    Section       NVARCHAR(100) NOT NULL,   -- e.g. 'About', 'Experience:Acme Corp', 'Skills'
    Text          NVARCHAR(MAX) NOT NULL,
    DisplayOrder  INT NOT NULL DEFAULT 0,
    UpdatedUtc    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE ExperienceEntries (
    Id            INT IDENTITY PRIMARY KEY,
    Company       NVARCHAR(200) NOT NULL,
    Role          NVARCHAR(200) NOT NULL,
    StartDate     DATE NOT NULL,
    EndDate       DATE NULL,                -- NULL = current
    Description   NVARCHAR(MAX) NOT NULL,
    DisplayOrder  INT NOT NULL DEFAULT 0
);

CREATE TABLE EducationEntries (
    Id            INT IDENTITY PRIMARY KEY,
    Institution   NVARCHAR(200) NOT NULL,
    Degree        NVARCHAR(200) NOT NULL,
    StartDate     DATE NOT NULL,
    EndDate       DATE NULL,
    DisplayOrder  INT NOT NULL DEFAULT 0
);

CREATE TABLE Testimonials (
    Id            INT IDENTITY PRIMARY KEY,
    AuthorName    NVARCHAR(200) NOT NULL,
    AuthorTitle   NVARCHAR(200) NULL,
    Quote         NVARCHAR(1000) NOT NULL,
    DisplayOrder  INT NOT NULL DEFAULT 0
);

CREATE TABLE ContactMessages (
    Id            INT IDENTITY PRIMARY KEY,
    Name          NVARCHAR(200) NOT NULL,
    Email         NVARCHAR(320) NOT NULL,
    Message       NVARCHAR(2000) NOT NULL,
    CreatedUtc    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Handled       BIT NOT NULL DEFAULT 0
);
```

Notes:
- `ContentChunks.Section` doubles as the "source" label returned to the frontend so the Ask AI
  answer can show *which part of the site* it drew from (parity with the reference site's
  `sources` field).
- No embeddings/vector column in v1, per the HLD decision. If content grows enough to need
  semantic search later, add a `VECTOR`/external vector store then — not before.
- Full-text index on `ContentChunks.Text` (`CREATE FULLTEXT INDEX`) is an optional upgrade to
  replace the in-memory keyword ranking with `CONTAINS`/`FREETEXT` SQL queries if the chunk
  count grows large.

## 5. API contracts (frontend ⇄ backend)

### `POST /api/ask-ai`

Request:
```json
{ "query": "What Azure skills does he have?" }
```
Response 200:
```json
{ "answer": "...", "sources": ["Experience:Acme Corp", "Skills"] }
```
Response 400: validation error (empty/too-long query).
Response 429: rate limit exceeded.
Response 502/504: LLM provider failure/timeout — frontend shows a friendly retry message.

### `POST /api/contact`

Request:
```json
{ "name": "Jane Doe", "email": "jane@example.com", "message": "Loved your portfolio!" }
```
Response 200: empty body.
Response 400: validation errors, field-level.
Response 429: rate limit exceeded.

## 6. Error handling & resilience

- Global exception middleware in the API returns a consistent `{ error: string }` JSON shape
  and logs the real exception server-side (never leaks stack traces to the client).
- `ILlmClient.CompleteAsync` wrapped with a timeout (e.g., 15s) + single retry on transient
  failure (Polly), then surfaces a typed exception the controller maps to 502/504.
- Frontend `AskAiComponent` and `ContactComponent` both handle HTTP error responses with a
  user-visible inline message — never a silent failure or unhandled console error.

## 7. Testing strategy (detail)

| Layer | Tooling | Coverage target |
|---|---|---|
| Angular components | Jasmine/Karma or Vitest | Shell nav, Ask AI widget open/submit/error states, Contact form validation |
| Angular services | Jasmine + `HttpClientTestingModule` | GithubService, AskAiService, ContactService — success + error paths |
| .NET unit tests | xUnit | RagService ranking logic, PromptBuilder output, validators |
| .NET integration tests | xUnit + `WebApplicationFactory` + SQLite/in-memory or test SQL DB | Controllers end-to-end incl. rate limiting behavior |
| E2E | Playwright | Full nav flow, Ask AI happy path (mocked API), Contact form submit |
