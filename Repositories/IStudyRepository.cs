using PatientWorklist.API.Entities;

namespace PatientWorklist.API.Repositories;

public interface IStudyRepository : IRepository<Study>
{
    Task<IEnumerable<Study>> GetAllWithDetailsAsync();
    Task<Study?> GetByIdWithDetailsAsync(int id);
}