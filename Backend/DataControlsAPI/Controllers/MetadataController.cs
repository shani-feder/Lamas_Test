using DataControlsAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataControlsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MetadataController : ControllerBase
{
    private readonly IMetadataService _metadataService;

    public MetadataController(IMetadataService metadataService)
    {
        _metadataService = metadataService;
    }

    [HttpGet("entities")]
    public async Task<IActionResult> GetEntities()
    {
        var entities = await _metadataService.GetAllEntitiesAsync();
        return Ok(entities);
    }

    [HttpGet("entities/{id}")]
    public async Task<IActionResult> GetEntity(int id)
    {
        var entity = await _metadataService.GetEntityByIdAsync(id);
        return entity == null ? NotFound() : Ok(entity);
    }

    [HttpGet("entities/{id}/fields")]
    public async Task<IActionResult> GetFieldsByEntity(int id)
    {
        var fields = await _metadataService.GetFieldsByEntityIdAsync(id);
        return Ok(fields);
    }

    [HttpGet("fields")]
    public async Task<IActionResult> GetAllFields()
    {
        var fields = await _metadataService.GetAllFieldsAsync();
        return Ok(fields);
    }

    [HttpGet("relations")]
    public async Task<IActionResult> GetRelations()
    {
        var relations = await _metadataService.GetRelationsAsync();
        return Ok(relations);
    }

    [HttpGet("entities/{id}/relations")]
    public async Task<IActionResult> GetRelationsByEntity(int id)
    {
        var relations = await _metadataService.GetRelationsByEntityIdAsync(id);
        return Ok(relations);
    }
}
