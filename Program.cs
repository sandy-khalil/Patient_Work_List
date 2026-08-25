using Microsoft.EntityFrameworkCore;
using PatientWorklist.API.Data;
using PatientWorklist.API.Repositories;

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

// Serve the static frontend (wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();