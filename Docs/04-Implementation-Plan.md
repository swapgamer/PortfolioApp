# Step-by-Step Implementation Plan — Personal Portfolio Website

Reference: [01-Requirement-Analysis.md](01-Requirement-Analysis.md),
[02-High-Level-Design.md](02-High-Level-Design.md), [03-Low-Level-Design.md](03-Low-Level-Design.md)

Paced for a solo, part-time developer. Each phase lists concrete tasks and an exit criterion
("done when…") so progress is checkable.

## Phase 0 — Project setup

- [ ] Create GitHub repo (private or public) for the portfolio; init `frontend/` and `backend/`
      folders as in the LLD structure.
- [ ] `ng new portfolio-app --standalone --routing --style=css` inside `frontend/`.
- [ ] Install & configure Tailwind CSS in the Angular app.
- [ ] `dotnet new webapi -n Portfolio.Api` inside `backend/`.
- [ ] Install SQL Server locally (or use existing instance) + open in SSMS; create a
      `PortfolioDb` database.
- [ ] Add EF Core packages to the API project; scaffold `PortfolioDbContext`.
- **Done when**: `ng serve` shows the default Angular page, `dotnet run` serves
  `/swagger`, and SSMS can connect to `PortfolioDb`.

## Phase 1 — VS Code shell UI (no real content yet)

- [ ] Build `ShellComponent`: title bar, tab bar, sidebar Explorer, router outlet.
- [ ] Define the shared `NAV_ITEMS` list and wire both tab bar and sidebar to it.
- [ ] Set up Tailwind theme tokens matching a VS Code dark color scheme (background `#1e1e1e`,
      panel `#252526`, border `#2d2d2d`, accent color of choice).
- [ ] Add placeholder routed components for all 8 pages (empty divs with the page name).
- [ ] Mobile breakpoint pass: sidebar collapses, tab bar scrolls.
- **Done when**: clicking every tab/sidebar item navigates correctly, active state highlights
  correctly, and the layout doesn't break at 375px width.

## Phase 2 — Static content pages

- [ ] Write real copy for Home, About sections.
- [ ] Create `content/experience.json`, `content/education.json`, `content/testimonials.json`.
- [ ] Build `ExperienceComponent`/`EducationComponent` as timeline UIs consuming that JSON.
- [ ] Build `TestimonialsComponent` as a card grid/carousel.
- **Done when**: all pages except Projects, Contact, Ask AI show real, final content and look
  correct on desktop + mobile.

## Phase 3 — Live Projects feed

- [ ] Build `GithubService.getRepos(username)` calling
      `https://api.github.com/users/{username}/repos?sort=updated&per_page=10`.
- [ ] Build `ProjectsComponent`: cards with repo name, description, stars, language, link.
- [ ] Handle loading and error states (GitHub API rate-limited/unreachable).
- **Done when**: Projects page reliably reflects the owner's actual public GitHub repos.

## Phase 4 — Backend API skeleton + Contact form

- [ ] Implement `ContactMessage` entity + EF Core migration; apply it (`dotnet ef database
      update`) and confirm the table exists via SSMS.
- [ ] Implement `ContactController` + `ContactRequest` validator (FluentValidation or
      DataAnnotations).
- [ ] Implement `EmailService` (start with a simple provider — e.g., SendGrid free tier, or
      SMTP via a transactional email service); store the API key in User Secrets locally.
- [ ] Configure CORS to allow `http://localhost:4200`.
- [ ] Build `ContactComponent` form in Angular + `ContactService.submit(...)`.
- **Done when**: submitting the form from the running Angular app creates a row in
  `ContactMessages` (verified in SSMS) and the owner receives a real email.

## Phase 5 — Ask AI (RAG feature)

- [ ] Implement `ContentChunk` entity + migration; seed it with real bio/experience/skills text
      (write a small seed script or insert via SSMS).
- [ ] Implement `RankByKeywordOverlap` + `PromptBuilder` (pure functions, unit-testable first).
- [ ] Implement `ILlmClient`/`OpenAiClient` calling the chosen provider's chat completion
      endpoint; store the API key in User Secrets locally, never in source control.
- [ ] Implement `AskAiController` + `RagService` wiring the above together, with the in-memory
      cache for chunks.
- [ ] Add the fixed-window rate limiter for `ask-ai`.
- [ ] Write xUnit tests for the ranking function and prompt builder (deterministic, no live
      LLM calls in tests — mock `ILlmClient`).
- [ ] Build `AskAiComponent` (Ctrl/Cmd+K modal) + `AskAiService` in Angular.
- **Done when**: asking a real question from the running app returns a correct, source-cited
  answer, and asking an out-of-scope question gets a graceful "I don't know" instead of a
  hallucinated answer.

## Phase 6 — Security & hardening pass

- [ ] Build the Angular app for production and **grep the output bundle** for the LLM key,
      SMTP credentials, and connection string to confirm none leaked client-side (same check
      used to audit the reference site).
- [ ] Confirm CORS rejects requests from an arbitrary third-party origin.
- [ ] Confirm rate limiting actually returns 429 after the configured threshold (manual test
      or a small script).
- [ ] Add global exception-handling middleware so unhandled errors never return raw stack
      traces.
- [ ] Add input length/format validation on every DTO.
- **Done when**: a basic security checklist (secrets, CORS, rate limits, validation, HTTPS) is
  fully green.

## Phase 7 — Analytics, SEO, accessibility polish

- [ ] Add a privacy-friendly analytics snippet (Cloudflare Web Analytics or similar).
- [ ] Add meta tags (title, description, Open Graph image) per route via Angular's `Meta`/
      `Title` services.
- [ ] Add `robots.txt` and `sitemap.xml`.
- [ ] Run Lighthouse; fix anything below ~90 on Performance/Accessibility/SEO.
- [ ] Keyboard-navigation pass: tab through the whole shell, confirm the Ask AI dialog traps
      focus and closes on `Escape`.
- **Done when**: Lighthouse scores meet the NFR targets and the site is fully keyboard-usable.

## Phase 8 — Deployment & CI/CD

- [ ] Provision Azure SQL Database (or chosen prod DB); apply EF Core migrations against it.
- [ ] Provision backend hosting (Azure App Service or Container Apps); set all secrets as
      environment/application configuration (or Key Vault references) — not in `appsettings.json`.
- [ ] Provision frontend static hosting (Cloudflare Pages or Azure Static Web Apps); point the
      custom domain at it.
- [ ] Update Angular `environment.prod.ts` `apiBaseUrl` to the deployed API URL; update backend
      CORS policy to the real production domain.
- [ ] Set up GitHub Actions: one workflow to build+deploy the Angular app on push to `main`,
      one to build+deploy the API.
- **Done when**: the custom domain serves the live site end-to-end (nav, projects, contact
  form, Ask AI) with no console errors and no secrets in the client bundle.

## Phase 9 — Launch checklist

- [ ] Cross-browser check (Chrome, Firefox, Safari, mobile Safari/Chrome).
- [ ] Re-run the full security checklist from Phase 6 against the **production** build.
- [ ] Verify contact form email actually arrives in production.
- [ ] Verify Ask AI works in production (LLM key valid in prod config, rate limiting active).
- [ ] Update GitHub repo README with a short write-up of the architecture — this documentation
      set becomes portfolio content itself (link it from the About/Projects page).
- **Done when**: all boxes checked and the link is ready to share with recruiters/on LinkedIn.

## Suggested pacing (part-time, evenings/weekends)

| Phase | Estimate |
|---|---|
| 0 – Setup | 0.5 day |
| 1 – Shell UI | 2–3 days |
| 2 – Static content pages | 2 days |
| 3 – Projects feed | 1 day |
| 4 – Contact form + backend skeleton | 2 days |
| 5 – Ask AI (RAG) | 3–4 days |
| 6 – Security hardening | 1 day |
| 7 – Analytics/SEO/a11y | 1–2 days |
| 8 – Deployment/CI-CD | 2 days |
| 9 – Launch checklist | 0.5 day |
| **Total** | **~3–3.5 weeks part-time** |

## Open decisions to confirm before Phase 0

1. LLM provider: OpenAI API vs Azure OpenAI (affects `ILlmClient` implementation and billing
   setup).
2. Frontend hosting: Cloudflare Pages vs Azure Static Web Apps.
3. Backend hosting: Azure App Service vs Azure Container Apps.
4. Blog: external embed vs minimal in-app list (affects whether Phase 2 includes a
   `BlogController`).
