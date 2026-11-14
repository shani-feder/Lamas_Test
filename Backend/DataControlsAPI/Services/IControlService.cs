using DataControlsAPI.DTOs;
using DataControlsAPI.Models;

namespace DataControlsAPI.Services;

public interface IControlService
{
    Task<List<ControlSummaryDto>> GetAllControlsAsync();
    Task<Control?> GetControlByIdAsync(int controlId);
    Task<int> CreateControlAsync(ControlDto dto);
    Task<bool> UpdateControlAsync(int controlId, ControlDto dto);
    Task<bool> DeleteControlAsync(int controlId);
    Task<List<OperatorInfo>> GetOperatorsAsync();
}
