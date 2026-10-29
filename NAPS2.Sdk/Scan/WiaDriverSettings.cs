namespace NAPS2.Scan;

/// <summary>
/// Editable WIA-specific device and scanner-item properties captured from a scanner.
/// </summary>
public class WiaDriverSettings
{
    /// <summary>
    /// WIA scanner-item name these property values were captured from.
    /// </summary>
    public string SelectedItemName { get; set; } = "";

    /// <summary>
    /// Editable device-level WIA properties keyed by their numeric WIA property identifiers.
    /// </summary>
    public Dictionary<int, object> DeviceProperties { get; set; } = new();

    /// <summary>
    /// Editable scanner-item-level WIA properties keyed by their numeric WIA property identifiers.
    /// </summary>
    public Dictionary<int, object> ItemProperties { get; set; } = new();

    public int Count => DeviceProperties.Count + ItemProperties.Count;
}
