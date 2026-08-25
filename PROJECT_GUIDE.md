# Patient Worklist API & Frontend — Complete Study Guide

A full walkthrough of the project: what it is, the technologies, the architecture, the request/response flow, and an explanation of **every file** and **every function**, so you can study and discuss it in detail.

---

## Table of Contents

1. [What is this project?](#1-what-is-this-project)
2. [Technology Stack](#2-technology-stack)
3. [Project Structure](#3-project-structure)
4. [Data Model (Database Design)](#4-data-model-database-design)
5. [Application Flow (End-to-End Request Lifecycle)](#5-application-flow-end-to-end-request-lifecycle)
6. [Startup Configuration (`Program.cs`)](#6-startup-configuration-programcs)
7. [Entities Layer](#7-entities-layer)
8. [Data Layer (DbContext + Seeder)](#8-data-layer-dbcontext--seeder)
9. [Repository Layer (Generic + Specific)](#9-repository-layer-generic--specific)
10. [DTO Layer & Mappers](#10-dto-layer--mappers)
11. [Controllers (REST API Endpoints)](#11-controllers-rest-api-endpoints)
12. [Frontend (wwwroot)](#12-frontend-wwwroot)
13. [Configuration Files](#13-configuration-files)
14. [Key Concepts to Discuss](#14-key-concepts-to-discuss)
15. [How to Run the Project](#15-how-to-run-the-project)

---

## 1. What is this project?

**Patient Worklist** is a hospital-style web application for managing:

- **Patients** — demographic info plus a Medical Record Number (MRN) and status.
- **Doctors** — the same person demographics plus a medical specialty.
- **Studies** — imaging procedures (CT, MRI, X-Ray, Ultrasound...) linking a patient to a doctor, with a modality, date, and status.

It is a **full-stack application**:

- **Backend** = ASP.NET Core Web API (C#) exposing a REST API with SQLite storage via Entity Framework Core.
- **Frontend** = a single static HTML page (`wwwroot/index.html`) served by the same ASP.NET Core server, using **Bootstrap 5**, **jQuery**, and **DataTables**. It talks to the API with `fetch()`.

The whole point is a **worklist** — the screen a radiology department uses to see which studies are scheduled, in progress, or completed for each patient.

---

## 2. Technology Stack

| Layer        | Technology                                                        |
| ------------ | ----------------------------------------------------------------- |
| Language     | C# (targeting **.NET 7**), JavaScript (vanilla + jQuery)          |
| Web framework| **ASP.NET Core** (minimal hosting in `Program.cs`)                |
| ORM          | **Entity Framework Core 7** (`Microsoft.EntityFrameworkCore`)     |
| Database     | **SQLite** (`Microsoft.EntityFrameworkCore.Sqlite`)                |
| API docs     | **Swashbuckle / Swagger UI** (Swashbuckle.AspNetCore)             |
| Frontend CSS | **Bootstrap 5.3** (via CDN)                                       |
| Frontend JS  | **jQuery 3.7**, **DataTables 1.13** (via CDN)                     |
| DI container | Built-in `Microsoft.Extensions.DependencyInjection`               |

Packages are declared in `PatientWorklist.API.csproj`.

### Key file — `PatientWorklist.API.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <InvariantGlobalization>false</InvariantGlobalization>
    <UseAppHost>false</UseAppHost>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="7.0.20" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="7.0.20" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="7.0.20" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
  </ItemGroup>
</Project>
```

**What each line means:**

- `Sdk="Microsoft.NET.Sdk.Web"` → it's an ASP.NET Core web project (gets the web tooling, the `WebApplication` API, and `wwwroot` static file support).
- `net7.0` → target framework = .NET 7.
- `Nullable enable` → nullable reference types are on; `string?` means "this can be null", `string` means "not null".
- `ImplicitUsings enable` → common `using` directives (System, System.Linq, etc.) are auto-imported, so most files don't need them.
- `InvariantGlobalization=false` → full globalization support (culture-aware formatting, dates, etc.).
- `UseAppHost=false` → build produces a `.dll` instead of a platform executable (makes builds portable).
- Package references bring in EF Core, the SQLite provider, EF tooling (migrations CLI), and Swagger.

---

## 3. Project Structure

```
Final Project/
├── Controllers/
│   ├── PatientsController.cs   → REST endpoints for /api/patients
│   ├── DoctorsController.cs    → REST endpoints for /api/doctors
│   └── StudiesController.cs    → REST endpoints for /api/studies
├── Data/
│   ├── ApplicationDbContext.cs → EF Core database context (tables + relationships)
│   └── DbSeeder.cs             → Creates DB & inserts demo data on startup
├── DTOs/
│   ├── PersonDto.cs            → Person DTOs + shared Age calculator
│   ├── PatientDtos.cs          → Patient DTOs + PatientMapper
│   ├── DoctorDtos.cs           → Doctor DTOs + DoctorMapper
│   └── StudyDtos.cs            → Study DTOs + StudyMapper
├── Entities/
│   ├── Person.cs               → shared human record (first/last name, DOB, gender, contact)
│   ├── Patient.cs              → patient record (MRN, status)
│   ├── Doctor.cs               → doctor record (specialty)
│   └── Study.cs                → imaging study linking patient ↔ doctor
├── Repositories/
│   ├── IRepository.cs          → generic repository interface
│   ├── GenericRepository.cs    → generic CRUD implementation
│   ├── IPatientRepository.cs   → patient-specific interface
│   ├── PatientRepository.cs    → patient-specific queries (with related data)
│   ├── IDoctorRepository.cs    → doctor-specific interface
│   ├── DoctorRepository.cs     → doctor-specific queries
│   ├── IStudyRepository.cs     → study-specific interface
│   └── StudyRepository.cs      → study-specific queries
├── Properties/
│   └── launchSettings.json     → dev launch profiles (ports, env vars)
├── wwwroot/
│   ├── index.html              → the single-page frontend
│   ├── app.js                  → all frontend logic (fetch, DataTables, modals)
│   └── style.css               → custom styling
├── Program.cs                  → application entry point (wiring everything together)
├── appsettings.json            → connection string + logging config
├── appsettings.Development.json→ dev-only logging config
└── PatientWorklist.db          → the SQLite database file (auto-created/seeded)
```

---

## 4. Data Model (Database Design)

Four tables: **Persons**, **Patients**, **Doctors**, **Studies**.

### 4.1 Entity Relationships (as an ER diagram)

```
Person (1) ──── (1) Patient
   │  PersonId                     PatientId
   │                               PersonId  (FK → Person, cascade delete)
   │                               MRN (unique)
   │                               Status
   │
   └── (1) Doctor
       DoctorId
       PersonId (FK → Person, cascade delete)
       Specialty
       Studies ─┐
                │   DoctorId (FK → Doctor, RESTRICT delete)
                │
Patient ────────┘   PatientId (FK → Patient, cascade delete)
   │                Modality
   └── Studies      StudyDate  (indexed with PatientId)
                    Status
```

### 4.2 Why the 1-to-1 Person split?

Instead of putting name/DOB/contact directly on `Patient` and `Doctor`, both store a **PersonId** pointing to a shared `Person` table. Benefits:

- **No duplicated columns** between patients and doctors.
- A person could theoretically be both a patient and a doctor (one `Person` row, two profile rows).
- The DB guarantees a patient/doctor always has a valid person.

### 4.3 Constraints and Delete Behaviors (defined in `ApplicationDbContext.OnModelCreating`)

| Relationship | Foreign key | Delete behavior | Meaning |
|---|---|---|---|
| Person → Patient | `Patient.PersonId` | **Cascade** | Deleting a Person deletes the Patient |
| Person → Doctor | `Doctor.PersonId` | **Cascade** | Deleting a Person deletes the Doctor |
| Patient → Study | `Study.PatientId` | **Cascade** | Deleting a Patient deletes all their Studies |
| Doctor → Study | `Study.DoctorId` | **Restrict** | A Doctor with studies **cannot** be deleted (DB blocks it; the API also checks first) |

Indexes/unique constraints:

- `Person.Email` → unique.
- `Patient.MRN` → unique.
- `Study(PatientId, StudyDate)` → composite index (fast lookups "all studies of a patient by date").

---

## 5. Application Flow (End-to-End Request Lifecycle)

### 5.1 Startup flow

1. `Program.cs` builds a `WebApplication` from the **builder**.
2. Services are registered in DI: controllers, Swagger, `ApplicationDbContext`, repositories.
3. `EnsureCreated()` + `DbSeeder.Seed()` run inside a scope → DB file is created and demo data inserted **if** `Doctors` is empty.
4. Middleware pipeline is assembled: Swagger → CORS → HTTPS redirect → Authorization → Controllers → static files.
5. `app.Run()` starts the Kestrel web server listening on the configured ports.

### 5.2 A request from the browser — example: "Show all patients"

```
Browser (index.html)
   │  DataTables ajax GET /api/patients        (fetch, app.js:88)
   ▼
Kestrel (ASP.NET Core server, Program.cs pipeline)
   │
   │  CORS "AllowAll" (any origin allowed)
   │  HTTPS redirect (skipped on plain http)
   │  Routing → Maps to PatientsController.GetPatients
   ▼
PatientsController.GetPatients()          (Controllers/PatientsController.cs:22)
   │  calls _patientRepository.GetAllWithDetailsAsync()
   ▼
PatientRepository.GetAllWithDetailsAsync()  (Repositories/PatientRepository.cs:13)
   │  EF Core: _dbSet.Include(Person).Include(Studies).AsNoTracking().ToListAsync()
   │  → SQL: SELECT ... FROM Patients JOIN Persons LEFT JOIN Studies ...
   ▼
SQLite database (PatientWorklist.db)
   │  returns the rows back up the stack
   ▼
PatientRepository returns IEnumerable<Patient>
   ▼
Controller maps each Patient → PatientDto via PatientMapper.ToDto(patient)
   │  returns Ok(...) → JSON array  (200 OK)
   ▼
Browser
   │  DataTables receives JSON, renders table rows with Edit/Delete/Studies buttons
   └─ user clicks "Add Patient" → POST /api/patients → same stack → 201 Created
```

### 5.3 The layered architecture (why it's split this way)

```
┌────────────────────────────┐
│  Controllers (HTTP layer)  │  ⇄ receives HTTP, validates, maps, returns status codes
├────────────────────────────┤
│  Repositories (data layer) │  ⇄ the ONLY place that talks to EF Core / SQL
├────────────────────────────┤
│  ApplicationDbContext      │  ⇄ EF Core maps entities ⇄ SQLite tables
└────────────────────────────┘
```

- **Controllers** never touch `DbContext` directly — they only use repositories (good separation / testability).
- **Repositories** never return DTOs — they return entities. Mapping happens in the controller (via static `*Mapper` classes).
- This is the classic **Repository Pattern** layered on top of EF Core.

---

## 6. Startup Configuration (`Program.cs`)

```csharp
var builder = WebApplication.CreateBuilder(args);

// Controllers + API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database (SQLite + EF Core)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository pattern wiring
builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IStudyRepository, StudyRepository>();

// CORS (so the page can call the API from any origin)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// Create DB + seed demo data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    DbSeeder.Seed(dbContext);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
```

### Line-by-line explanation

| Line | What it does |
|---|---|
| `WebApplication.CreateBuilder(args)` | Bootstraps the host: reads config, sets up logging, DI, server. |
| `builder.Services.AddControllers()` | Registers MVC controllers as services + enables `[ApiController]` behaviors. |
| `AddEndpointsApiExplorer()` + `AddSwaggerGen()` | Provides Swagger metadata for the API (interactive docs UI). |
| `AddDbContext<ApplicationDbContext>(...)` | Registers the EF Core context in DI with a **Scoped** lifetime; wires it to the SQLite provider using the `DefaultConnection` string from `appsettings.json`. |
| `AddScoped<IRepository<>, GenericRepository<>>` | Registers the **open generic** repository so any `IRepository<T>` can be injected. |
| `AddScoped<IPatientRepository, PatientRepository>` etc. | Registers the concrete repositories (scoped = one instance per HTTP request). |
| `AddCors` + `AllowAll` policy | Lets the frontend call the API from any origin/method/header (dev convenience; not secure for production). |
| `CreateScope()` + `DbSeeder.Seed(dbContext)` | Creates an isolated DI scope, pulls the DbContext out, and seeds data before serving traffic. |
| `UseSwagger() / UseSwaggerUI()` | Dev-only interactive API docs at `/swagger`. |
| `UseCors("AllowAll")` | Applies the CORS policy. **Order matters** — it must be after routing and before endpoints. |
| `UseHttpsRedirection()` | Redirects http → https (warns when no https port is configured). |
| `UseAuthorization()` | Authorization middleware (no auth is configured, so it's effectively a no-op). |
| `MapControllers()` | Enables attribute-based routing for `[Route("api/...")]` controllers. |
| `UseDefaultFiles()` + `UseStaticFiles()` | Serves `wwwroot/index.html` at `/` and all static assets (JS/CSS). |
| `app.Run()` | Blocks and starts serving requests. |

**DI lifetimes refresher (important for discussion):**
- **Scoped** = created once per HTTP request. Perfect for `DbContext` and repositories, because one request = one unit of work.
- Transient = new instance every injection. Singleton = one for the whole process.

---

## 7. Entities Layer

Entities are plain C# classes (POCOs) that map 1-to-1 to database tables. Validation is done with **DataAnnotations** and also declared in `OnModelCreating` (Fluent API). The attribute-based version drives **model validation** (when the API auto-checks `ModelState`); the fluent version drives **database schema**.

### 7.1 `Entities/Person.cs`

```csharp
public class Person
{
    [Key]  public int PersonId { get; set; }
    [Required][MaxLength(100)] public string FirstName { get; set; } = string.Empty;
    [Required][MaxLength(100)] public string LastName { get; set; } = string.Empty;
    [Required] public DateTime DateOfBirth { get; set; }
    [Required][MaxLength(20)] public string Gender { get; set; } = string.Empty;
    [MaxLength(20)] public string? Phone { get; set; }
    [EmailAddress][MaxLength(150)] public string? Email { get; set; }

    public Patient? Patient { get; set; }   // navigation: inverse of Patient.Person
    public Doctor? Doctor { get; set; }     // navigation: inverse of Doctor.Person
}
```

**Every member explained:**
- `[Key]` → `PersonId` is the primary key; EF uses `Id`/`<TypeName>Id` naming convention, so it would be a key even without the attribute.
- `[Required]` → must be present; maps to a `NOT NULL` column.
- `[MaxLength(n)]` → max string length; maps to `VARCHAR(n)` in SQL.
- `[EmailAddress]` → server-side validation that the value looks like an email (checked by ModelState before save).
- `string?` (nullable) Phone/Email → optional columns (`NULL` allowed in DB).
- Navigation properties `Patient`/`Doctor` → EF uses these to build the 1-to-1 relationship. They're nullable because a Person may be neither/both.
- `= string.Empty` initializers → avoid null-reference warnings from the `Nullable enable` setting.

### 7.2 `Entities/Patient.cs`

```csharp
public class Patient
{
    [Key] public int PatientId { get; set; }

    [ForeignKey(nameof(Person))]
    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    [Required][MaxLength(50)] public string MRN { get; set; } = string.Empty;
    [Required][MaxLength(50)] public string Status { get; set; } = string.Empty;

    public ICollection<Study> Studies { get; set; } = new List<Study>();
}
```

- `[ForeignKey(nameof(Person))]` → says `PersonId` is the FK to the `Person` table. `nameof(Person)` is compile-time-safe (no magic strings).
- `Person` → **required reference** (`= null!` tells the compiler "trust me, this is set after construction" — avoids nullable warnings). EF always loads/creates it because `PersonId` is `NOT NULL`.
- `MRN` (Medical Record Number) → unique per patient (enforced in `OnModelCreating`).
- `Status` → e.g. `Active`, `Pending`, `Inactive`.
- `Studies` → collection navigation; a patient **has many** studies. Initialized to empty list so it's never null.

### 7.3 `Entities/Doctor.cs`

```csharp
public class Doctor
{
    [Key] public int DoctorId { get; set; }
    [ForeignKey(nameof(Person))]
    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;
    [Required][MaxLength(100)] public string Specialty { get; set; } = string.Empty;
    public ICollection<Study> Studies { get; set; } = new List<Study>();
}
```

Identical pattern to `Patient`, but adds `Specialty` (Radiology, Cardiology, etc.) and has **many studies** too.

### 7.4 `Entities/Study.cs`

```csharp
public class Study
{
    [Key] public int StudyId { get; set; }
    [ForeignKey(nameof(Patient))] public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    [ForeignKey(nameof(Doctor))] public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    [Required][MaxLength(50)] public string Modality { get; set; } = string.Empty;
    public DateTime StudyDate { get; set; }
    [Required][MaxLength(50)] public string Status { get; set; } = string.Empty;
}
```

- **Many-to-one** to both `Patient` and `Doctor` (each study belongs to exactly one of each).
- `Modality` = imaging type: `CT`, `MRI`, `X-Ray`, `Ultrasound`, `PET`, `Mammography`.
- `StudyDate` = when the imaging happens/is scheduled.
- `Status` = `Scheduled`, `In Progress`, `Completed`, `Cancelled`...

---

## 8. Data Layer (DbContext + Seeder)

### 8.1 `Data/ApplicationDbContext.cs`

The class that EF Core uses to talk to the database. It maps entities to tables and configures relationships.

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Study> Studies => Set<Study>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // ... fluent configuration ...
    }
}
```

**Members explained:**
- Constructor takes `DbContextOptions<ApplicationDbContext>` — these options come from DI (`AddDbContext` in Program.cs). This is **constructor injection**.
- Each `DbSet<T>` exposes a table; `=> Set<T>()` is an expression-bodied property that returns the set. EF uses the property names as table names: `Persons`, `Patients`, `Doctors`, `Studies`.
- `OnModelCreating` is called once when EF builds the model. It's the **Fluent API** — an alternative to attributes, and here it duplicates them (both approaches are used; the fluent config is authoritative for the DB schema).

**What the Fluent config does:**

```csharp
modelBuilder.Entity<Person>(entity =>
{
    // Column constraints (mirror the attributes)
    entity.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
    entity.Property(p => p.LastName).IsRequired().HasMaxLength(100);
    entity.Property(p => p.Gender).IsRequired().HasMaxLength(20);
    entity.Property(p => p.Phone).HasMaxLength(20);
    entity.Property(p => p.Email).HasMaxLength(150);

    entity.HasIndex(p => p.Email).IsUnique();   // unique index on email

    // 1-to-1 Person ↔ Patient
    entity.HasOne(p => p.Patient)               // Person.Patient
          .WithOne(p => p.Person)               // Patient.Person
          .HasForeignKey<Patient>(p => p.PersonId) // FK lives on Patient
          .OnDelete(DeleteBehavior.Cascade);    // delete Person → delete Patient

    // 1-to-1 Person ↔ Doctor
    entity.HasOne(p => p.Doctor)
          .WithOne(d => d.Person)
          .HasForeignKey<Doctor>(d => d.PersonId)
          .OnDelete(DeleteBehavior.Cascade);
});
```

Key fluent concepts:
- `HasOne(...).WithOne(...)` defines a **one-to-one** relationship.
- `HasForeignKey<Patient>(...)` specifies the dependent side (the table holding the FK).
- `HasMany(...).WithOne(...)` defines **one-to-many** (used below).
- `OnDelete(DeleteBehavior.Cascade | Restrict)` sets what happens to children when the parent is deleted.

```csharp
modelBuilder.Entity<Patient>(entity =>
{
    entity.Property(p => p.MRN).IsRequired().HasMaxLength(50);
    entity.Property(p => p.Status).IsRequired().HasMaxLength(50);
    entity.HasIndex(p => p.MRN).IsUnique();                 // unique MRN

    entity.HasMany(p => p.Studies)                          // Patient.Studies
          .WithOne(s => s.Patient)                          // Study.Patient
          .HasForeignKey(s => s.PatientId)
          .OnDelete(DeleteBehavior.Cascade);                // delete patient → delete studies
});

modelBuilder.Entity<Doctor>(entity =>
{
    entity.Property(d => d.Specialty).IsRequired().HasMaxLength(100);
    entity.HasMany(d => d.Studies)
          .WithOne(s => s.Doctor)
          .HasForeignKey(s => s.DoctorId)
          .OnDelete(DeleteBehavior.Restrict);               // cannot delete doctor with studies
});

modelBuilder.Entity<Study>(entity =>
{
    entity.Property(s => s.Modality).IsRequired().HasMaxLength(50);
    entity.Property(s => s.Status).IsRequired().HasMaxLength(50);
    entity.HasIndex(s => new { s.PatientId, s.StudyDate }); // composite index
});
```

`entity.HasIndex(s => new { s.PatientId, s.StudyDate })` creates a **composite (multi-column) index** on `(PatientId, StudyDate)` → fast when filtering a patient's studies by date.

### 8.2 `Data/DbSeeder.cs`

```csharp
public static class DbSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();   // create DB file + tables if missing
        if (context.Doctors.Any()) return;  // already seeded → skip
        // ... build 3 doctors, 5 patients, 5 studies ...
        context.Doctors.AddRange(doctors);
        context.Patients.AddRange(patients);
        context.Studies.AddRange(studies);
        context.SaveChanges();
    }
}
```

**What it does:**
- `EnsureCreated()` → creates the SQLite file and schema if it doesn't exist. (Note: this is *not* EF Migrations — it's a quick-and-dirty dev approach. Good discussion point.)
- The `if (context.Doctors.Any()) return;` guard makes seeding **idempotent** — it only seeds on first run.
- Creates 3 doctors (Radiology, Cardiology, Neurology), 5 patients (with varied statuses: Active, Pending, Inactive), and 5 studies across them (CT/MRI/X-Ray/Ultrasound with dates in 2026 and statuses Completed/Scheduled/In Progress).
- Notice the studies are created with **entity objects**, not IDs (`Patient = patients[0], Doctor = doctors[0]`) — EF Core resolves the actual FK values from these when saving. This is a neat trick of the **change tracker**.
- `SaveChanges()` executes one transaction that inserts all rows.

---

## 9. Repository Layer (Generic + Specific)

### 9.1 `Repositories/IRepository.cs` — the generic contract

```csharp
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<bool> ExistsAsync(int id);
}
```

- `where T : class` → type constraint: `T` must be a reference type (an entity).
- All methods return `Task` → **asynchronous** (non-blocking I/O).
- `T?` return → nullable reference type (could return null).

### 9.2 `Repositories/GenericRepository.cs` — the generic implementation

```csharp
public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();   // get the DbSet for whatever T is
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync() =>
        await _dbSet.ToListAsync();          // SELECT * FROM <T>

    public virtual async Task<T?> GetByIdAsync(int id) =>
        await _dbSet.FindAsync(id);          // SELECT * WHERE Id = @id (uses PK)

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);       // stage insert
        await _context.SaveChangesAsync();   // flush to DB
        return entity;                       // now has its generated Id
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);               // mark modified
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task<bool> ExistsAsync(int id) =>
        await GetByIdAsync(id) is not null;
}
```

**Important EF Core details (good discussion material):**
- **Change tracking**: EF tracks entity instances. `Add`, `Update`, `Remove` just *mark* the state (`Added`/`Modified`/`Deleted`); nothing hits the DB until `SaveChangesAsync()`. This is the **Unit of Work** pattern.
- `FindAsync(id)` first checks the **in-memory change tracker** before querying the DB.
- `virtual` methods → subclasses (the specific repositories) can override them.

### 9.3 Patient/Doctor/Study specific repositories

Each specific repository adds methods that **eagerly load related data** using `.Include(...)`.

```csharp
public class PatientRepository : GenericRepository<Patient>, IPatientRepository
{
    public PatientRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Patient>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(p => p.Person)     // load Patient.Person (JOIN)
            .Include(p => p.Studies)    // load Patient.Studies (LEFT JOIN)
            .AsNoTracking()             // no change tracking → faster reads
            .ToListAsync();
    }

    public async Task<Patient?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Person)
            .Include(p => p.Studies)
            .FirstOrDefaultAsync(p => p.PatientId == id);
    }
}
```

**Explain these:**
- `.Include(p => p.Person)` → eager-load the related Person in the same query (generates a `JOIN`). Without it, `patient.Person` would be null and the mapper would crash.
- `.Include(p => p.Studies)` → loads the collection of studies.
- `.AsNoTracking()` → tells EF not to track these read-only objects → less memory, faster. Only safe because the objects won't be updated.
- `.FirstOrDefaultAsync(predicate)` → returns the first match or `null`.

**DoctorRepository** is identical to PatientRepository but for Doctors.

**StudyRepository** uses `ThenInclude` to reach two levels deep:

```csharp
.Include(s => s.Patient).ThenInclude(p => p.Person)  // Study → Patient → Person
.Include(s => s.Doctor).ThenInclude(d => d.Person)   // Study → Doctor → Person
```

`ThenInclude` follows the next navigation level — this is how you eagerly load a chain of relationships in one query.

### 9.4 Why both generic and specific?

The generic repository handles the 80% (plain CRUD). The specific interfaces add the 20% (custom queries with includes, filters). `PatientRepository : GenericRepository<Patient>, IPatientRepository` means it gets BOTH the generic methods and its own — classic **generic + inheritance** pattern.

---

## 10. DTO Layer & Mappers

**DTO = Data Transfer Object** — plain classes that define the exact shape of JSON sent over HTTP. They exist because:

1. **Security** — never expose the entity (DB shape) directly; only what the client needs.
2. **Control** — we can flatten nested objects (e.g., put `FirstName` on a PatientDto instead of `Person.FirstName`).
3. **Separation** — entities are for EF; DTOs are for the wire.

### 10.1 `DTOs/PersonDto.cs`

```csharp
public class PersonDto      // output: what the API sends back
{
    public int PersonId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class PersonCreateDto   // input: what the client sends to create
{
    [Required][MaxLength(100)] public string FirstName ...
    [Required][MaxLength(100)] public string LastName ...
    [Required] public DateTime DateOfBirth ...
    [Required][MaxLength(20)] public string Gender ...
    [MaxLength(20)] public string? Phone ...
    [EmailAddress][MaxLength(150)] public string? Email ...
}

public static class PersonMapper
{
    public static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}
```

**Explain `CalculateAge`:**
- Start with `currentYear - birthYear`.
- If the birthday hasn't happened *yet this year*, subtract 1.
- `today.AddYears(-age)` gives the date `age` years ago; if the raw birthday is still in the future relative to that, the person is one year younger than the naive difference. This handles **leap years and birthdays correctly** (no `DateTime` arithmetic errors like `DateTime.Now - dob / 365`).

### 10.2 `DTOs/PatientDtos.cs`

```csharp
public class PatientDto          // output shape the browser receives
{
    public int PatientId { get; set; }
    public int PersonId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int Age => PersonMapper.CalculateAge(DateOfBirth);  // computed, not stored
    public string Gender { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string MRN { get; set; }
    public string Status { get; set; }
    public int StudiesCount { get; set; }
}
```

**Explain key points:**
- The DTO **flattens** the Person/Patient split into one object (FirstName, LastName... + MRN, Status) — the browser doesn't care about the DB schema.
- `Age` is a **computed expression-bodied property** — not stored in DB, calculated on the fly. JSON serialization will include it automatically.
- `StudiesCount` → count of the patient's studies (fed by the mapper).

```csharp
public class PatientCreateDto   // input for POST
{
    [Required][MaxLength(100)] public string FirstName ...
    // ...person fields...
    [Required][MaxLength(50)] public string MRN ...
    [Required][MaxLength(50)] public string Status ...
}
public class PatientUpdateDto : PatientCreateDto { }   // input for PUT (same shape)
```

`PatientUpdateDto : PatientCreateDto` → inheritance reuse; update accepts the same fields. This is a simplification (real apps often make update fields optional).

**The mapper (entity ⇄ DTO conversion):**

```csharp
public static class PatientMapper
{
    public static PatientDto ToDto(Patient patient)       // entity → output DTO
    {
        return new PatientDto
        {
            PatientId = patient.PatientId,
            PersonId = patient.PersonId,
            FirstName = patient.Person.FirstName,         // requires Person loaded!
            ...
            StudiesCount = patient.Studies?.Count ?? 0    // null-safe count
        };
    }

    public static Patient ToEntity(PatientCreateDto dto)  // input DTO → entity
    {
        return new Patient
        {
            MRN = dto.MRN,
            Status = dto.Status,
            Person = new Person { ... }                    // builds Person too
        };
    }

    public static void ApplyUpdate(Patient patient, PatientUpdateDto dto) // copy fields onto existing entity
    {
        patient.MRN = dto.MRN;
        patient.Status = dto.Status;
        patient.Person.FirstName = dto.FirstName;
        // ... etc
    }
}
```

- `ToDto` **requires** `patient.Person` to be loaded (hence the `GetAllWithDetailsAsync` / includes). If it were null, you'd get a `NullReferenceException`.
- `patient.Studies?.Count ?? 0` → safe navigation: if `Studies` is null use 0 (here Studies is initialized to an empty list, so it's belt-and-braces).
- `ApplyUpdate` **mutates** the existing tracked entity (including its `Person`). Because the entity is already tracked by EF, `UpdateAsync` just marks it Modified and `SaveChanges` persists *all* changed fields. Note `PatientUpdateDto` has no `PatientId`/`PersonId` — IDs come from the URL, not the body.

### 10.3 `DTOs/DoctorDtos.cs` and `DTOs/StudyDtos.cs`

- `DoctorDto` mirrors `PatientDto` but has `Specialty` instead of `MRN`.
- `StudyDto` is fully flattened and human-friendly:

```csharp
public class StudyDto
{
    public int StudyId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; }   // "John Doe"
    public string PatientMrn { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; }    // "Sarah Mitchell"
    public string Modality { get; set; }
    public DateTime StudyDate { get; set; }
    public string Status { get; set; }
}
```

The StudyMapper builds these from the deeply-included entities:

```csharp
PatientName = $"{study.Patient.Person.FirstName} {study.Patient.Person.LastName}",
DoctorName  = $"{study.Doctor.Person.FirstName} {study.Doctor.Person.LastName}",
```

String interpolation creates the display name — requires the `StudyRepository.GetAllWithDetailsAsync` two-level includes.

---

## 11. Controllers (REST API Endpoints)

All controllers share these attributes:

```csharp
[ApiController]            // automatic ModelState validation, automatic 400 on bad binding
[Route("api/[controller]")]// route = /api/patients, /api/doctors, /api/studies
[Produces("application/json")] // documents response content type
public class XController : ControllerBase { }
```

`ControllerBase` → gives access to `Ok()`, `NotFound()`, `BadRequest()`, `NoContent()`, `CreatedAtAction()`, and the `ModelState` property.

### 11.1 `PatientsController` — endpoints

| Method | Route | Function | Returns |
|---|---|---|---|
| GET | `/api/patients` | All patients with details | 200 + `PatientDto[]` |
| GET | `/api/patients/{id}` | One patient | 200 / 404 |
| POST | `/api/patients` | Create patient | 201 + Location header |
| PUT | `/api/patients/{id}` | Update patient | 204 / 404 / 400 |
| DELETE | `/api/patients/{id}` | Delete patient + studies | 204 / 404 |

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatients()
{
    var patients = await _patientRepository.GetAllWithDetailsAsync();
    return Ok(patients.Select(PatientMapper.ToDto));
}
```

`patients.Select(PatientMapper.ToDto)` → LINQ `Select` projects every entity to a DTO (method-group syntax). Because `IEnumerable` is lazy, the mapping runs when JSON serialization enumerates it.

```csharp
[HttpGet("{id:int}")]
public async Task<ActionResult<PatientDto>> GetPatient(int id)
{
    var patient = await _patientRepository.GetByIdWithDetailsAsync(id);
    if (patient is null)
        return NotFound(new { message = $"Patient with id {id} was not found." });
    return Ok(PatientMapper.ToDto(patient));
}
```

- `{id:int}` → route constraint: only matches if the segment is an integer.
- The anonymous object `new { message = ... }` becomes JSON `{"message":"..."}` — the frontend reads `data.message` to show the error toast.
- 404 vs 200 based on null check.

```csharp
[HttpPost]
public async Task<ActionResult<PatientDto>> CreatePatient([FromBody] PatientCreateDto dto)
{
    if (!ModelState.IsValid) return BadRequest(ModelState);   // validation failed

    var patient = await _patientRepository.AddAsync(PatientMapper.ToEntity(dto));
    var saved = await _patientRepository.GetByIdWithDetailsAsync(patient.PatientId);
    return CreatedAtAction(nameof(GetPatient), new { id = patient.PatientId },
                           PatientMapper.ToDto(saved ?? patient));
}
```

- `[FromBody]` → deserialize the JSON body into the DTO.
- `ModelState.IsValid` → checks all the DataAnnotations (`[Required]`, `[MaxLength]`, `[EmailAddress]`). With `[ApiController]`, this check is automatic — the manual check here is redundant but explicit.
- `AddAsync` inserts and `SaveChanges` assigns the new ID.
- **Re-fetch with details** (`GetByIdWithDetailsAsync`) so we can return a fully-populated DTO.
- `CreatedAtAction(nameof(GetPatient), new { id = ... }, body)` → 201 + `Location: /api/patients/{id}` + the created object. `nameof(GetPatient)` keeps the link string compile-safe.

```csharp
[HttpPut("{id:int}")]
public async Task<IActionResult> UpdatePatient(int id, [FromBody] PatientUpdateDto dto)
{
    if (!ModelState.IsValid) return BadRequest(ModelState);

    var patient = await _patientRepository.GetByIdWithDetailsAsync(id);
    if (patient is null) return NotFound(...);

    PatientMapper.ApplyUpdate(patient, dto);        // copy DTO fields onto entity
    await _patientRepository.UpdateAsync(patient);  // save changes
    return NoContent();                             // 204 = success, no body
}
```

- `UpdateAsync` re-saves the whole (tracked) entity graph, including the nested `Person`, because EF tracks it all.

```csharp
[HttpDelete("{id:int}")]
public async Task<IActionResult> DeletePatient(int id)
{
    var patient = await _patientRepository.GetByIdAsync(id);
    if (patient is null) return NotFound(...);
    await _patientRepository.DeleteAsync(patient);  // cascades to Studies
    return NoContent();
}
```

`DeleteAsync(patient)` removes the Patient; the **cascade delete** in the DB wipes their Studies.

### 11.2 `DoctorsController` — endpoints

| Method | Route | Function | Returns |
|---|---|---|---|
| GET | `/api/doctors` | All doctors | 200 |
| GET | `/api/doctors/{id}` | One doctor | 200 / 404 |
| POST | `/api/doctors` | Create doctor | 201 |
| PUT | `/api/doctors/{id}` | Update doctor | 204 / 404 / 400 |
| DELETE | `/api/doctors/{id}` | Delete doctor (only if no studies) | 204 / 404 / **409** |

The DELETE is the interesting one — it has **business-rule protection**:

```csharp
[HttpDelete("{id:int}")]
public async Task<IActionResult> DeleteDoctor(int id)
{
    var doctor = await _doctorRepository.GetByIdWithDetailsAsync(id);
    if (doctor is null) return NotFound(...);

    if (doctor.Studies is { Count: > 0 })          // has studies?
        return Conflict(new { message = $"Doctor with id {id} has {doctor.Studies.Count} study(ies) and cannot be deleted." });

    await _doctorRepository.DeleteAsync(doctor);
    return NoContent();
}
```

- `doctor.Studies is { Count: > 0 }` → **property pattern matching** (C#): "if Studies is not null and Count is greater than 0".
- Returns **409 Conflict** instead of deleting, because the DB has `DeleteBehavior.Restrict` on `Doctor → Study`. So this check mirrors the DB constraint and gives the user a friendly message.

### 11.3 `StudiesController` — endpoints

| Method | Route | Function | Returns |
|---|---|---|---|
| GET | `/api/studies?patientId=&doctorId=` | All studies, optional filters | 200 |
| GET | `/api/studies/{id}` | One study | 200 / 404 |
| POST | `/api/studies` | Create study (validates FK existence) | 201 / 400 |
| PUT | `/api/studies/{id}` | Update study (validates FK existence) | 204 / 400 / 404 |
| DELETE | `/api/studies/{id}` | Delete study | 204 / 404 |

This controller injects **three** repositories (constructor injection) because a Study needs to verify its patient and doctor exist:

```csharp
public StudiesController(IStudyRepository studyRepository,
                         IPatientRepository patientRepository,
                         IDoctorRepository doctorRepository) { ... }
```

**Filtering:**

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<StudyDto>>> GetStudies([FromQuery] int? patientId, [FromQuery] int? doctorId)
{
    var studies = await _studyRepository.GetAllWithDetailsAsync();

    if (patientId.HasValue)
        studies = studies.Where(s => s.PatientId == patientId.Value);
    if (doctorId.HasValue)
        studies = studies.Where(s => s.DoctorId == doctorId.Value);

    return Ok(studies.Select(StudyMapper.ToDto));
}
```

- `[FromQuery] int?` → optional nullable query-string parameters. `?patientId=3&doctorId=1`.
- **Note**: filtering happens **in memory** after the full query. The `Where` runs in C#, not SQL. (Discussion point — for large tables you'd want `IQueryable` filtering to push the WHERE to the DB.)

**Referential integrity validation on create:**

```csharp
if (!await _patientRepository.ExistsAsync(dto.PatientId))
    return BadRequest(new { message = $"Patient with id {dto.PatientId} does not exist." });
if (!await _doctorRepository.ExistsAsync(dto.DoctorId))
    return BadRequest(new { message = $"Doctor with id {dto.DoctorId} does not exist." });
```

Prevents inserting a study with a patient/doctor ID that doesn't exist (otherwise SQLite would throw an FK constraint error → 500). Returns a clean **400** instead.

### 11.4 HTTP status codes used across the API

| Code | Meaning | Used when |
|---|---|---|
| 200 OK | Success with body | GET |
| 201 Created | Success + Location header | POST |
| 204 No Content | Success, no body | PUT / DELETE |
| 400 Bad Request | Invalid data / FK doesn't exist | POST / PUT |
| 404 Not Found | Id not found | any {id} route |
| 409 Conflict | Rule violation (delete doctor w/ studies) | DELETE doctor |

---

## 12. Frontend (wwwroot)

The frontend is a **single-page application** (SPA) served as static files. It uses Bootstrap 5 for layout/modals, DataTables for the tables, and the native `fetch` API to talk to the backend. No build step, no framework.

### 12.1 `wwwroot/index.html` — structure

The page is composed of:

1. **Navbar** — brand + live date/time (`#navDateTime`).
2. **Tab navigation** (`#mainTabs`) — three pills: **Studies**, **Patients**, **Doctors**. Bootstrap *pills* switch between three panes.
3. **Three cards**, each containing a table:
   - `#studiesTable` — columns: ID, Patient, MRN, Modality, Date, Doctor, Actions.
   - `#patientsTable` — ID, Name, MRN, Age, Gender, Studies, Actions.
   - `#doctorsTable` — ID, Name, Specialty, Age, Gender, Phone, Email, Studies, Actions.
4. **Toast** (`#appToast`) — Bootstrap toast notifications for success/error messages.
5. **Modals** (forms):
   - `#patientModal` — Add/Edit patient (name, DOB, gender, phone, email, MRN). The Status is *not* an input — it's tracked internally in JS (`currentPatientStatus`) and defaults to `Active`.
   - `#doctorModal` — Add/Edit doctor (name, DOB, gender, phone, email, specialty dropdown).
   - `#studyModal` — Add/Edit study (patient dropdown, doctor dropdown, modality dropdown, date).
   - `#viewStudiesModal` — shows all studies for one patient (read + delete).

**CDN dependencies loaded at the bottom:**
```html
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
<script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
<script src="https://cdn.datatables.net/1.13.7/js/jquery.dataTables.min.js"></script>
<script src="https://cdn.datatables.net/1.13.7/js/dataTables.bootstrap5.min.js"></script>
<script src="app.js"></script>
```

DataTables requires jQuery (hence both). Bootstrap bundle includes Popper for tooltips/dropdowns. Everything from a CDN (needs internet; discussion point for offline use).

### 12.2 `wwwroot/app.js` — all the frontend logic

#### API base + fetch helper

```js
const API = {
  patients: '/api/patients',
  doctors: '/api/doctors',
  studies: '/api/studies'
};
```

```js
function apiFetch(url, method, body) {
  const opts = { method, headers: { 'Content-Type': 'application/json' } };
  if (body !== undefined) opts.body = JSON.stringify(body);
  return fetch(url, opts).then(async (res) => {
    if (!res.ok) {
      let data = null;
      try { data = await res.json(); } catch (e) { }
      let msg = 'Request failed (' + res.status + ')';
      if (data) {
        if (typeof data === 'string' && data) msg = data;
        else if (data.message) msg = data.message;          // our API's error shape
        else if (data.errors && typeof data.errors === 'object')
          msg = Object.values(data.errors).flat().filter(Boolean).join(' ');  // ModelState errors
      }
      throw new Error(msg || 'Unknown error');
    }
    return res.status === 204 ? null : res.json();          // 204 has no body
  });
}
```

**Explain:**
- One generic function for ALL HTTP calls (CRUD + GET).
- Unwraps the error body to show a friendly message — handles three shapes: plain string, `{ message }` (our controllers), and `{ errors }` (ModelState validation errors from `BadRequest(ModelState)`).
- `204` (No Content) has no body → return `null` instead of trying `.json()` (which would throw).
- Throws `Error(msg)` so callers use `.catch`/`try-catch`.

#### Small helper functions

```js
function showToast(message, type) {
  const toastEl = document.getElementById('appToast');
  toastEl.className = 'toast align-items-center border-0 text-bg-' + (type || 'success');
  document.getElementById('appToastBody').textContent = message;
  bootstrap.Toast.getOrCreateInstance(toastEl, { delay: 3500 }).show();
}
```
- Toast with color coding (`success`/`danger`) and 3.5s auto-dismiss. Uses Bootstrap's global `bootstrap` object.

```js
function escapeHtml(value) { /* escapes & < > " ' */ }
```
- **Prevents XSS** — any data coming from the API is escaped before injecting into `innerHTML` (data could contain `<script>` etc.). Good security discussion point.

```js
function formatDate(value) { ... }   // ISO → YYYY-MM-DD for date inputs / display
function fullName(first, last) { return ((first||'')+' '+(last||'')).trim() || '-'; }
function todayStr() { ... }          // today as YYYY-MM-DD
```

```js
function statusBadge(status) {
  var map = { 'Active':'success', 'Pending':'warning', 'Inactive':'secondary', ... };
  return '<span class="badge bg-' + (map[status] || 'secondary') + '">' + escapeHtml(status) + '</span>';
}
```
- Maps a status string to a Bootstrap color. **Note**: `statusBadge` is defined but not actually used in the current tables (the tables show `studiesCount` badge, and statuses are hidden in study/patient tables). Fun observation for discussion — dead code.

**Live clock:** an IIFE `(function updateDateTime(){...})()` sets the navbar clock and refreshes every 30s with `setInterval`.

#### DataTables definitions

```js
var patientsTable = new DataTable('#patientsTable', {
  ajax: { url: API.patients, dataSrc: '' },   // fetch data from /api/patients
  columns: [
    { data: 'patientId' },                    // binds to JSON property
    { data: null, render: function(d) { return escapeHtml(fullName(d.firstName, d.lastName)); } },
    { data: 'mrn', render: ... },
    { data: 'age' },
    { data: 'gender' },
    { data: 'studiesCount', render: function(d) { return '<span class="badge bg-secondary">' + d + '</span>'; } },
    { data: null, orderable: false, className: 'text-end',
      render: function(d) {
        return '<div class="d-flex justify-content-end gap-1">' +
          '<button ... onclick="viewPatientStudies(' + d.patientId + ')">Studies</button>' +
          '<button ... onclick="editPatientClick(' + d.patientId + ')">Edit</button>' +
          '<button ... onclick="deletePatientClick(' + d.patientId + ')">Delete</button>' +
          '</div>';
      } }
  ],
  order: [[0, 'desc']],   // sort by ID descending (newest first)
  language: { emptyTable: 'No patients found.' },
  responsive: true
});
```

**Explain:**
- `ajax: { url, dataSrc: '' }` → DataTables calls `GET /api/patients` itself. Our API returns a **plain array**, so `dataSrc: ''` tells it the array is at the root (by default DataTables expects `{ data: [...] }`).
- `{ data: 'patientId' }` → render the JSON property directly in that column.
- `{ data: null, render: fn }` → build the cell from the whole row object `d`. The `render` callback receives the row's data.
- Action buttons use **inline `onclick` handlers** that call global functions — simple, but a discussion point (global scope; an SPA framework or event delegation would be cleaner).
- `order: [[0,'desc']]` → initial sort on first column descending.

`doctorsTable` and `studiesTable` follow the same pattern. Studies table sorts by date desc (`order: [[4,'desc']]`).

#### Dropdown loaders

```js
async function loadDoctorsDropdown(selectId, selectedId) {
  var doctors = await apiFetch(API.doctors, 'GET');
  document.getElementById(selectId).innerHTML =
    '<option value="">Select doctor</option>' +
    doctors.map(d => '<option value="' + d.doctorId + '"' + (String(d.doctorId)===String(selectedId)?' selected':'') + '>' +
      escapeHtml(fullName(d.firstName, d.lastName)) + ' (' + escapeHtml(d.specialty) + ')</option>').join('');
}
```
- Fetches all doctors/patients, builds `<option>` elements, preselects the one passed as `selectedId` (used when editing). String comparison avoids type mismatch (number vs string).

#### Patient CRUD (frontend)

```js
function openPatientModal(patient) {
  form.reset();                        // clear fields
  form.classList.remove('was-validated');
  document.getElementById('patientModalTitle').textContent = patient ? 'Edit Patient' : 'Add Patient';
  ...
  if (patient) { fill all fields from the DTO; currentPatientStatus = patient.status; }
  else { editPatientId = ''; currentPatientStatus = 'Active'; }
  bootstrap.Modal.getOrCreateInstance(document.getElementById('patientModal')).show();
}
```
- Same modal for **Add** and **Edit** — `patient` being null means Add.
- Hidden input `#editPatientId` stores the ID being edited (empty = creating).
- Status isn't in the form; JS tracks it via `currentPatientStatus` (preserved on edit, defaults to `Active` on add).

```js
document.getElementById('patientForm').addEventListener('submit', async function(e) {
  e.preventDefault();
  if (!form.checkValidity()) { form.classList.add('was-validated'); return; }  // HTML5 validation
  var payload = { firstName, lastName, dateOfBirth, gender, phone||null, email||null, mrn, status: currentPatientStatus };
  try {
    var id = document.getElementById('editPatientId').value;
    if (id) { await apiFetch(API.patients+'/'+id, 'PUT', payload); showToast('Patient updated successfully.'); }
    else    { await apiFetch(API.patients, 'POST', payload); showToast('Patient added successfully.'); }
    bootstrap.Modal.getInstance(...).hide();
    patientsTable.ajax.reload();     // refresh table from server
    studiesTable.ajax.reload();      // studies list may reference patient names
  } catch (err) { showToast(err.message, 'danger'); }
});
```

- `checkValidity()` → native browser HTML5 validation (uses the `required` attributes in the HTML).
- `.trim() || null` → empty optional fields become `null` (matches DTO `string?`).
- On success: hide modal, **reload both tables** (a patient name change affects studies' `patientName`).
- `catch` shows the server error message in a red toast.

`editPatientClick`/`deletePatientClick` are straightforward GET/DELETE wrappers; delete uses `confirm()`.

#### Doctor CRUD
Same pattern as patients, minus the status tracking, plus a specialty dropdown.

#### Study CRUD

```js
async function openStudyModal(study) {
  ...
  await loadPatientsDropdown('studyPatient', study ? study.patientId : null);
  await loadDoctorsDropdown('studyModalDoctor', study ? study.doctorId : null);
  if (study) { set modality/date; currentStudyStatus = study.status; }
  else { studyModalDate = todayStr(); currentStudyStatus = 'Scheduled'; }
  ...show modal...
}
```

- The modal is populated **async** — it must fetch patients + doctors to fill the two dropdowns **before** the user can pick. Hence `await`.
- Study status also isn't a form field — defaults to `Scheduled`, preserved on edit.

Submit handler sends `patientId`, `doctorId` (parsed with `parseInt`), `modality`, `studyDate`, `status`; reloads studies + patients tables after save.

#### View patient studies modal

```js
function viewPatientStudies(patientId) {
  currentViewPatientId = patientId;
  var rows = patientsTable.rows().data().toArray();      // grab from loaded table data
  var p = rows.find(r => r.patientId === patientId);     // find the patient row
  document.getElementById('viewStudiesPatientName').textContent = p ? fullName(p.firstName,p.lastName)+' ('+p.mrn+')' : '#'+patientId;
  loadViewStudies(patientId);
  bootstrap.Modal.getOrCreateInstance(document.getElementById('viewStudiesModal')).show();
}

function loadViewStudies(patientId) {
  apiFetch(API.studies + '?patientId=' + patientId, 'GET')   // uses the query filter!
    .then(studies => { build tbody rows with Delete buttons });
}
```

- Reads the patient's display name from **already-loaded DataTables data** (no extra fetch).
- Calls the **server-side filter** `GET /api/studies?patientId=N` (this is why `StudiesController.GetStudies` supports the query params).
- Renders a simple table inside the modal; each row has a Delete button calling `deleteViewStudy(studyId)` which deletes then reloads everything.

### 12.3 `wwwroot/style.css`

Custom styling layered on Bootstrap:
- Light background (`#f0f4f8`), system font stack.
- **Navbar** — dark blue gradient (`#1a365d → #2d5986`).
- **Tab pills** — light gray default, purple-blue gradient when active (`#667eea → #764ba2`).
- **Cards** — rounded, no border, clean header with gradient button.
- **Table headers** — uppercase, letter-spaced, small.
- **Modals** — gradient headers with white close button.
- Smaller rounded action buttons, styled toasts.

The palette is consistent (blue/purple gradients) — a nice design discussion point.

---

## 13. Configuration Files

### 13.1 `appsettings.json` (base config)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=PatientWorklist.db"
  },
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*"
}
```

- `DefaultConnection` → SQLite connection string; `Data Source=PatientWorklist.db` points to the DB file in the project root. (`builder.Configuration.GetConnectionString("DefaultConnection")` in Program.cs reads this exact key.)
- `Logging` → levels; `Microsoft.AspNetCore: Warning` silences the noisy framework logs.
- `AllowedHosts: "*"` → the host accepts requests from any host header.

### 13.2 `appsettings.Development.json` (overrides in Development)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

The extra line `Microsoft.EntityFrameworkCore.Database.Command: Information` makes EF Core **log every SQL command** — that's the SQL you saw in the console when we ran the app. Dev-only (via the `ASPNETCORE_ENVIRONMENT=Development` variable), which is why it's in the Development file.

### 13.3 `Properties/launchSettings.json`

Defines launch profiles (used by `dotnet run`):

- **http** profile → `http://localhost:5068`, opens browser at `/swagger`, env = Development.
- **https** profile → `https://localhost:7216` + `http://localhost:5068`.
- **IIS Express** profile → Windows-only, port 15680/44378.

When we ran it, it listened on `http://localhost:5068`.

---

## 14. Key Concepts to Discuss

Here are the important technical points — great for a presentation or exam discussion:

1. **Layered architecture / Separation of concerns**
   Controllers (HTTP) → Repositories (data access) → EF Core → SQLite. Each layer has one job.

2. **Repository Pattern**
   Wraps data access behind interfaces (`IRepository<T>`), so controllers depend on abstractions, not on EF directly → easier to unit-test with fakes.

3. **Generic Repository with inheritance**
   `GenericRepository<T>` handles CRUD for any entity; specific repos extend it with custom includes.

4. **Unit of Work**
   EF's `DbContext` tracks entities; `SaveChangesAsync()` commits everything at once as one transaction.

5. **Eager loading with `.Include()` / `.ThenInclude()`**
   Necessary to avoid lazy-loading N+1 problems and null refs in mappers.

6. **DTOs vs Entities**
   DTOs control the API contract, flatten nesting, hide internals. Mapping via static `*Mapper` classes (AutoMapper could be an alternative — discussion point).

7. **DataAnnotations vs Fluent API**
   Both used here for validation/schema — attributes drive `ModelState` validation, fluent config drives the actual DB schema. Slight duplication (could be a discussion point).

8. **Delete behaviors**
   Cascade (Person→Patient, Patient→Studies) vs Restrict (Doctor→Studies) — and how the API mirrors the DB rule with a 409.

9. **`EnsureCreated()` vs EF Migrations**
   `EnsureCreated` only creates the schema if the DB doesn't exist; it doesn't handle schema evolution. Migrations are the production-grade alternative.

10. **Async/await everywhere**
    Non-blocking I/O for scalability; `Task<T>` return types.

11. **In-memory filtering in StudiesController**
    `Where(...)` runs in C# after the full table loads. Better: filter via `IQueryable` so SQL does it. Scalability discussion point.

12. **CORS open policy (`AllowAnyOrigin`)**
    Fine for dev/demo; in production restrict origins.

13. **Frontend is vanilla JS + jQuery + DataTables**
    No framework. Inline `onclick` handlers + global state (`currentPatientStatus`) — simple but not very scalable/maintainable.

14. **XSS defense via `escapeHtml`**
    All dynamic data is escaped before `innerHTML`.

15. **Client + server validation**
    HTML5 `checkValidity()` on the front; DataAnnotations + `ModelState` on the back.

16. **HTTP status codes as an API contract**
    200/201/204/400/404/409 with consistent `{ message }` error JSON the frontend understands.

---

## 15. How to Run the Project

```bash
# In the project folder:
dotnet run
```

- Dev URL: http://localhost:5068 (root serves the frontend at `/`, API at `/api/...`, Swagger at `/swagger`).

**On this machine** only the .NET 8 runtime is installed, and the project targets net7.0. Run with roll-forward:

```bash
DOTNET_ROLL_FORWARD=LatestMajor dotnet run
```

**First run** creates `PatientWorklist.db` and seeds demo data (3 doctors, 5 patients, 5 studies). To reset the data, delete the `.db`/`.db-shm`/`.db-wal` files and run again.

---

### Quick API reference (test with Swagger at `/swagger`)

| Resource | Endpoint | Methods |
|---|---|---|
| Patients | `/api/patients` | GET, POST |
| Patients | `/api/patients/{id}` | GET, PUT, DELETE |
| Doctors | `/api/doctors` | GET, POST |
| Doctors | `/api/doctors/{id}` | GET, PUT, DELETE |
| Studies | `/api/studies` | GET, POST |
| Studies | `/api/studies/{id}` | GET, PUT, DELETE |
| Studies (filter) | `/api/studies?patientId=N&doctorId=M` | GET |
