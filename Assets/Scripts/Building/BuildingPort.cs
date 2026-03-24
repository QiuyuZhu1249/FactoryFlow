public enum PortType
{
    Input,
    Output
}

/// <summary>
/// Describes a single input or output port on a building.
/// </summary>
public class BuildingPort
{
    public PortType Type { get; private set; }
    public Direction Direction { get; private set; }

    public BuildingPort(PortType type, Direction direction)
    {
        Type = type;
        Direction = direction;
    }
}
