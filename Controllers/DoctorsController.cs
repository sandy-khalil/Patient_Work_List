using Microsoft.AspNetCore.Mvc;
using PatientWorklist.API.DTOs;
using PatientWorklist.API.Repositories;

namespace PatientWorklist.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorRepository _doctorRepository;

    public DoctorsController(IDoctorRepository doctorRepository)
    {
        _doctorRepository = doctorRepository;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DoctorDto>>> GetDoctors()
    {
        var doctors = await _doctorRepository.GetAllWithDetailsAsync();
        return Ok(doctors.Select(DoctorMapper.ToDto));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorDto>> GetDoctor(int id)
    {
        var doctor = await _doctorRepository.GetByIdWithDetailsAsync(id);
        if (doctor is null)
        {
            return NotFound(new { message = $"Doctor with id {id} was not found." });
        }

        return Ok(DoctorMapper.ToDto(doctor));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DoctorDto>> CreateDoctor([FromBody] DoctorCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var doctor = await _doctorRepository.AddAsync(DoctorMapper.ToEntity(dto));
        var saved = await _doctorRepository.GetByIdWithDetailsAsync(doctor.DoctorId);
        return CreatedAtAction(nameof(GetDoctor), new { id = doctor.DoctorId }, DoctorMapper.ToDto(saved ?? doctor));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDoctor(int id, [FromBody] DoctorUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var doctor = await _doctorRepository.GetByIdWithDetailsAsync(id);
        if (doctor is null)
        {
            return NotFound(new { message = $"Doctor with id {id} was not found." });
        }

        DoctorMapper.ApplyUpdate(doctor, dto);
        await _doctorRepository.UpdateAsync(doctor);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteDoctor(int id)
    {
        var doctor = await _doctorRepository.GetByIdWithDetailsAsync(id);
        if (doctor is null)
        {
            return NotFound(new { message = $"Doctor with id {id} was not found." });
        }

        if (doctor.Studies is { Count: > 0 })
        {
            return Conflict(new { message = $"Doctor with id {id} has {doctor.Studies.Count} study(ies) and cannot be deleted." });
        }

        await _doctorRepository.DeleteAsync(doctor);
        return NoContent();
    }
}