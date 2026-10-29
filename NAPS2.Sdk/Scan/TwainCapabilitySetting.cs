namespace NAPS2.Scan;

/// <summary>
/// A TWAIN capability value captured from a data source.
/// </summary>
public class TwainCapabilitySetting
{
    public ushort CapabilityId { get; set; }
    public ushort ItemType { get; set; }
    public string Value { get; set; } = "";
}
