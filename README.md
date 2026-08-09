# Handin — Assignment & Submission Management System

A role-based web application for schools/colleges where **teachers** create and grade
assignments, **students** submit and track their work, and **admins** manage people and
academic structure. Built for the OnnoRokom Projukti Assistant Software Engineer
recruitment project.

## Overview

Three roles, one system:

- **Admin** — creates Admin/Teacher/Student accounts, classes/courses, subjects, and
  assigns teachers to the subject(s)/class(es) they teach.
- **Teacher** — creates assignments (draft or published) for a subject/class they are
  assigned to, sets a deadline and max marks, reviews submissions, and awards marks and
  feedback.
- **Student** — sees published assignments for their enrolled class, submits an answer
  before the deadline, can update it if the teacher allows resubmission, and sees their
  marks and feedback once graded.

## Main features

- JWT-based authentication with three roles enforced **server-side** on every endpoint
  (not just hidden in the UI).
- Full assignment lifecycle: draft → publish → submit → (optionally resubmit) → grade.
- Teachers can also change a submission's status independently of grading (e.g. mark it
  "Returned for revision" to send it back to the student without awarding marks).
- Admin has a read-only, system-wide view of every assignment and submission across all
  teachers and classes — not just the ones scoped to their own account.
- Business rules enforced in the API, not just the UI:
  - A teacher can only create/edit/delete assignments for a subject/class they are
    actually assigned to teach.
  - A student can only see and submit to assignments for their own enrolled class.
  - Submissions are rejected after the deadline; updates are blocked after the deadline,
    after grading, or if the teacher disabled resubmission.
  - Marks awarded can never exceed an assignment's maximum marks.
- Swagger/OpenAPI docs with a built-in "Authorize" button for testing with a JWT.
- Seed data + working demo accounts created automatically on first run.

## Technology stack

| Layer      | Technology                                                             |
|------------|-------------------------------------------------------------------------|
| Frontend   | Next.js 14 (App Router), React 18, TypeScript, Tailwind CSS            |
| Backend    | ASP.NET Core 8 Web API, C#, EF Core 8                                  |
| Database   | PostgreSQL (Npgsql provider)                                           |
| Auth       | JWT bearer tokens, role-based authorization policies                   |
| Testing    | xUnit, Moq, FluentAssertions, EF Core InMemory provider                |
| Docs       | Swashbuckle (Swagger / OpenAPI)                                        |
| Containers | Docker + docker-compose (Postgres, API, web — all optional)            |

## Project structure

```
.
├── backend/
│   ├── AssignmentSystem.sln
│   ├── Dockerfile
│   ├── src/
│   │   ├── AssignmentSystem.Api/            # Controllers, Program.cs, middleware, config
│   │   ├── AssignmentSystem.Core/           # Entities, DTOs, service interfaces (no dependencies)
│   │   └── AssignmentSystem.Infrastructure/ # EF Core DbContext, JWT/password services, business logic
│   └── tests/
│       └── AssignmentSystem.Tests/          # xUnit tests for business rules & authorization
├── frontend/
│   ├── Dockerfile
│   └── src/
│       ├── app/                             # Next.js App Router pages (login, admin, teacher, student)
│       ├── components/                      # Shared UI (AppShell, RequireRole, StatusPill, ...)
│       ├── lib/                             # API client, auth context, shared TS types
│       └── middleware.ts                    # Route protection by role
├── database/
│   └── schema.sql                           # Plain-SQL reference/fallback to the EF Core schema
├── docker-compose.yml                       # Postgres + API + web, one command to run everything
└── .env.example                             # All required environment variables, documented
```

## Setup instructions

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) and npm
- [PostgreSQL 14+](https://www.postgresql.org/download/) (or Docker, see below)
- (Optional) [Docker](https://www.docker.com/) and docker-compose, for the one-command setup

### Option A — Docker (fastest)

This starts Postgres, the API, and the frontend together:

```bash
docker compose up --build
```

- Frontend: http://localhost:3000
- API + Swagger: http://localhost:5000/swagger

The API automatically applies EF Core migrations and seeds demo accounts on first
start (see "Database setup" below for how migrations are generated).

> Note: the `Jwt__Secret` in `docker-compose.yml` is a placeholder for local use only —
> replace it before deploying anywhere real.

### Option B — Run locally without Docker

#### 1. Database setup

Create a local Postgres database and user matching `ConnectionStrings:DefaultConnection`
in `backend/src/AssignmentSystem.Api/appsettings.json` (defaults to
`Host=localhost;Port=5432;Database=assignment_system;Username=postgres;Password=postgres`),
or override it via an environment variable / `dotnet user-secrets` — see `.env.example`.

EF Core migrations are already generated and committed under
`backend/src/AssignmentSystem.Infrastructure/Migrations/`. You don't need to run
`dotnet ef` yourself — the API applies any pending migrations automatically on startup
(see `Program.cs`), against whatever empty `assignment_system` database you point it at.
`database/schema.sql` is also included as a human-readable reference to the same schema,
in case anyone wants to inspect it without the EF tooling.

If you ever change the entity model and need to generate a *new* migration:

```bash
cd backend
dotnet tool install --global dotnet-ef   # if not already installed
dotnet ef migrations add YourMigrationName \
  --project src/AssignmentSystem.Infrastructure \
  --startup-project src/AssignmentSystem.Api
```

Demo accounts and a sample class/subject/assignment are then seeded **automatically**
the first time the API starts (see `DbSeeder.cs`) — no manual seed script needed.

#### 2. Run the backend

```bash
cd backend
dotnet restore
dotnet run --project src/AssignmentSystem.Api
```

The API starts on `http://localhost:5000` with Swagger UI at `/swagger`.

Set a real JWT secret before running (don't leave the placeholder in
`appsettings.json`):

```bash
cd src/AssignmentSystem.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Secret" "$(openssl rand -base64 48)"
```

#### 3. Run the frontend

```bash
cd frontend
cp .env.example .env.local   # defaults to http://localhost:5000, adjust if needed
npm install
npm run dev
```

Open http://localhost:3000 — it will redirect to `/login`.

#### 4. Run the tests

```bash
cd backend
dotnet test
```

This runs the full xUnit suite covering the submission workflow (deadlines,
resubmission rules, grading, duplicate submissions) and assignment/teacher
authorization rules, using EF Core's InMemory provider (no real database needed).

## Demo credentials

Seeded automatically on first run of the API:

| Role    | Email                | Password     |
|---------|-----------------------|--------------|
| Admin   | admin@school.test     | Passw0rd!    |
| Teacher | teacher@school.test   | Passw0rd!    |
| Student | student@school.test   | Passw0rd!    |

The seeded teacher is assigned to teach **Physics** for **Grade 9 - Section A**, and the
seeded student is enrolled in that same class, with one sample published assignment
already waiting to be submitted.

## Assumptions

Documented here as requested, since the brief left some design decisions open:

1. **Student enrollment is one class/course at a time.** A student belongs to exactly
   one `ClassCourse` (modeled as a 1:1 `StudentEnrollment`). This matches how a school
   "class" or a college "course cohort" usually works. Extending to multiple
   concurrent enrollments (e.g. electives) would mean changing this to a many-to-many
   table — noted as a possible extension.
2. **A subject belongs to exactly one class/course.** "Physics for Grade 9-A" and
   "Physics for Grade 10-A" are modeled as two separate `Subject` rows, both created by
   the admin. This keeps assignment/subject scoping simple and avoids ambiguity about
   which class a subject-level assignment applies to.
3. **Teachers are explicitly assigned to a (subject, class) pair by the admin**
   (`TeacherSubjectClass`), and can only create/manage assignments for pairs they're
   assigned to. This directly implements "Admin: assign teachers to subjects/classes."
4. **Submission answers are plain text** (`AnswerText`), with an optional
   `AttachmentUrl` field already modeled for a future file-upload extension. Actual file
   upload/storage (e.g. to disk or S3) was treated as optional, per the brief's
   "optional additions" list, and left unimplemented to keep scope focused on the core
   workflow.
5. **One submission per student per assignment.** Resubmission is an *update* to the
   same submission (if the teacher's `AllowResubmission` flag is on and the deadline
   hasn't passed), not a new row — this preserves a single source of truth for grading
   and avoids the API needing to reconcile multiple attempts.
6. **Deactivation instead of hard deletion for users.** Admin "deletion" of a user is a
   soft deactivation (`IsActive = false`), preserving assignment/submission history and
   avoiding cascading deletes that would silently destroy academic records.
7. **Marks and status changes are teacher-only**, and scoped to the teacher's own
   assignments — a teacher cannot grade or view submissions belonging to another
   teacher's assignment, even within the same subject/class.
8. **Generic login error messages.** "Invalid email or password" is returned for both a
   nonexistent email and a wrong password, to avoid leaking which emails are registered.

## Known limitations

- **File upload for submissions is not implemented** — see Assumption 4. The data model
  already supports an attachment URL for a future extension.
- **No pagination or advanced filtering** on list endpoints (assignments, users,
  submissions) — acceptable at the scale of a single class/course, called out here as a
  known gap rather than an oversight.
- **No email notifications** (e.g. "assignment graded", "new assignment published") —
  out of scope per the brief's optional-additions list.
- **No integration/end-to-end tests** — the test suite covers business rules and
  authorization logic at the service layer, there are no `WebApplicationFactory`-based
  HTTP-level tests yet.
- **No live-hosted URL is included.** A live deployment was attempted (frontend on
  Vercel, backend/database on several free-tier hosts) but every general-purpose backend
  host we tried that could run a Dockerized ASP.NET Core API for free either required
  card verification for identity purposes or had discontinued its free self-serve hosting
  product entirely. Since a live URL is explicitly listed as an **optional** addition in
  the brief (not a requirement), and adding a card was outside what we were willing to do
  for a free-tier demo, we left it out rather than take on cost or the risk of a
  mid-evaluation platform outage. Everything runs correctly locally per the setup
  instructions above (verified via a full fresh `git clone`, including from an empty
  database).

## API documentation

Once the backend is running, interactive API docs (with a JWT "Authorize" button for
testing protected endpoints) are available at:

```
http://localhost:5000/swagger
```
