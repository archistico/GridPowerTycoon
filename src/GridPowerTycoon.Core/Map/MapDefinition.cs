namespace GridPowerTycoon.Core.Map;

public sealed class MapDefinition
{
    public int Version { get; init; } = 1;
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public int Width { get; init; }
    public int Height { get; init; }
    public Dictionary<string, string> Legend { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Rows { get; init; } = new();

    /// <summary>
    /// Optional real terrain layer. When a visible row contains a cloud cell, this layer tells what tile
    /// is revealed after unlocking that cloud. If omitted, cloud cells reveal Land.
    /// </summary>
    public List<string> HiddenRows { get; init; } = new();
}
