# DevForge: The Ultimate Personal AI Software Factory

DevForge is an open-source, self-hosted Internal Developer Platform (IDP) designed to run on a local Raspberry Pi 5 node equipped with a 1TB NVMe SSD. It transforms database schema definitions directly into verified, running business applications.

---

## Directory Layout

*   `/control-plane-api`: .NET 9 API utilizing Clean Architecture, CQRS (MediatR), FluentValidation, and EF Core.
*   `/portal-ui`: Next.js 15 App Router Frontend portal (Tailwind CSS, React hooks, and dashboard stats).
*   `docker-compose.yml`: Dynamic orchestration blueprint linking PostgreSQL, Redis, APIs, portals, and proxies.

---

## Setup & Execution

### 1. Configure Secrets
Copy `.env.example` to `.env` in the root directory and fill in your own values:
```env
DB_PASSWORD=<your_db_password>
GRAFANA_ADMIN_PASSWORD=<your_grafana_admin_password>
GEMINI_API_KEY=<your_gemini_api_key>
```
No default credentials ship in this repository — `docker-compose` will refuse to start until these are set.

### 2. Boot Local Control Plane
Start the background services:
```bash
docker-compose up -d
```

### 3. Launch Development Environments
*   **Backend API**: Launch `control-plane-api/DevForge.ControlPlane.slnx` inside your IDE or run `dotnet run --project src/WebApi`.
*   **Portal Frontend**: Boot the next development portal:
    ```bash
    cd portal-ui
    npm run dev
    ```
    Access the portal at `http://localhost:3000`.
