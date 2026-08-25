# Patient Worklist API

A full-stack hospital worklist application for managing patients, doctors, and imaging studies. Built with ASP.NET Core Web API, Entity Framework Core, SQLite, and a Bootstrap frontend.

## Overview

This application simulates a radiology department worklist — the screen used to track which imaging studies are scheduled, in progress, or completed for each patient. It provides a REST API backend and a single-page frontend served by the same ASP.NET Core server.

## Tech Stack

| Layer | Technology |
|-------|------------|
| Backend | ASP.NET Core (.NET 7) |
| ORM | Entity Framework Core 7 |
| Database | SQLite |
| API Docs | Swagger / Swashbuckle |
| Frontend | Bootstrap 5, jQuery, DataTables |

## Project Structure

```
├── Controllers/          # REST API endpoints
│   ├── PatientsController.cs
│   ├── DoctorsController.cs
│   └── StudiesController.cs
├── Data/                 # EF Core DbContext + seeder
│   ├── ApplicationDbContext.cs
│   └── DbSeeder.cs
├── DTOs/                 # Data Transfer Objects + mappers
│   ├── PersonDto.cs
│   ├── PatientDtos.cs
│   ├── DoctorDtos.cs
│   └── StudyDtos.cs
├── Entities/             # Database entity models
│   ├── Person.cs
│   ├── Patient.cs
│   ├── Doctor.cs
│   └── Study.cs
├── Repositories/         # Repository pattern (generic + specific)
│   ├── IRepository.cs
│   ├── GenericRepository.cs
│   ├── IPatientRepository.cs / PatientRepository.cs
│   ├── IDoctorRepository.cs / DoctorRepository.cs
│   └── IStudyRepository.cs / StudyRepository.cs
├── wwwroot/              # Static frontend
│   ├── index.html
│   ├── app.js
│   └── style.css
├── Program.cs            # Application entry point
├── appsettings.json      # Configuration
└── PatientWorklist.API.csproj
```

## Data Model

- **Person** — shared record for name, DOB, gender, contact info
- **Patient** — extends Person with MRN and status
- **Doctor** — extends Person with specialty
- **Study** — links a Patient to a Doctor with modality, date, and status

Relationships: Person 1:1 Patient, Person 1:1 Doctor, Patient 1:N Studies, Doctor 1:N Studies.

## API Endpoints

### Patients

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/patients` | List all patients |
| GET | `/api/patients/{id}` | Get patient by ID |
| POST | `/api/patients` | Create patient |
| PUT | `/api/patients/{id}` | Update patient |
| DELETE | `/api/patients/{id}` | Delete patient |

### Doctors

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/doctors` | List all doctors |
| GET | `/api/doctors/{id}` | Get doctor by ID |
| POST | `/api/doctors` | Create doctor |
| PUT | `/api/doctors/{id}` | Update doctor |
| DELETE | `/api/doctors/{id}` | Delete doctor (only if no studies) |

### Studies

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/studies` | List all studies (optional `?patientId=` / `?doctorId=` filters) |
| GET | `/api/studies/{id}` | Get study by ID |
| POST | `/api/studies` | Create study |
| PUT | `/api/studies/{id}` | Update study |
| DELETE | `/api/studies/{id}` | Delete study |

## Getting Started

### Prerequisites

- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)

### Run

```bash
dotnet run
```

The app will:
1. Create `PatientWorklist.db` and seed demo data on first run
2. Start the API and serve the frontend

Open `https://localhost:5001` (or `http://localhost:5000`) in your browser.

Swagger UI is available at `/swagger` in development mode.

## Architecture

```
Controllers (HTTP)  →  Repositories (data access)  →  EF Core / SQLite
```

- Controllers handle HTTP requests, validate input, and return DTOs
- Repositories encapsulate all database queries using the Repository Pattern
- Generic repository provides CRUD; specific repositories add eager loading with `.Include()`
- DTOs separate the API contract from the database schema

## Frontend

The frontend is a single-page application (`wwwroot/index.html`) with three tabs:

- **Studies** — view and manage imaging studies
- **Patients** — add, edit, delete patients
- **Doctors** — add, edit, delete doctors

Built with Bootstrap 5 for UI, DataTables for sortable/searchable tables, and vanilla JavaScript with `fetch()` for API calls.

## License

MIT
