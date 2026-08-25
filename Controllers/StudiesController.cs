using Microsoft.AspNetCore.Mvc;
using PatientWorklist.API.DTOs;
using PatientWorklist.API.Repositories;

namespace PatientWorklist.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StudiesController : ControllerBase
{
    private readonly IStudyRepository _studyRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IDoctorRepository _doctorRepository;

    public StudiesController(
        IStudyRepository studyRepository,
        IPatientRepository patientRepository,
        IDoctorRepository doctorRepository)
    {
        _studyRepository = studyRepository;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StudyDto>>> GetStudies([FromQuery] int? patientId, [FromQuery] int? doctorId)
    {
        var studies = await _studyRepository.GetAllWithDetailsAsync();

        if (patientId.HasValue)
        {
            studies = studies.Where(s => s.PatientId == patientId.Value);
        }

        if (doctorId.HasValue)
        {
            studies = studies.Where(s => s.DoctorId == doctorId.Value);
        }

        return Ok(studies.Select(StudyMapper.ToDto));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudyDto>> GetStudy(int id)
    {
        var study = await _studyRepository.GetByIdWithDetailsAsync(id);
        if (study is null)
        {
            return NotFound(new { message = $"Study with id {id} was not found." });
        }

        return Ok(StudyMapper.ToDto(study));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StudyDto>> CreateStudy([FromBody] StudyCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!await _patientRepository.ExistsAsync(dto.PatientId))
        {
            return BadRequest(new { message = $"Patient with id {dto.PatientId} does not exist." });
        }

        if (!await _doctorRepository.ExistsAsync(dto.DoctorId))
        {
            return BadRequest(new { message = $"Doctor with id {dto.DoctorId} does not exist." });
        }

        var study = await _studyRepository.AddAsync(StudyMapper.ToEntity(dto));
        var saved = await _studyRepository.GetByIdWithDetailsAsync(study.StudyId);
        return CreatedAtAction(nameof(GetStudy), new { id = study.StudyId }, StudyMapper.ToDto(saved ?? study));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStudy(int id, [FromBody] StudyUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var study = await _studyRepository.GetByIdAsync(id);
        if (study is null)
        {
            return NotFound(new { message = $"Study with id {id} was not found." });
        }

        if (!await _patientRepository.ExistsAsync(dto.PatientId))
        {
            return BadRequest(new { message = $"Patient with id {dto.PatientId} does not exist." });
        }

        if (!await _doctorRepository.ExistsAsync(dto.DoctorId))
        {
            return BadRequest(new { message = $"Doctor with id {dto.DoctorId} does not exist." });
        }

        StudyMapper.ApplyUpdate(study, dto);
        await _studyRepository.UpdateAsync(study);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStudy(int id)
    {
        var study = await _studyRepository.GetByIdAsync(id);
        if (study is null)
        {
            return NotFound(new { message = $"Study with id {id} was not found." });
        }

        await _studyRepository.DeleteAsync(study);
        return NoContent();
    }
}