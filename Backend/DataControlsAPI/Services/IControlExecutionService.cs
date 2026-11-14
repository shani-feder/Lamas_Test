using DataControlsAPI.DTOs;

namespace DataControlsAPI.Services;

public interface IControlExecutionService
{
    Task<ControlExecutionResultDto> ExecuteControlAsync(int controlId);
    Task<List<ControlExecutionResultDto>> ExecuteAllControlsAsync();
}
