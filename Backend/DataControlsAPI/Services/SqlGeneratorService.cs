using DataControlsAPI.Models;
using System.Text;

namespace DataControlsAPI.Services;

public class SqlGeneratorService : ISqlGeneratorService
{
    private readonly IMetadataService _metadataService;

    public SqlGeneratorService(IMetadataService metadataService)
    {
        _metadataService = metadataService;
    }

    public string GenerateControlQuery(Control control)
    {
        var sb = new StringBuilder();
        var entity = _metadataService.GetEntityByIdAsync(control.PrimaryEntityId).Result;
        if (entity == null) throw new InvalidOperationException("Primary entity not found");

        var primaryAlias = "t0";
        var aliasMap = new Dictionary<int, string> { { entity.EntityId, primaryAlias } };

        // Build SELECT
        sb.AppendLine($"SELECT {primaryAlias}.*");

        // Build FROM
        sb.AppendLine($"FROM {entity.TableName} {primaryAlias}");

        // Build JOINs
        int aliasIndex = 1;
        foreach (var join in control.Joins)
        {
            if (join.Relation == null) continue;

            var targetAlias = $"t{aliasIndex++}";
            var joinType = join.JoinType.ToUpper();

            if (join.Relation.SourceEntityId == entity.EntityId)
            {
                aliasMap[join.Relation.TargetEntityId] = targetAlias;
                sb.AppendLine($"{joinType} JOIN {join.Relation.TargetTableName} {targetAlias} ON {primaryAlias}.{join.Relation.SourceColumn} = {targetAlias}.{join.Relation.TargetColumn}");
            }
            else
            {
                aliasMap[join.Relation.SourceEntityId] = targetAlias;
                sb.AppendLine($"{joinType} JOIN {join.Relation.SourceTableName} {targetAlias} ON {targetAlias}.{join.Relation.SourceColumn} = {primaryAlias}.{join.Relation.TargetColumn}");
            }
        }

        // Build WHERE
        if (control.Conditions.Any())
        {
            sb.AppendLine("WHERE");
            var conditions = control.Conditions.OrderBy(c => c.OrderIndex).ToList();
            var groups = conditions.GroupBy(c => c.GroupNumber).OrderBy(g => g.Key);

            bool firstGroup = true;
            foreach (var group in groups)
            {
                if (!firstGroup)
                    sb.Append($" {group.First().LogicalOperator} ");
                
                if (groups.Count() > 1)
                    sb.Append("(");

                bool firstCondition = true;
                foreach (var cond in group)
                {
                    if (!firstCondition)
                        sb.Append($" {cond.LogicalOperator} ");

                    var alias = GetAliasForField(cond, aliasMap, entity.EntityId);
                    sb.Append(BuildCondition(cond, alias));
                    firstCondition = false;
                }

                if (groups.Count() > 1)
                    sb.Append(")");
                
                firstGroup = false;
            }
        }

        return sb.ToString();
    }

    private string GetAliasForField(ControlCondition cond, Dictionary<int, string> aliasMap, int primaryEntityId)
    {
        var fields = _metadataService.GetAllFieldsAsync().Result;
        var field = fields.FirstOrDefault(f => f.FieldId == cond.FieldId);
        if (field == null) return "t0";

        return aliasMap.TryGetValue(field.EntityId, out var alias) ? alias : "t0";
    }

    private string BuildCondition(ControlCondition cond, string alias)
    {
        var column = $"{alias}.{cond.ColumnName}";

        return cond.Operator.ToUpper() switch
        {
            "IS NULL" => $"{column} IS NULL",
            "IS NOT NULL" => $"{column} IS NOT NULL",
            "LIKE" => $"{column} LIKE '%{EscapeValue(cond.Value)}%'",
            "IN" => $"{column} IN ({cond.Value})",
            _ when cond.ValueType == "field" => $"{column} {cond.Operator} {alias}.{cond.Value}",
            _ when cond.ValueType == "expression" => $"{column} {cond.Operator} {cond.Value}",
            _ when IsNumeric(cond.Value) => $"{column} {cond.Operator} {cond.Value}",
            _ => $"{column} {cond.Operator} N'{EscapeValue(cond.Value)}'"
        };
    }

    private static string EscapeValue(string? value) => value?.Replace("'", "''") ?? "";

    private static bool IsNumeric(string? value) => 
        decimal.TryParse(value, out _) || value == "0" || value == "1";
}
