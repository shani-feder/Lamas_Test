using DataControlsAPI.DTOs;
using DataControlsAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataControlsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ControlsController : ControllerBase
{
    private readonly IControlService _controlService;
    private readonly IControlExecutionService _executionService;
    private readonly ISqlGeneratorService _sqlGenerator;

    public ControlsController(
        IControlService controlService,
        IControlExecutionService executionService,
        ISqlGeneratorService sqlGenerator)
    {
        _controlService = controlService;
        _executionService = executionService;
        _sqlGenerator = sqlGenerator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var controls = await _controlService.GetAllControlsAsync();
        return Ok(controls);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var control = await _controlService.GetControlByIdAsync(id);
        return control == null ? NotFound() : Ok(control);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ControlDto dto)
    {
        var id = await _controlService.CreateControlAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id }, new { controlId = id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ControlDto dto)
    {
        var success = await _controlService.UpdateControlAsync(id, dto);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _controlService.DeleteControlAsync(id);
        return success ? NoContent() : NotFound();
    }

    [HttpGet("operators")]
    public async Task<IActionResult> GetOperators()
    {
        var operators = await _controlService.GetOperatorsAsync();
        return Ok(operators);
    }

    [HttpPost("{id}/execute")]
    public async Task<IActionResult> Execute(int id)
    {
        var result = await _executionService.ExecuteControlAsync(id);
        return Ok(result);
    }

    [HttpPost("execute-all")]
    public async Task<IActionResult> ExecuteAll()
    {
        var results = await _executionService.ExecuteAllControlsAsync();
        return Ok(results);
    }

    [HttpGet("{id}/preview-sql")]
    public async Task<IActionResult> PreviewSql(int id)
    {
        var control = await _controlService.GetControlByIdAsync(id);
        if (control == null) return NotFound();

        var sql = _sqlGenerator.GenerateControlQuery(control);
        return Ok(new { sql });
    }
}
