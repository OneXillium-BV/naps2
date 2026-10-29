namespace NAPS2.Scan;

/// <summary>
/// Editable WIA device and item properties captured from a scanner.
/// </summary>
public class WiaProfileSettings
{
    public string ItemName { get; set; } = "";

    public Dictionary<int, object> DeviceProps { get; set; } = new();

    public Dictionary<int, object> ItemProps { get; set; } = new();

    public int Count => DeviceProps.Count + ItemProps.Count;
}
