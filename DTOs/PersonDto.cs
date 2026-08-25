using System.ComponentModel.DataAnnotations;

namespace PatientWorklist.API.DTOs;

public class PersonDto
{
    public int PersonId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class PersonCreateDto
{
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
}

public static class PersonMapper
{
    public static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}