using Microsoft.EntityFrameworkCore;
using PatientWorklist.API.Data;
using PatientWorklist.API.Entities;

namespace PatientWorklist.API.Repositories;

public class PatientRepository : GenericRepository<Patient>, IPatientRepository
{
    public PatientRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Patient>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(p => p.Person)
            .Include(p => p.Studies)
            .AsNoTracking()
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