using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientWorklist.API.Entities;

public class Patient
{
    [Key]
    public int PatientId { get; set; }

    [ForeignKey(nameof(Person))]
    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string MRN { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    public ICollection<Study> Studies { get; set; } = new List<Study>();
}
