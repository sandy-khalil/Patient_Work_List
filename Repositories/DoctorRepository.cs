using Microsoft.EntityFrameworkCore;
using PatientWorklist.API.Data;
using PatientWorklist.API.Entities;

namespace PatientWorklist.API.Repositories;

public class DoctorRepository : GenericRepository<Doctor>, IDoctorRepository
{
    public DoctorRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Doctor>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(d => d.Person)
            .Include(d => d.Studies)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Doctor?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(d => d.Person)
            .Include(d => d.Studies)
            .FirstOrDefaultAsync(d => d.DoctorId == id);
    }
}