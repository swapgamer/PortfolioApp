# Requirement Analysis — Personal Portfolio Website ("VS Code" theme)

## 1. Background

Inspiration: [mak-thevar.dev](https://mak-thevar.dev) — a personal portfolio styled as a VS
Code editor window (activity bar, tab bar, file explorer, "Ask AI" command palette-style
assistant). We are building an equivalent, original portfolio for the owner of this repo,
reusing the *concept* (not the code/content), tailored to their own stack strengths.

## 2. Owner profile & constraints

- 3 years' professional experience: **.NET / C#, SQL Server (SSMS), Angular**
- Solo developer, part-time build (evenings/weekends)
- Budget-conscious — prefer free/low-cost hosting tiers
- Wants the AI assistant feature, but security-conscious (no client-exposed LLM keys)
- Wants to lean on Angular + .NET rather than learn React, to showcase existing strengths
  to recruiters/clients

## 3. Goals

1. A fast, visually distinctive personal portfolio that stands out from generic templates.
2. Demonstrate real engineering skill (not just content) — the site itself is a portfolio
   piece: clean architecture, a working API, a small RAG-based AI feature.
3. Make content easy to update without redeploying code (config/DB-driven where sensible).
4. Keep running costs near zero.

## 4. Functional requirements

### 4.1 Frontend (Angular SPA)

| ID | Requirement |
|----|-------------|
| FR-1 | VS Code–like shell: title bar, top tab bar acting as page navigation, left sidebar "Explorer" listing the same pages as files, bottom status bar (optional). |
| FR-2 | Pages/sections, each a tab: Home, About, Experience, Education, Projects, Testimonials, Contact, Blog. |
| FR-3 | Home page: intro/hero, "Know Me" and "View Work" CTAs, a "What I Bring to the Table" highlights grid. |
| FR-4 | Projects page: pulled **live** from GitHub REST API (`/users/{user}/repos`), sorted by stars/updated, no manual duplication of repo metadata. |
| FR-5 | Experience & Education pages: structured, config- or DB-driven (JSON/DB), rendered as timeline components. |
| FR-6 | Testimonials: static or DB-driven list of quotes/attributions. |
| FR-7 | Blog: either an external link (e.g., dev.to/Hashnode/Medium) embedded as a tab, or a minimal in-app list backed by the API — decide in HLD. |
| FR-8 | Contact page: a form (name, email, message) that submits to the backend API and sends an email/notification — no exposed SMTP/email-provider keys in the client. |
| FR-9 | "Ask AI" command-palette style widget (Ctrl/Cmd+K): free-text question box, answers constrained to the site's own content (RAG), shows "Thinking…" state and graceful error state. |
| FR-10 | Responsive layout: usable on mobile (sidebar collapses, tabs become scrollable/hamburger). |
| FR-11 | Dark theme matching a VS Code color scheme (e.g., Dark+ / One Dark) as the default and only theme for v1. |

### 4.2 Backend (ASP.NET Core Web API)

| ID | Requirement |
|----|-------------|
| FR-12 | `POST /api/ask-ai` — accepts `{ query }`, returns `{ answer, sources }`. Performs retrieval over stored site-content chunks, then calls an LLM with the retrieved context and the user's question. |
| FR-13 | `POST /api/contact` — accepts `{ name, email, message }`, validates, persists, and sends a notification email. |
| FR-14 | Content is stored in SQL Server (via SSMS-managed schema) so it can be updated without a redeploy of the frontend. |
| FR-15 | Rate limiting on `/api/ask-ai` and `/api/contact` to control LLM cost and prevent spam. |
| FR-16 | CORS locked to the portfolio's own domain(s). |
| FR-17 | Secrets (LLM API key, SMTP credentials, connection strings) live only in server-side configuration (User Secrets locally, environment variables / Key Vault in production) — never shipped to the client bundle. |

## 5. Non-functional requirements

| ID | Requirement |
|----|-------------|
| NFR-1 | **Performance**: first contentful paint < 2s on a typical broadband connection; frontend is a static build served from a CDN/edge host. |
| NFR-2 | **Security**: no secrets in client JS (verified by inspecting the built bundle, same technique used to audit the reference site); input validation & sanitization on all API endpoints; HTTPS only. |
| NFR-3 | **Cost**: target $0–~$5/month running cost (free static hosting + free/cheap API hosting tier + pay-as-you-go LLM calls kept low via rate limiting and small context). |
| NFR-4 | **Accessibility**: keyboard-navigable tabs, sufficient color contrast despite dark theme, ARIA roles on the command-palette dialog. |
| NFR-5 | **Maintainability**: content changes (experience, projects blurbs, testimonials) should not require a frontend code change/redeploy where feasible. |
| NFR-6 | **Observability**: basic analytics (page views) and server-side logging of API errors. |
| NFR-7 | **SEO**: meta tags, Open Graph tags, sitemap — since it's a public portfolio meant to be found by recruiters. |

## 6. Out of scope (v1)

- User authentication / admin login UI for editing content (content updates via direct DB edit or a seed script for v1).
- Multi-language support.
- CMS integration.
- Full blog engine (rich editor, comments) — a simple list + external link is enough for v1.
- Mobile app.

## 7. Success criteria

- Site is live on a custom domain, fully navigable, matches the VS Code concept visually.
- Ask AI answers correctly from real site content and refuses/deflects gracefully on
  out-of-scope questions.
- Lighthouse performance/accessibility/SEO scores ≥ 90.
- No secrets discoverable in browser devtools or the JS bundle.
- Owner can update Experience/Projects/Testimonials content without touching frontend code.

## 8. Assumptions

- Owner already owns or will buy a custom domain.
- GitHub profile has enough public repos to make the live Projects feed meaningful.
- LLM provider: Azure OpenAI or OpenAI API (final choice made in HLD) — owner will provision
  an API key.
