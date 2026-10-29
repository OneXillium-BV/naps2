#if !MAC
using System.Globalization;
using Microsoft.Extensions.Logging;
using NTwain;
using NTwain.Data;

namespace NAPS2.Scan.Internal.Twain;

internal static class TwainProfileSettings
{
    public static List<TwainCapabilitySetting> Capture(DataSource source, Microsoft.Extensions.Logging.ILogger logger)
    {
        var result = new List<TwainCapabilitySetting>();
        foreach (var value in source.Capabilities.CapSupportedCaps.GetValues())
        {
            var id = (CapabilityId) Convert.ToUInt16(value, CultureInfo.InvariantCulture);
            if (id == CapabilityId.CapSupportedCaps) continue;
            try
            {
                using var capability = new TWCapability(id);
                if (source.DGControl.Capability.GetCurrent(capability) != ReturnCode.Success)
                {
                    continue;
                }

                var reader = CapabilityReader.ReadValue(capability);
                var current = reader.ContainerType switch
                {
                    ContainerType.OneValue => reader.OneValue,
                    ContainerType.Enum when reader.CollectionValues != null &&
                                           reader.EnumCurrentIndex < reader.CollectionValues.Count =>
                        reader.CollectionValues[reader.EnumCurrentIndex],
                    ContainerType.Range => reader.RangeCurrentValue,
                    _ => null
                };
                if (current == null || !TryFormat(reader.ItemType, current, out var formatted))
                {
                    continue;
                }

                result.Add(new TwainCapabilitySetting
                {
                    CapabilityId = Convert.ToUInt16(id, CultureInfo.InvariantCulture),
                    ItemType = Convert.ToUInt16(reader.ItemType, CultureInfo.InvariantCulture),
                    Value = formatted
                });
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Unable to capture TWAIN capability {CapabilityId}", id);
            }
        }

        return result;
    }

    public static void Apply(DataSource source, IEnumerable<TwainCapabilitySetting>? settings,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        if (settings == null) return;
        foreach (var setting in settings)
        {
            try
            {
                var itemType = (ItemType) setting.ItemType;
                if (!TryParse(itemType, setting.Value, out var parsed))
                {
                    continue;
                }

                using var capability = TryParseString(itemType, setting.Value, out var text)
                    ? new TWCapability((CapabilityId) setting.CapabilityId, text, itemType)
                    : new TWCapability((CapabilityId) setting.CapabilityId,
                        new TWOneValue { ItemType = itemType, Item = ToRawValue(itemType, parsed) });
                var result = source.DGControl.Capability.Set(capability);
                if (result != ReturnCode.Success)
                {
                    logger.LogDebug("TWAIN capability {CapabilityId} was not applied: {ReturnCode}",
                        setting.CapabilityId, result);
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Unable to apply TWAIN capability {CapabilityId}", setting.CapabilityId);
            }
        }
    }

    private static bool TryFormat(ItemType itemType, object value, out string formatted)
    {
        formatted = "";
        switch (itemType)
        {
            case ItemType.Int8:
            case ItemType.Int16:
            case ItemType.Int32:
            case ItemType.UInt8:
            case ItemType.UInt16:
            case ItemType.UInt32:
            case ItemType.Bool:
                formatted = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
                return true;
            case ItemType.Fix32:
                var fixedValue = (TWFix32) value;
                formatted = $"{fixedValue.Whole},{fixedValue.Fraction}";
                return true;
            case ItemType.String32:
            case ItemType.String64:
            case ItemType.String128:
            case ItemType.String255:
                formatted = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
                return true;
            default:
                return false;
        }
    }

    private static bool TryParse(ItemType itemType, string value, out object parsed)
    {
        parsed = "";
        switch (itemType)
        {
            case ItemType.Int8:
                parsed = sbyte.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                return true;
            case ItemType.Int16:
                parsed = short.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                return true;
            case ItemType.Int32:
                parsed = int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                return true;
            case ItemType.UInt8:
                parsed = byte.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                return true;
            case ItemType.UInt16:
            case ItemType.Bool:
                parsed = ushort.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                return true;
            case ItemType.UInt32:
                parsed = uint.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                return true;
            case ItemType.Fix32:
                var parts = value.Split(',');
                if (parts.Length != 2) return false;
                parsed = new TWFix32
                {
                    Whole = short.Parse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture),
                    Fraction = ushort.Parse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture)
                };
                return true;
            case ItemType.String32:
            case ItemType.String64:
            case ItemType.String128:
            case ItemType.String255:
                parsed = value;
                return true;
            default:
                return false;
        }
    }

    private static bool TryParseString(ItemType itemType, string value, out string parsed)
    {
        parsed = value;
        return itemType is ItemType.String32 or ItemType.String64 or ItemType.String128 or ItemType.String255;
    }

    private static uint ToRawValue(ItemType itemType, object value)
    {
        return itemType switch
        {
            ItemType.Int8 => unchecked((byte) (sbyte) value),
            ItemType.Int16 => unchecked((ushort) (short) value),
            ItemType.Int32 => unchecked((uint) (int) value),
            ItemType.Fix32 => (uint) ((TWFix32) value).Whole << 16 | ((TWFix32) value).Fraction,
            _ => Convert.ToUInt32(value, CultureInfo.InvariantCulture)
        };
    }
}
#endif
