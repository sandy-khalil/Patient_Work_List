using Microsoft.EntityFrameworkCore;
using PatientWorklist.API.Data;
using PatientWorklist.API.Entities;

namespace PatientWorklist.API.Repositories;

public class StudyRepository : GenericRepository<Study>, IStudyRepository
{
    public StudyRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Study>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(s => s.Patient).ThenInclude(p => p.Person)
            .Include(s => s.Doctor).ThenInclude(d => d.Person)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Study?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(s => s.Patient).ThenInclude(p => p.Person)
            .Include(s => s.Doctor).ThenInclude(d => d.Person)
            .FirstOrDefaultAsync(s => s.StudyId == id);
    }
}