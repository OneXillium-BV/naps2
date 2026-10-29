using NAPS2.Serialization;
using NAPS2.Scan;
using NAPS2.Scan.Internal.Wia;
using NAPS2.Wia;
using Xunit;

namespace NAPS2.Sdk.Tests.Scan;

public class WiaScanDriverTests : ContextualTests
{
    private readonly WiaScanDriver _driver;

    public WiaScanDriverTests()
    {
        _driver = new WiaScanDriver(ScanningContext);
    }

    [Theory]
    [InlineData(WiaVersion.Wia10, "1.0")]
    [InlineData(WiaVersion.Wia20, "2.0")]
    [InlineData(WiaVersion.Default, null)]     // Edge case: default/unspecified WIA version
    public void GetWiaScanCaps_SetsProtocolVersionFromWiaVersion(WiaVersion wiaVersion, string expected)
    {
        var caps = _driver.GetWiaScanCaps("Naps2", "Scanner 3000", wiaVersion,
            false, false, false, null, null);

        Assert.Equal(expected, caps.MetadataCaps!.ProtocolVersion);
    }

    [Fact]
    public void GetWiaScanCaps_MapsManufacturerAndModel()
    {
        var caps = _driver.GetWiaScanCaps("Naps2", "Scanner 3000", WiaVersion.Wia20,
            false, false, false, null, null);

        Assert.Equal("Naps2", caps.MetadataCaps!.Manufacturer);
        Assert.Equal("Scanner 3000", caps.MetadataCaps!.Model);
    }

    [Fact]
    public void GetWiaScanCaps_OnlyIncludesCapsForSupportedPaperSources()
    {
        var flatbedCaps = new PerSourceCaps();
        var feederCaps = new PerSourceCaps();

        var caps = _driver.GetWiaScanCaps(null, null, WiaVersion.Wia20,
            true, true, true, flatbedCaps, feederCaps);

        Assert.True(caps.PaperSourceCaps!.SupportsFlatbed);
        Assert.True(caps.PaperSourceCaps!.SupportsFeeder);
        Assert.True(caps.PaperSourceCaps!.SupportsDuplex);
        Assert.Same(flatbedCaps, caps.FlatbedCaps);
        Assert.Same(feederCaps, caps.FeederCaps);
        Assert.Same(feederCaps, caps.DuplexCaps); // duplex reuses feeder caps
    }

    [Fact]
    public void GetWiaScanCaps_ExcludesCapsForUnsupportedPaperSources()
    {
        var flatbedCaps = new PerSourceCaps();
        var feederCaps = new PerSourceCaps();

        var caps = _driver.GetWiaScanCaps(null, null, WiaVersion.Wia20,
            false, false, false, flatbedCaps, feederCaps);

        Assert.False(caps.PaperSourceCaps!.SupportsFlatbed);
        Assert.False(caps.PaperSourceCaps!.SupportsFeeder);
        Assert.False(caps.PaperSourceCaps!.SupportsDuplex);
        Assert.Null(caps.FlatbedCaps);
        Assert.Null(caps.FeederCaps);
        Assert.Null(caps.DuplexCaps);
    }

    [Fact]
    public void WiaProfileSettings_SerializesArbitraryPropertyValues()
    {
        var settings = new WiaProfileSettings
        {
            ItemName = "Feeder",
            DeviceProps = new Dictionary<int, object>
            {
                [1] = 123,
                [2] = "custom device value"
            },
            ItemProps = new Dictionary<int, object>
            {
                [3] = 456
            }
        };

        var reloaded = settings.ToXml().FromXml<WiaProfileSettings>();

        Assert.Equal("Feeder", reloaded.ItemName);
        Assert.Equal(123, reloaded.DeviceProps[1]);
        Assert.Equal("custom device value", reloaded.DeviceProps[2]);
        Assert.Equal(456, reloaded.ItemProps[3]);
        Assert.Equal(3, reloaded.Count);
    }
}
