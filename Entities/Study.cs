using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PatientWorklist.API.Entities;

public class Study
{
    [Key]
    public int StudyId { get; set; }

    [ForeignKey(nameof(Patient))]
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    [ForeignKey(nameof(Doctor))]
    public int DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Modality { get; set; } = string.Empty;

    public DateTime StudyDate { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
}
