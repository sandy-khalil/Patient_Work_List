using Microsoft.AspNetCore.Mvc;
using PatientWorklist.API.DTOs;
using PatientWorklist.API.Entities;
using PatientWorklist.API.Repositories;

namespace PatientWorklist.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PatientsController : ControllerBase
{
    private readonly IPatientRepository _patientRepository;

    public PatientsController(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatients()
    {
        var patients = await _patientRepository.GetAllWithDetailsAsync();
        return Ok(patients.Select(PatientMapper.ToDto));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PatientDto>> GetPatient(int id)
    {
        var patient = await _patientRepository.GetByIdWithDetailsAsync(id);
        if (patient is null)
        {
            return NotFound(new { message = $"Patient with id {id} was not found." });
        }

        return Ok(PatientMapper.ToDto(patient));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PatientDto>> CreatePatient([FromBody] PatientCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var patient = await _patientRepository.AddAsync(PatientMapper.ToEntity(dto));
        var saved = await _patientRepository.GetByIdWithDetailsAsync(patient.PatientId);
        return CreatedAtAction(nameof(GetPatient), new { id = patient.PatientId }, PatientMapper.ToDto(saved ?? patient));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePatient(int id, [FromBody] PatientUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var patient = await _patientRepository.GetByIdWithDetailsAsync(id);
        if (patient is null)
        {
            return NotFound(new { message = $"Patient with id {id} was not found." });
        }

        PatientMapper.ApplyUpdate(patient, dto);
        await _patientRepository.UpdateAsync(patient);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePatient(int id)
    {
        var patient = await _patientRepository.GetByIdAsync(id);
        if (patient is null)
        {
            return NotFound(new { message = $"Patient with id {id} was not found." });
        }

        await _patientRepository.DeleteAsync(patient);
        return NoContent();
    }
}