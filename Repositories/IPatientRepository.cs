using PatientWorklist.API.Entities;

namespace PatientWorklist.API.Repositories;

public interface IPatientRepository : IRepository<Patient>
{
    Task<IEnumerable<Patient>> GetAllWithDetailsAsync();
    Task<Patient?> GetByIdWithDetailsAsync(int id);
}