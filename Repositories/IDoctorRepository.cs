using PatientWorklist.API.Entities;

namespace PatientWorklist.API.Repositories;

public interface IDoctorRepository : IRepository<Doctor>
{
    Task<IEnumerable<Doctor>> GetAllWithDetailsAsync();
    Task<Doctor?> GetByIdWithDetailsAsync(int id);
}