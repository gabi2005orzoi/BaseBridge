using Google.Protobuf.WellKnownTypes;

namespace BaseBridge.Models.DTOs;

public class DatabaseSchemaResponse
{
    public List<TableInfo> Tables { get; set; } = new();
}

public class TableInfo
{
    public string Name { get; set; } = string.Empty;
    public List<ColumnInfo> Columns { get; set; } = new();
    public List<RelationshipInfo> Relationships { get; set; } = new();
    public List<Dictionary<string, object>> SampleData { get; set; } = new();
}

public class ColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
}

public class RelationshipInfo
{
    public string Column { get; set; } = string.Empty;
    public string ReferencedTable { get; set; } = string.Empty;
    public string ReferencedColumn { get; set; } = string.Empty;
}