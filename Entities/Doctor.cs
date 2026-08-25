using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientWorklist.API.Entities;

public class Doctor
{
    [Key]
    public int DoctorId { get; set; }

    [ForeignKey(nameof(Person))]
    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Specialty { get; set; } = string.Empty;

    public ICollection<Study> Studies { get; set; } = new List<Study>();
}
