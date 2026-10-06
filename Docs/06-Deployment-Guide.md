# Deployment Guide — Personal Portfolio Website

Reference: [02-High-Level-Design.md](02-High-Level-Design.md) section 4,
[04-Implementation-Plan.md](04-Implementation-Plan.md) Phase 8.

## 0. Provisioned resources (live)

All-Azure, free tier, under subscription `fad09c44-c628-44bc-93ff-800d366ac194`
(`swapnamoykar2000@gmail.com`), resource group **`portfolio-rg`**.

| Resource | Name | Region | Tier | URL |
|---|---|---|---|---|
| SQL logical server | `portfolio-sql-swapnamoy2000` | Central US | — | `portfolio-sql-swapnamoy2000.database.windows.net` |
| SQL database | `PortfolioDb` | Central US | Free limit (GP_S_Gen5_2, serverless, auto-pause on exhaustion) | — |
| App Service plan | `portfolio-plan-eastasia` | East Asia | F1 (Free) | — |
| App Service (API) | `portfolio-api-swapnamoy` | East Asia | Free | `https://portfolio-api-swapnamoy.azurewebsites.net` |
| Static Web App (UI) | `portfolio-ui-swapnamoy` | Central US | Free | `https://proud-pebble-003f8d410.5.azurestaticapps.net` |

Region notes: `eastus`/`eastus2` rejected new SQL servers and had zero F1 quota on this
subscription at creation time (common for new/free subscriptions) — East Asia and Central US
had capacity. Mixed regions are fine here; it's a low-traffic personal site, the extra
cross-region latency between App Service and SQL is negligible.

**Already configured on the App Service** (`portfolio-api-swapnamoy`):
- `ConnectionStrings__DefaultConnection` → the real `PortfolioDb` connection string
- `Cors__AllowedOrigins__0` → `https://proud-pebble-003f8d410.5.azurestaticapps.net`
- `ASPNETCORE_ENVIRONMENT` → `Production`

**Already configured in the repo**:
- `UI/src/environments/environment.prod.ts` → points at the real API URL above
- `UI/public/_redirects` → SPA routing fallback (harmless if unused; Azure Static Web Apps
  handles SPA fallback natively via its own routing, so this file matters only if you ever
  switch to Cloudflare Pages)

**Not yet done** (needs your GitHub login — see steps below):
1. Push this repo to GitHub
2. Add 3 GitHub Actions secrets
3. First push triggers both workflows, which actually deploy code to the two (currently empty)
   Azure resources
4. Add the real OpenAI key to the App Service (you chose to defer this until the end)

## 1. Push to GitHub

```bash
# from C:\Users\Admin\Desktop\Portfolio
gh repo create portfolio --private --source=. --remote=origin
git push -u origin master
```

No `gh` CLI? Create the repo manually at github.com/new instead, then:

```bash
git remote add origin https://github.com/<your-username>/portfolio.git
git push -u origin master
```

## 2. Add the 3 GitHub Actions secrets

**Settings → Secrets and variables → Actions → New repository secret** on the GitHub repo:

| Secret name | Value | How to get it |
|---|---|---|
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | the SWA deployment token | `az staticwebapp secrets list --name portfolio-ui-swapnamoy --resource-group portfolio-rg --query properties.apiKey -o tsv` |
| `AZURE_WEBAPP_NAME` | `portfolio-api-swapnamoy` | — |
| `AZURE_WEBAPP_PUBLISH_PROFILE` | the publish profile XML | `az webapp deployment list-publishing-profiles --name portfolio-api-swapnamoy --resource-group portfolio-rg --xml` |

Both workflows (`.github/workflows/deploy-api.yml` and `deploy-ui.yml`) are already in the repo
and path-filtered (API changes deploy only the API, UI changes deploy only the UI). Once the
secrets are in place, push to `main`/`master` and both deploy automatically.

## 3. Add the real OpenAI key

This is the moment you were waiting for — everything else has been running on the local
keyword-match fallback in `RagService` until now:

```bash
az webapp config appsettings set \
  --name portfolio-api-swapnamoy \
  --resource-group portfolio-rg \
  --settings OpenAI__ApiKey="<your real OpenAI API key>"
```

No redeploy needed — App Service picks up new app settings on the next request after a brief
restart. Ask AI should start returning real generated answers instead of the extractive
fallback immediately after.

## 4. Connecting via SSMS (optional)

The SQL server's firewall only allows Azure's own services by default (secure default — the
database isn't open to the public internet). To connect from SSMS on your own machine:

```bash
az sql server firewall-rule create \
  --resource-group portfolio-rg \
  --server portfolio-sql-swapnamoy2000 \
  --name AllowMyIP \
  --start-ip-address <your-public-ip> \
  --end-ip-address <your-public-ip>
```

Find your public IP at whatismyip.com, or let the Azure Portal's "Add client IP" button on the
SQL server's networking blade do it for you.

## 5. Custom domain (optional)

- **Frontend**: Static Web App → **Custom domains** → add your domain, follow the CNAME/TXT
  instructions Azure gives you.
- **Backend**: not usually necessary to put a custom domain on the API itself — the
  `azurewebsites.net` URL is fine to keep calling from the frontend.
- Once you have the final frontend domain, update `Cors__AllowedOrigins__0` on the App Service
  to match it exactly (same `az webapp config appsettings set` command as step 3, different key).

## 6. Post-deploy verification checklist

- [ ] Frontend loads at the Static Web Apps URL, all 8 nav tabs work (try loading `/projects`
      directly via URL, not just client-side nav, to confirm SPA routing works)
- [ ] Projects page shows live GitHub repos
- [ ] Contact form submits successfully and a row appears in the production `ContactMessages`
      table (connect via SSMS per step 4 to check, or query via `sqlcmd` from a machine whose
      IP you've allow-listed)
- [ ] Ask AI returns a real LLM-generated answer, not the local fallback — confirms the OpenAI
      key from step 3 is active
- [ ] Browser devtools → Network tab: confirm no OpenAI/SMTP keys appear anywhere in responses
      or the loaded JS bundle (same bundle-grep check used throughout this project)
- [ ] `https://` loads cleanly on both frontend and backend (Azure provides this by default on
      both App Service and Static Web Apps — no certificate setup needed)
