# Deployment Guide — Personal Portfolio Website

Reference: [02-High-Level-Design.md](02-High-Level-Design.md) section 4,
[04-Implementation-Plan.md](04-Implementation-Plan.md) Phase 8.

## 0. What's already done vs what you need to do

| Done (this session) | You need to do (requires your own account access) |
|---|---|
| Local git repo initialized, `.gitignore` excludes `Draft/`, `reports/`, `research_notes/` | Create the GitHub repo and push |
| `UI/public/_redirects` — SPA routing fallback for Cloudflare Pages | Create the Cloudflare Pages project, connect it to the repo |
| App auto-applies EF Core migrations on startup (`Program.cs`) — no separate migration step needed | Provision Azure SQL Database |
| CORS origins now configurable (`Cors:AllowedOrigins`), not hardcoded | Provision Azure App Service, configure its app settings |
| `.github/workflows/deploy-api.yml` — builds + deploys the API on push to `main` | Add the two GitHub secrets the workflow needs |
| | Point `environment.prod.ts` at the real API URL once it exists |
| | Buy/point a custom domain (optional for a first deploy) |

Everything in the right column needs your login (Azure, Cloudflare, GitHub) — I can't do those
steps for you. The commands below are exact copy-paste blocks so there's no guessing.

## 1. Push to GitHub

```bash
# from C:\Users\Admin\Desktop\Portfolio
gh repo create portfolio --private --source=. --remote=origin
git push -u origin master
```

If you don't have `gh` (GitHub CLI) installed, create the repo manually at
github.com/new instead, then:

```bash
git remote add origin https://github.com/<your-username>/portfolio.git
git push -u origin master
```

## 2. Azure SQL Database

```bash
az login

az group create --name portfolio-rg --location eastus

az sql server create \
  --name portfolio-sql-<unique-suffix> \
  --resource-group portfolio-rg \
  --location eastus \
  --admin-user portfolioadmin \
  --admin-password "<choose-a-strong-password>"

# Allow Azure services (including your App Service) to reach this server
az sql server firewall-rule create \
  --resource-group portfolio-rg \
  --server portfolio-sql-<unique-suffix> \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

az sql db create \
  --resource-group portfolio-rg \
  --server portfolio-sql-<unique-suffix> \
  --name PortfolioDb \
  --service-objective Basic
```

Note the connection string shape -- you'll need it in step 3:

```
Server=tcp:portfolio-sql-<unique-suffix>.database.windows.net,1433;Initial Catalog=PortfolioDb;Persist Security Info=False;User ID=portfolioadmin;Password=<your-password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## 3. Azure App Service (backend API)

```bash
az appservice plan create \
  --name portfolio-plan \
  --resource-group portfolio-rg \
  --sku F1 \
  --is-linux

az webapp create \
  --name portfolio-api-<unique-suffix> \
  --resource-group portfolio-rg \
  --plan portfolio-plan \
  --runtime "DOTNETCORE:8.0"
```

Set the app settings it needs (replace every placeholder, including the OpenAI key you were
holding off on until now -- this is the right moment to add it):

```bash
az webapp config appsettings set \
  --name portfolio-api-<unique-suffix> \
  --resource-group portfolio-rg \
  --settings \
    ConnectionStrings__DefaultConnection="<the connection string from step 2>" \
    OpenAI__ApiKey="<your real OpenAI API key>" \
    Cors__AllowedOrigins__0="https://<your-cloudflare-pages-url-or-custom-domain>"
```

Download the publish profile and add it to GitHub as a secret:

```bash
az webapp deployment list-publishing-profiles \
  --name portfolio-api-<unique-suffix> \
  --resource-group portfolio-rg \
  --xml
```

Copy the full XML output, then in GitHub: **Settings → Secrets and variables → Actions → New
repository secret**:

- `AZURE_WEBAPP_PUBLISH_PROFILE` = the XML you just copied
- `AZURE_WEBAPP_NAME` = `portfolio-api-<unique-suffix>`

Push to `main` (or re-run the workflow manually) and `.github/workflows/deploy-api.yml` will
build, publish, and deploy the API automatically from here on. The app migrates its own schema
on startup, so the database will have all 5 tables after the first successful deploy -- verify
with the same `sqlcmd`-style check we used locally, pointed at the Azure SQL server instead.

## 4. Cloudflare Pages (frontend)

Cloudflare Pages' native Git integration replaces what would otherwise be a second GitHub
Actions workflow -- simpler, and it's the standard way to use Pages, so no custom YAML is
needed here.

1. In the Cloudflare dashboard: **Workers & Pages → Create → Pages → Connect to Git** → select
   the `portfolio` repo.
2. Build settings:
   - **Framework preset**: Angular
   - **Build command**: `cd UI && npm ci && npm run build`
   - **Build output directory**: `UI/dist/portfolio-app/browser`
3. Deploy. Cloudflare will build and deploy on every push to `main` from here on.

## 5. Wire the frontend to the real backend URL

Once step 3 gives you the API's real URL (`https://portfolio-api-<unique-suffix>.azurewebsites.net`),
update it before the next Cloudflare build:

```ts
// UI/src/environments/environment.prod.ts
export const environment = {
  production: true,
  apiBaseUrl: 'https://portfolio-api-<unique-suffix>.azurewebsites.net'
};
```

Commit and push -- Cloudflare rebuilds automatically, and the production bundle will now call
the real API instead of the `TODO` placeholder.

## 6. Custom domain (optional for a first deploy)

- **Frontend**: Cloudflare Pages → your project → **Custom domains** → add your domain (if it's
  already on Cloudflare DNS this is a couple of clicks; otherwise follow Cloudflare's CNAME
  instructions).
- **Backend**: not usually necessary to put a custom domain on the API itself -- the
  `azurewebsites.net` URL is fine to keep calling from the frontend.
- Once you have the final frontend domain, update `Cors__AllowedOrigins__0` in the App Service
  settings (step 3) to match it exactly.

## 7. Post-deploy verification checklist

- [ ] Frontend loads at the Cloudflare Pages URL, all 8 nav tabs work (confirms the `_redirects`
      SPA fallback is working -- try loading `/projects` directly, not just via client-side nav)
- [ ] Projects page shows live GitHub repos
- [ ] Contact form submits successfully and a row appears in the production `ContactMessages`
      table
- [ ] Ask AI returns a real LLM-generated answer (not the local fallback) -- confirms the OpenAI
      key is set correctly in App Service config
- [ ] Browser devtools → Network tab: confirm no OpenAI/SMTP keys appear anywhere in responses
      or the loaded JS bundle (same bundle-grep check used throughout this project)
- [ ] `https://` loads cleanly on both frontend and backend (no mixed-content warnings)
