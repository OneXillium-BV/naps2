namespace NAPS2.Scan;

/// <summary>
/// A TWAIN capability value captured from a data source.
/// </summary>
public class TwainCapabilitySetting
{
    /// <summary>
    /// TWAIN capability identifier. The numeric value is the stable identifier used when applying the setting.
    /// </summary>
    public ushort CapabilityId { get; set; }

    /// <summary>
    /// SDK name for the capability identifier, when known.
    /// </summary>
    public string CapabilityName { get; set; } = "";

    /// <summary>
    /// TWAIN item type identifier used to interpret the string value.
    /// </summary>
    public ushort ItemType { get; set; }

    /// <summary>
    /// SDK name for the TWAIN item type, when known.
    /// </summary>
    public string ItemTypeName { get; set; } = "";

    /// <summary>
    /// Current capability value formatted invariantly according to its TWAIN item type.
    /// </summary>
    public string Value { get; set; } = "";
}
