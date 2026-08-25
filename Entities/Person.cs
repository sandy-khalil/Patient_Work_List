using System.ComponentModel.DataAnnotations;

namespace PatientWorklist.API.Entities;

public class Person
{
    [Key]
    public int PersonId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [MaxLength(20)]
    public string Gender { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [EmailAddress]
    [MaxLength(150)]
    public string? Email { get; set; }

    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
}
