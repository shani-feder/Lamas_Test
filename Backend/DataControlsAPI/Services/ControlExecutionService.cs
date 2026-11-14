using DataControlsAPI.DTOs;
using DataControlsAPI.Models;
using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace DataControlsAPI.Services;

public class ControlExecutionService : IControlExecutionService
{
    private readonly string _connectionString;
    private readonly IControlService _controlService;
    private readonly ISqlGeneratorService _sqlGenerator;
    private readonly IMetadataService _metadataService;

    public ControlExecutionService(
        IConfiguration configuration,
        IControlService controlService,
        ISqlGeneratorService sqlGenerator,
        IMetadataService metadataService)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _controlService = controlService;
        _sqlGenerator = sqlGenerator;
        _metadataService = metadataService;
    }

    public async Task<ControlExecutionResultDto> ExecuteControlAsync(int controlId)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new ControlExecutionResultDto { ControlId = controlId };

        try
        {
            var control = await _controlService.GetControlByIdAsync(controlId);
            if (control == null)
            {
                result.Status = "Error";
                result.ErrorMessage = "Control not found";
                return result;
            }

            result.ControlName = control.ControlName;
            var sql = _sqlGenerator.GenerateControlQuery(control);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // Get total records count
            var entity = await _metadataService.GetEntityByIdAsync(control.PrimaryEntityId);
            var countCmd = new SqlCommand($"SELECT COUNT(*) FROM {entity!.TableName}", conn);
            result.TotalRecordsChecked = (int)await countCmd.ExecuteScalarAsync();

            // Execute control query
            var cmd = new SqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            // Get columns info
            var fields = await _metadataService.GetFieldsByEntityIdAsync(control.PrimaryEntityId);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var colName = reader.GetName(i);
                var field = fields.FirstOrDefault(f => f.ColumnName == colName);
                result.Columns.Add(new ColumnInfoDto
                {
                    ColumnName = colName,
                    BusinessName = field?.BusinessName ?? colName,
                    DataType = field?.DataType ?? "text"
                });
            }

            // Read violations
            while (await reader.ReadAsync())
            {
                var violation = new ViolationRecordDto();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    violation.Data[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                result.Violations.Add(violation);
            }

            result.FailedRecordsCount = result.Violations.Count;
            result.Status = "Success";

            stopwatch.Stop();
            result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;

            // Save execution history
            await SaveExecutionAsync(controlId, result, sql);
        }
        catch (Exception ex)
        {
            result.Status = "Error";
            result.ErrorMessage = ex.Message;
            stopwatch.Stop();
            result.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;
        }

        return result;
    }

    public async Task<List<ControlExecutionResultDto>> ExecuteAllControlsAsync()
    {
        var results = new List<ControlExecutionResultDto>();
        var controls = await _controlService.GetAllControlsAsync();

        foreach (var control in controls.Where(c => c.IsActive))
        {
            results.Add(await ExecuteControlAsync(control.ControlId));
        }

        return results;
    }

    private async Task SaveExecutionAsync(int controlId, ControlExecutionResultDto result, string sql)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            INSERT INTO ControlExecutions (ControlId, ExecutedBy, TotalRecordsChecked, FailedRecordsCount, 
                                          ExecutionTimeMs, Status, ErrorMessage, GeneratedSQL)
            VALUES (@ControlId, @ExecutedBy, @Total, @Failed, @Time, @Status, @Error, @SQL)", conn);
        
        cmd.Parameters.AddWithValue("@ControlId", controlId);
        cmd.Parameters.AddWithValue("@ExecutedBy", "System");
        cmd.Parameters.AddWithValue("@Total", result.TotalRecordsChecked);
        cmd.Parameters.AddWithValue("@Failed", result.FailedRecordsCount);
        cmd.Parameters.AddWithValue("@Time", result.ExecutionTimeMs);
        cmd.Parameters.AddWithValue("@Status", result.Status);
        cmd.Parameters.AddWithValue("@Error", (object?)result.ErrorMessage ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SQL", sql);

        await cmd.ExecuteNonQueryAsync();
    }
}
