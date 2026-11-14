namespace DataControlsAPI.DTOs;

public class ControlDto
{
    public int? ControlId { get; set; }
    public string ControlName { get; set; } = string.Empty;
    public string? ControlDescription { get; set; }
    public int PrimaryEntityId { get; set; }
    public List<ConditionDto> Conditions { get; set; } = new();
    public List<JoinDto> Joins { get; set; } = new();
}

public class ConditionDto
{
    public int? ConditionId { get; set; }
    public int FieldId { get; set; }
    public string Operator { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string ValueType { get; set; } = "static";
    public string LogicalOperator { get; set; } = "AND";
    public int GroupNumber { get; set; }
    public int OrderIndex { get; set; }
}

public class JoinDto
{
    public int? JoinId { get; set; }
    public int RelationId { get; set; }
    public string JoinType { get; set; } = "INNER";
}

public class ControlExecutionResultDto
{
    public int ControlId { get; set; }
    public string ControlName { get; set; } = string.Empty;
    public int TotalRecordsChecked { get; set; }
    public int FailedRecordsCount { get; set; }
    public int ExecutionTimeMs { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public List<ViolationRecordDto> Violations { get; set; } = new();
    public List<ColumnInfoDto> Columns { get; set; } = new();
}

public class ViolationRecordDto
{
    public Dictionary<string, object?> Data { get; set; } = new();
    public List<string> Reasons { get; set; } = new();
}

public class ColumnInfoDto
{
    public string ColumnName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
}

public class ControlSummaryDto
{
    public int ControlId { get; set; }
    public string ControlName { get; set; } = string.Empty;
    public string? ControlDescription { get; set; }
    public string? PrimaryEntityName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? LastExecutionFailedCount { get; set; }
    public DateTime? LastExecutionDate { get; set; }
}

public class OperatorInfo
{
    public string Value { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool RequiresValue { get; set; } = true;
    public List<string> ApplicableTypes { get; set; } = new();
}
