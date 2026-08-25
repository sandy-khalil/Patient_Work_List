using System.ComponentModel.DataAnnotations;
using PatientWorklist.API.Entities;

namespace PatientWorklist.API.DTOs;

public class StudyDto
{
    public int StudyId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientMrn { get; set; } = string.Empty;
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Modality { get; set; } = string.Empty;
    public DateTime StudyDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class StudyCreateDto
{
    [Required]
    public int PatientId { get; set; }

    [Required]
    public int DoctorId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Modality { get; set; } = string.Empty;

    [Required]
    public DateTime StudyDate { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
}

public class StudyUpdateDto : StudyCreateDto
{
}

public static class StudyMapper
{
    public static StudyDto ToDto(Study study)
    {
        return new StudyDto
        {
            StudyId = study.StudyId,
            PatientId = study.PatientId,
            PatientName = $"{study.Patient.Person.FirstName} {study.Patient.Person.LastName}",
            PatientMrn = study.Patient.MRN,
            DoctorId = study.DoctorId,
            DoctorName = $"{study.Doctor.Person.FirstName} {study.Doctor.Person.LastName}",
            Modality = study.Modality,
            StudyDate = study.StudyDate,
            Status = study.Status
        };
    }

    public static Study ToEntity(StudyCreateDto dto)
    {
        return new Study
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            Modality = dto.Modality,
            StudyDate = dto.StudyDate,
            Status = dto.Status
        };
    }

    public static void ApplyUpdate(Study study, StudyUpdateDto dto)
    {
        study.PatientId = dto.PatientId;
        study.DoctorId = dto.DoctorId;
        study.Modality = dto.Modality;
        study.StudyDate = dto.StudyDate;
        study.Status = dto.Status;
    }
}