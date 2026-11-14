using DataControlsAPI.DTOs;
using DataControlsAPI.Models;
using Microsoft.Data.SqlClient;

namespace DataControlsAPI.Services;

public class ControlService : IControlService
{
    private readonly string _connectionString;

    public ControlService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<List<ControlSummaryDto>> GetAllControlsAsync()
    {
        var controls = new List<ControlSummaryDto>();
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            SELECT c.ControlId, c.ControlName, c.ControlDescription, e.BusinessName, c.IsActive, c.CreatedDate,
                   (SELECT TOP 1 FailedRecordsCount FROM ControlExecutions WHERE ControlId = c.ControlId ORDER BY ExecutionDate DESC),
                   (SELECT TOP 1 ExecutionDate FROM ControlExecutions WHERE ControlId = c.ControlId ORDER BY ExecutionDate DESC)
            FROM Controls c
            LEFT JOIN BusinessEntities e ON c.PrimaryEntityId = e.EntityId
            ORDER BY c.ControlName", conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            controls.Add(new ControlSummaryDto
            {
                ControlId = reader.GetInt32(0),
                ControlName = reader.GetString(1),
                ControlDescription = reader.IsDBNull(2) ? null : reader.GetString(2),
                PrimaryEntityName = reader.IsDBNull(3) ? null : reader.GetString(3),
                IsActive = reader.GetBoolean(4),
                CreatedDate = reader.GetDateTime(5),
                LastExecutionFailedCount = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                LastExecutionDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7)
            });
        }
        return controls;
    }

    public async Task<Control?> GetControlByIdAsync(int controlId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        // Get control
        var cmd = new SqlCommand(@"
            SELECT c.ControlId, c.ControlName, c.ControlDescription, c.PrimaryEntityId, c.IsActive, 
                   c.CreatedDate, c.CreatedBy, c.LastModifiedDate, c.LastModifiedBy, e.BusinessName
            FROM Controls c
            LEFT JOIN BusinessEntities e ON c.PrimaryEntityId = e.EntityId
            WHERE c.ControlId = @ControlId", conn);
        cmd.Parameters.AddWithValue("@ControlId", controlId);

        Control? control = null;
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                control = new Control
                {
                    ControlId = reader.GetInt32(0),
                    ControlName = reader.GetString(1),
                    ControlDescription = reader.IsDBNull(2) ? null : reader.GetString(2),
                    PrimaryEntityId = reader.GetInt32(3),
                    IsActive = reader.GetBoolean(4),
                    CreatedDate = reader.GetDateTime(5),
                    CreatedBy = reader.IsDBNull(6) ? null : reader.GetString(6),
                    LastModifiedDate = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                    LastModifiedBy = reader.IsDBNull(8) ? null : reader.GetString(8),
                    PrimaryEntityName = reader.IsDBNull(9) ? null : reader.GetString(9)
                };
            }
        }

        if (control == null) return null;

        // Get conditions
        cmd = new SqlCommand(@"
            SELECT cc.ConditionId, cc.ControlId, cc.FieldId, cc.Operator, cc.Value, cc.ValueType,
                   cc.LogicalOperator, cc.GroupNumber, cc.OrderIndex, f.BusinessName, f.ColumnName, e.TableName, f.DataType
            FROM ControlConditions cc
            JOIN BusinessFields f ON cc.FieldId = f.FieldId
            JOIN BusinessEntities e ON f.EntityId = e.EntityId
            WHERE cc.ControlId = @ControlId
            ORDER BY cc.OrderIndex", conn);
        cmd.Parameters.AddWithValue("@ControlId", controlId);

        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                control.Conditions.Add(new ControlCondition
                {
                    ConditionId = reader.GetInt32(0),
                    ControlId = reader.GetInt32(1),
                    FieldId = reader.GetInt32(2),
                    Operator = reader.GetString(3),
                    Value = reader.IsDBNull(4) ? null : reader.GetString(4),
                    ValueType = reader.IsDBNull(5) ? null : reader.GetString(5),
                    LogicalOperator = reader.GetString(6),
                    GroupNumber = reader.GetInt32(7),
                    OrderIndex = reader.GetInt32(8),
                    FieldBusinessName = reader.GetString(9),
                    ColumnName = reader.GetString(10),
                    TableName = reader.GetString(11),
                    DataType = reader.IsDBNull(12) ? null : reader.GetString(12)
                });
            }
        }

        // Get joins
        cmd = new SqlCommand(@"
            SELECT cj.JoinId, cj.ControlId, cj.RelationId, cj.JoinType,
                   r.SourceEntityId, r.TargetEntityId, r.SourceColumn, r.TargetColumn, r.RelationName, r.RelationType,
                   s.TableName, t.TableName
            FROM ControlJoins cj
            JOIN EntityRelations r ON cj.RelationId = r.RelationId
            JOIN BusinessEntities s ON r.SourceEntityId = s.EntityId
            JOIN BusinessEntities t ON r.TargetEntityId = t.EntityId
            WHERE cj.ControlId = @ControlId", conn);
        cmd.Parameters.AddWithValue("@ControlId", controlId);

        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                control.Joins.Add(new ControlJoin
                {
                    JoinId = reader.GetInt32(0),
                    ControlId = reader.GetInt32(1),
                    RelationId = reader.GetInt32(2),
                    JoinType = reader.GetString(3),
                    Relation = new EntityRelation
                    {
                        RelationId = reader.GetInt32(2),
                        SourceEntityId = reader.GetInt32(4),
                        TargetEntityId = reader.GetInt32(5),
                        SourceColumn = reader.IsDBNull(6) ? null : reader.GetString(6),
                        TargetColumn = reader.IsDBNull(7) ? null : reader.GetString(7),
                        RelationName = reader.IsDBNull(8) ? null : reader.GetString(8),
                        RelationType = reader.IsDBNull(9) ? null : reader.GetString(9),
                        SourceTableName = reader.GetString(10),
                        TargetTableName = reader.GetString(11)
                    }
                });
            }
        }

        return control;
    }

    public async Task<int> CreateControlAsync(ControlDto dto)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        using var transaction = conn.BeginTransaction();

        try
        {
            // Insert control
            var cmd = new SqlCommand(@"
                INSERT INTO Controls (ControlName, ControlDescription, PrimaryEntityId, CreatedBy)
                VALUES (@Name, @Desc, @EntityId, @CreatedBy);
                SELECT SCOPE_IDENTITY();", conn, transaction);
            cmd.Parameters.AddWithValue("@Name", dto.ControlName);
            cmd.Parameters.AddWithValue("@Desc", (object?)dto.ControlDescription ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EntityId", dto.PrimaryEntityId);
            cmd.Parameters.AddWithValue("@CreatedBy", "System");

            var controlId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            // Insert conditions
            foreach (var cond in dto.Conditions)
            {
                cmd = new SqlCommand(@"
                    INSERT INTO ControlConditions (ControlId, FieldId, Operator, Value, ValueType, LogicalOperator, GroupNumber, OrderIndex)
                    VALUES (@ControlId, @FieldId, @Operator, @Value, @ValueType, @LogicalOp, @GroupNum, @OrderIdx)", conn, transaction);
                cmd.Parameters.AddWithValue("@ControlId", controlId);
                cmd.Parameters.AddWithValue("@FieldId", cond.FieldId);
                cmd.Parameters.AddWithValue("@Operator", cond.Operator);
                cmd.Parameters.AddWithValue("@Value", (object?)cond.Value ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ValueType", cond.ValueType);
                cmd.Parameters.AddWithValue("@LogicalOp", cond.LogicalOperator);
                cmd.Parameters.AddWithValue("@GroupNum", cond.GroupNumber);
                cmd.Parameters.AddWithValue("@OrderIdx", cond.OrderIndex);
                await cmd.ExecuteNonQueryAsync();
            }

            // Insert joins
            foreach (var join in dto.Joins)
            {
                cmd = new SqlCommand(@"
                    INSERT INTO ControlJoins (ControlId, RelationId, JoinType)
                    VALUES (@ControlId, @RelationId, @JoinType)", conn, transaction);
                cmd.Parameters.AddWithValue("@ControlId", controlId);
                cmd.Parameters.AddWithValue("@RelationId", join.RelationId);
                cmd.Parameters.AddWithValue("@JoinType", join.JoinType);
                await cmd.ExecuteNonQueryAsync();
            }

            transaction.Commit();
            return controlId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> UpdateControlAsync(int controlId, ControlDto dto)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        using var transaction = conn.BeginTransaction();

        try
        {
            // Update control
            var cmd = new SqlCommand(@"
                UPDATE Controls SET ControlName = @Name, ControlDescription = @Desc, 
                       PrimaryEntityId = @EntityId, LastModifiedDate = GETDATE(), LastModifiedBy = @ModifiedBy
                WHERE ControlId = @ControlId", conn, transaction);
            cmd.Parameters.AddWithValue("@ControlId", controlId);
            cmd.Parameters.AddWithValue("@Name", dto.ControlName);
            cmd.Parameters.AddWithValue("@Desc", (object?)dto.ControlDescription ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EntityId", dto.PrimaryEntityId);
            cmd.Parameters.AddWithValue("@ModifiedBy", "System");

            var rows = await cmd.ExecuteNonQueryAsync();
            if (rows == 0) return false;

            // Delete old conditions and joins
            cmd = new SqlCommand("DELETE FROM ControlConditions WHERE ControlId = @ControlId", conn, transaction);
            cmd.Parameters.AddWithValue("@ControlId", controlId);
            await cmd.ExecuteNonQueryAsync();

            cmd = new SqlCommand("DELETE FROM ControlJoins WHERE ControlId = @ControlId", conn, transaction);
            cmd.Parameters.AddWithValue("@ControlId", controlId);
            await cmd.ExecuteNonQueryAsync();

            // Insert new conditions
            foreach (var cond in dto.Conditions)
            {
                cmd = new SqlCommand(@"
                    INSERT INTO ControlConditions (ControlId, FieldId, Operator, Value, ValueType, LogicalOperator, GroupNumber, OrderIndex)
                    VALUES (@ControlId, @FieldId, @Operator, @Value, @ValueType, @LogicalOp, @GroupNum, @OrderIdx)", conn, transaction);
                cmd.Parameters.AddWithValue("@ControlId", controlId);
                cmd.Parameters.AddWithValue("@FieldId", cond.FieldId);
                cmd.Parameters.AddWithValue("@Operator", cond.Operator);
                cmd.Parameters.AddWithValue("@Value", (object?)cond.Value ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ValueType", cond.ValueType);
                cmd.Parameters.AddWithValue("@LogicalOp", cond.LogicalOperator);
                cmd.Parameters.AddWithValue("@GroupNum", cond.GroupNumber);
                cmd.Parameters.AddWithValue("@OrderIdx", cond.OrderIndex);
                await cmd.ExecuteNonQueryAsync();
            }

            // Insert new joins
            foreach (var join in dto.Joins)
            {
                cmd = new SqlCommand(@"
                    INSERT INTO ControlJoins (ControlId, RelationId, JoinType)
                    VALUES (@ControlId, @RelationId, @JoinType)", conn, transaction);
                cmd.Parameters.AddWithValue("@ControlId", controlId);
                cmd.Parameters.AddWithValue("@RelationId", join.RelationId);
                cmd.Parameters.AddWithValue("@JoinType", join.JoinType);
                await cmd.ExecuteNonQueryAsync();
            }

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteControlAsync(int controlId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        using var transaction = conn.BeginTransaction();

        try
        {
            var cmd = new SqlCommand("DELETE FROM ControlConditions WHERE ControlId = @ControlId", conn, transaction);
            cmd.Parameters.AddWithValue("@ControlId", controlId);
            await cmd.ExecuteNonQueryAsync();

            cmd = new SqlCommand("DELETE FROM ControlJoins WHERE ControlId = @ControlId", conn, transaction);
            cmd.Parameters.AddWithValue("@ControlId", controlId);
            await cmd.ExecuteNonQueryAsync();

            cmd = new SqlCommand("DELETE FROM ControlExecutions WHERE ControlId = @ControlId", conn, transaction);
            cmd.Parameters.AddWithValue("@ControlId", controlId);
            await cmd.ExecuteNonQueryAsync();

            cmd = new SqlCommand("DELETE FROM Controls WHERE ControlId = @ControlId", conn, transaction);
            cmd.Parameters.AddWithValue("@ControlId", controlId);
            var rows = await cmd.ExecuteNonQueryAsync();

            transaction.Commit();
            return rows > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public Task<List<OperatorInfo>> GetOperatorsAsync()
    {
        var operators = new List<OperatorInfo>
        {
            new() { Value = "=", DisplayName = "שווה ל", RequiresValue = true, ApplicableTypes = new() { "text", "number", "date", "boolean" } },
            new() { Value = "!=", DisplayName = "שונה מ", RequiresValue = true, ApplicableTypes = new() { "text", "number", "date", "boolean" } },
            new() { Value = ">", DisplayName = "גדול מ", RequiresValue = true, ApplicableTypes = new() { "number", "date" } },
            new() { Value = "<", DisplayName = "קטן מ", RequiresValue = true, ApplicableTypes = new() { "number", "date" } },
            new() { Value = ">=", DisplayName = "גדול או שווה ל", RequiresValue = true, ApplicableTypes = new() { "number", "date" } },
            new() { Value = "<=", DisplayName = "קטן או שווה ל", RequiresValue = true, ApplicableTypes = new() { "number", "date" } },
            new() { Value = "LIKE", DisplayName = "מכיל", RequiresValue = true, ApplicableTypes = new() { "text" } },
            new() { Value = "IS NULL", DisplayName = "ריק", RequiresValue = false, ApplicableTypes = new() { "text", "number", "date", "boolean" } },
            new() { Value = "IS NOT NULL", DisplayName = "לא ריק", RequiresValue = false, ApplicableTypes = new() { "text", "number", "date", "boolean" } },
            new() { Value = "IN", DisplayName = "אחד מ", RequiresValue = true, ApplicableTypes = new() { "text", "number" } }
        };
        return Task.FromResult(operators);
    }
}
