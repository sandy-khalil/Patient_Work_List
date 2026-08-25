using System.ComponentModel.DataAnnotations;
using PatientWorklist.API.Entities;

namespace PatientWorklist.API.DTOs;

public class PatientDto
{
    public int PatientId { get; set; }
    public int PersonId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int Age => PersonMapper.CalculateAge(DateOfBirth);
    public string Gender { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string MRN { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int StudiesCount { get; set; }
}

public class PatientCreateDto
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

    [Required]
    [MaxLength(50)]
    public string MRN { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
}

public class PatientUpdateDto : PatientCreateDto
{
}

public static class PatientMapper
{
    public static PatientDto ToDto(Patient patient)
    {
        return new PatientDto
        {
            PatientId = patient.PatientId,
            PersonId = patient.PersonId,
            FirstName = patient.Person.FirstName,
            LastName = patient.Person.LastName,
            DateOfBirth = patient.Person.DateOfBirth,
            Gender = patient.Person.Gender,
            Phone = patient.Person.Phone,
            Email = patient.Person.Email,
            MRN = patient.MRN,
            Status = patient.Status,
            StudiesCount = patient.Studies?.Count ?? 0
        };
    }

    public static Patient ToEntity(PatientCreateDto dto)
    {
        return new Patient
        {
            MRN = dto.MRN,
            Status = dto.Status,
            Person = new Person
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                Phone = dto.Phone,
                Email = dto.Email
            }
        };
    }

    public static void ApplyUpdate(Patient patient, PatientUpdateDto dto)
    {
        patient.MRN = dto.MRN;
        patient.Status = dto.Status;
        patient.Person.FirstName = dto.FirstName;
        patient.Person.LastName = dto.LastName;
        patient.Person.DateOfBirth = dto.DateOfBirth;
        patient.Person.Gender = dto.Gender;
        patient.Person.Phone = dto.Phone;
        patient.Person.Email = dto.Email;
    }
}