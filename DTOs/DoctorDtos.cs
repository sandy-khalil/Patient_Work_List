using System.ComponentModel.DataAnnotations;
using PatientWorklist.API.Entities;

namespace PatientWorklist.API.DTOs;

public class DoctorDto
{
    public int DoctorId { get; set; }
    public int PersonId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int Age => PersonMapper.CalculateAge(DateOfBirth);
    public string Gender { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Specialty { get; set; } = string.Empty;
    public int StudiesCount { get; set; }
}

public class DoctorCreateDto
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
    [MaxLength(100)]
    public string Specialty { get; set; } = string.Empty;
}

public class DoctorUpdateDto : DoctorCreateDto
{
}

public static class DoctorMapper
{
    public static DoctorDto ToDto(Doctor doctor)
    {
        return new DoctorDto
        {
            DoctorId = doctor.DoctorId,
            PersonId = doctor.PersonId,
            FirstName = doctor.Person.FirstName,
            LastName = doctor.Person.LastName,
            DateOfBirth = doctor.Person.DateOfBirth,
            Gender = doctor.Person.Gender,
            Phone = doctor.Person.Phone,
            Email = doctor.Person.Email,
            Specialty = doctor.Specialty,
            StudiesCount = doctor.Studies?.Count ?? 0
        };
    }

    public static Doctor ToEntity(DoctorCreateDto dto)
    {
        return new Doctor
        {
            Specialty = dto.Specialty,
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

    public static void ApplyUpdate(Doctor doctor, DoctorUpdateDto dto)
    {
        doctor.Specialty = dto.Specialty;
        doctor.Person.FirstName = dto.FirstName;
        doctor.Person.LastName = dto.LastName;
        doctor.Person.DateOfBirth = dto.DateOfBirth;
        doctor.Person.Gender = dto.Gender;
        doctor.Person.Phone = dto.Phone;
        doctor.Person.Email = dto.Email;
    }
}