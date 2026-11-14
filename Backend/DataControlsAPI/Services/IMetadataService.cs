using DataControlsAPI.Models;

namespace DataControlsAPI.Services;

public interface IMetadataService
{
    Task<List<BusinessEntity>> GetAllEntitiesAsync();
    Task<BusinessEntity?> GetEntityByIdAsync(int entityId);
    Task<List<BusinessField>> GetFieldsByEntityIdAsync(int entityId);
    Task<List<BusinessField>> GetAllFieldsAsync();
    Task<List<EntityRelation>> GetRelationsAsync();
    Task<List<EntityRelation>> GetRelationsByEntityIdAsync(int entityId);
}
