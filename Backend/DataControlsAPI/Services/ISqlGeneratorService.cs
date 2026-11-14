using DataControlsAPI.Models;

namespace DataControlsAPI.Services;

public interface ISqlGeneratorService
{
    string GenerateControlQuery(Control control);
}
