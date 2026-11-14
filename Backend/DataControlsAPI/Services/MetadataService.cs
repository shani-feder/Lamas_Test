using DataControlsAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DataControlsAPI.Services;

public class MetadataService : IMetadataService
{
    private readonly string _connectionString;

    public MetadataService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<List<BusinessEntity>> GetAllEntitiesAsync()
    {
        var entities = new List<BusinessEntity>();
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            SELECT EntityId, TableName, BusinessName, BusinessDescription, PrimaryKeyColumn, IsActive
            FROM BusinessEntities WHERE IsActive = 1", conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            entities.Add(new BusinessEntity
            {
                EntityId = reader.GetInt32(0),
                TableName = reader.GetString(1),
                BusinessName = reader.GetString(2),
                BusinessDescription = reader.IsDBNull(3) ? null : reader.GetString(3),
                PrimaryKeyColumn = reader.IsDBNull(4) ? null : reader.GetString(4),
                IsActive = reader.GetBoolean(5)
            });
        }
        return entities;
    }

    public async Task<BusinessEntity?> GetEntityByIdAsync(int entityId)
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            SELECT EntityId, TableName, BusinessName, BusinessDescription, PrimaryKeyColumn, IsActive
            FROM BusinessEntities WHERE EntityId = @EntityId", conn);
        cmd.Parameters.AddWithValue("@EntityId", entityId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new BusinessEntity
            {
                EntityId = reader.GetInt32(0),
                TableName = reader.GetString(1),
                BusinessName = reader.GetString(2),
                BusinessDescription = reader.IsDBNull(3) ? null : reader.GetString(3),
                PrimaryKeyColumn = reader.IsDBNull(4) ? null : reader.GetString(4),
                IsActive = reader.GetBoolean(5)
            };
        }
        return null;
    }

    public async Task<List<BusinessField>> GetFieldsByEntityIdAsync(int entityId)
    {
        var fields = new List<BusinessField>();
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            SELECT f.FieldId, f.EntityId, f.ColumnName, f.BusinessName, f.DataType, 
                   f.IsRequired, f.IsSearchable, f.DisplayOrder, e.BusinessName, e.TableName
            FROM BusinessFields f
            JOIN BusinessEntities e ON f.EntityId = e.EntityId
            WHERE f.EntityId = @EntityId
            ORDER BY f.DisplayOrder", conn);
        cmd.Parameters.AddWithValue("@EntityId", entityId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            fields.Add(MapField(reader));
        }
        return fields;
    }

    public async Task<List<BusinessField>> GetAllFieldsAsync()
    {
        var fields = new List<BusinessField>();
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            SELECT f.FieldId, f.EntityId, f.ColumnName, f.BusinessName, f.DataType, 
                   f.IsRequired, f.IsSearchable, f.DisplayOrder, e.BusinessName, e.TableName
            FROM BusinessFields f
            JOIN BusinessEntities e ON f.EntityId = e.EntityId
            WHERE e.IsActive = 1
            ORDER BY e.BusinessName, f.DisplayOrder", conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            fields.Add(MapField(reader));
        }
        return fields;
    }

    public async Task<List<EntityRelation>> GetRelationsAsync()
    {
        var relations = new List<EntityRelation>();
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            SELECT r.RelationId, r.SourceEntityId, r.TargetEntityId, r.SourceColumn, r.TargetColumn,
                   r.RelationName, r.RelationType, s.TableName, t.TableName, s.BusinessName, t.BusinessName
            FROM EntityRelations r
            JOIN BusinessEntities s ON r.SourceEntityId = s.EntityId
            JOIN BusinessEntities t ON r.TargetEntityId = t.EntityId", conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            relations.Add(MapRelation(reader));
        }
        return relations;
    }

    public async Task<List<EntityRelation>> GetRelationsByEntityIdAsync(int entityId)
    {
        var relations = new List<EntityRelation>();
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new SqlCommand(@"
            SELECT r.RelationId, r.SourceEntityId, r.TargetEntityId, r.SourceColumn, r.TargetColumn,
                   r.RelationName, r.RelationType, s.TableName, t.TableName, s.BusinessName, t.BusinessName
            FROM EntityRelations r
            JOIN BusinessEntities s ON r.SourceEntityId = s.EntityId
            JOIN BusinessEntities t ON r.TargetEntityId = t.EntityId
            WHERE r.SourceEntityId = @EntityId OR r.TargetEntityId = @EntityId", conn);
        cmd.Parameters.AddWithValue("@EntityId", entityId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            relations.Add(MapRelation(reader));
        }
        return relations;
    }

    private static BusinessField MapField(SqlDataReader reader) => new()
    {
        FieldId = reader.GetInt32(0),
        EntityId = reader.GetInt32(1),
        ColumnName = reader.GetString(2),
        BusinessName = reader.GetString(3),
        DataType = reader.IsDBNull(4) ? null : reader.GetString(4),
        IsRequired = reader.GetBoolean(5),
        IsSearchable = reader.GetBoolean(6),
        DisplayOrder = reader.GetInt32(7),
        EntityBusinessName = reader.GetString(8),
        TableName = reader.GetString(9)
    };

    private static EntityRelation MapRelation(SqlDataReader reader) => new()
    {
        RelationId = reader.GetInt32(0),
        SourceEntityId = reader.GetInt32(1),
        TargetEntityId = reader.GetInt32(2),
        SourceColumn = reader.IsDBNull(3) ? null : reader.GetString(3),
        TargetColumn = reader.IsDBNull(4) ? null : reader.GetString(4),
        RelationName = reader.IsDBNull(5) ? null : reader.GetString(5),
        RelationType = reader.IsDBNull(6) ? null : reader.GetString(6),
        SourceTableName = reader.GetString(7),
        TargetTableName = reader.GetString(8),
        SourceEntityName = reader.GetString(9),
        TargetEntityName = reader.GetString(10)
    };
}
