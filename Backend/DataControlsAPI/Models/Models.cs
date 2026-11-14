namespace DataControlsAPI.Models;

/// <summary>
/// ישות עסקית - מייצגת טבלה בבסיס הנתונים עם שם עסקי
/// </summary>
public class BusinessEntity
{
    public int EntityId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string? BusinessDescription { get; set; }
    public string? PrimaryKeyColumn { get; set; }
    public bool IsActive { get; set; }
    public List<BusinessField> Fields { get; set; } = new();
    public List<EntityRelation> Relations { get; set; } = new();
}

/// <summary>
/// שדה עסקי - מייצג עמודה בטבלה עם שם עסקי
/// </summary>
public class BusinessField
{
    public int FieldId { get; set; }
    public int EntityId { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string? DataType { get; set; }
    public bool IsRequired { get; set; }
    public bool IsSearchable { get; set; }
    public int DisplayOrder { get; set; }
    
    // For display purposes
    public string? EntityBusinessName { get; set; }
    public string? TableName { get; set; }
}

/// <summary>
/// קשר בין ישויות
/// </summary>
public class EntityRelation
{
    public int RelationId { get; set; }
    public int SourceEntityId { get; set; }
    public int TargetEntityId { get; set; }
    public string? SourceColumn { get; set; }
    public string? TargetColumn { get; set; }
    public string? RelationName { get; set; }
    public string? RelationType { get; set; }
    
    // For display
    public string? SourceTableName { get; set; }
    public string? TargetTableName { get; set; }
    public string? SourceEntityName { get; set; }
    public string? TargetEntityName { get; set; }
}

/// <summary>
/// בקרה
/// </summary>
public class Control
{
    public int ControlId { get; set; }
    public string ControlName { get; set; } = string.Empty;
    public string? ControlDescription { get; set; }
    public int PrimaryEntityId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedBy { get; set; }
    
    // Navigation
    public string? PrimaryEntityName { get; set; }
    public List<ControlCondition> Conditions { get; set; } = new();
    public List<ControlJoin> Joins { get; set; } = new();
}

/// <summary>
/// תנאי בקרה
/// </summary>
public class ControlCondition
{
    public int ConditionId { get; set; }
    public int ControlId { get; set; }
    public int FieldId { get; set; }
    public string Operator { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? ValueType { get; set; }
    public string LogicalOperator { get; set; } = "AND";
    public int GroupNumber { get; set; }
    public int OrderIndex { get; set; }
    
    // For display
    public string? FieldBusinessName { get; set; }
    public string? ColumnName { get; set; }
    public string? TableName { get; set; }
    public string? DataType { get; set; }
}

/// <summary>
/// JOIN לבקרה
/// </summary>
public class ControlJoin
{
    public int JoinId { get; set; }
    public int ControlId { get; set; }
    public int RelationId { get; set; }
    public string JoinType { get; set; } = "INNER";
    
    // For SQL generation
    public EntityRelation? Relation { get; set; }
}

/// <summary>
/// תוצאת הרצת בקרה
/// </summary>
public class ControlExecution
{
    public int ExecutionId { get; set; }
    public int ControlId { get; set; }
    public DateTime ExecutionDate { get; set; }
    public string? ExecutedBy { get; set; }
    public int TotalRecordsChecked { get; set; }
    public int FailedRecordsCount { get; set; }
    public int ExecutionTimeMs { get; set; }
    public string? Status { get; set; }
    public string? ErrorMessage { get; set; }
    public string? GeneratedSQL { get; set; }
}

/// <summary>
/// רשומה חריגה - תוצאת בקרה
/// </summary>
public class ControlViolation
{
    public Dictionary<string, object?> Record { get; set; } = new();
    public List<string> ViolationReasons { get; set; } = new();
}
