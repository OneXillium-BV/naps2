using NAPS2.Scan;
using NAPS2.Sdk.Tests;
using Xunit;

namespace NAPS2.Lib.Tests.Config;

public class ProfileManagerTests : ContextualTests
{
    private readonly string _userPath;
    private readonly string _systemPath;
    private readonly ProfileManager _profileManager;

    public ProfileManagerTests()
    {
        _userPath = P(Path.GetRandomFileName());
        _systemPath = P(Path.GetRandomFileName());
        _profileManager = new ProfileManager(_userPath, _systemPath, false, false, false);
    }

    [Fact]
    public void SaveOneProfile()
    {
        var profile = new ScanProfile
        {
            Device = new ScanProfileDevice("test_id", "test_name"),
            DisplayName = "A Profile",
            TwainDriverSettings = new List<TwainCapabilitySetting>
            {
                new()
                {
                    CapabilityId = 0x8123,
                    CapabilityName = "CapXResolution",
                    ItemType = 4,
                    ItemTypeName = "Fix32",
                    Value = "300"
                }
            },
            WiaDriverSettings = new WiaDriverSettings
            {
                SelectedItemName = "Scan",
                DeviceProperties = new Dictionary<int, object> { [0x1234] = 42 },
                ItemProperties = new Dictionary<int, object> { [0x5678] = "custom" }
            }
        };
        _profileManager.Mutate(new ListMutation<ScanProfile>.Append(profile), new Selectable<ScanProfile>());
        _profileManager.Save();

        var doc = XDocument.Load(_userPath);

        var root = doc.Root;
        Assert.NotNull(root);
        Assert.Equal("ArrayOfScanProfile", root.Name);
        var profileEl = root.Elements("ScanProfile").Single();
        Assert.Equal("test_id", profileEl.Element("Device")?.Element("ID")?.Value);
        Assert.Equal("A Profile", profileEl.Element("DisplayName")?.Value);
        Assert.Equal("300", profileEl.Element("TwainDriverSettings")?.Element("TwainCapabilitySetting")?
            .Element("Value")?.Value);
        Assert.Equal("CapXResolution", profileEl.Element("TwainDriverSettings")?
            .Element("TwainCapabilitySetting")?.Element("CapabilityName")?.Value);
        Assert.Equal("Scan", profileEl.Element("WiaDriverSettings")?.Element("SelectedItemName")?.Value);
        // XmlIgnore elements
        Assert.Empty(profileEl.Elements("IsLocked"));
        Assert.Empty(profileEl.Elements("IsDeviceLocked"));
        Assert.Empty(profileEl.Elements("UpgradedFrom"));

        var reloaded = new ProfileManager(_userPath, _systemPath, false, false, false).Profiles.Single();
        var savedSetting = Assert.Single(reloaded.TwainDriverSettings!);
        Assert.Equal((ushort) 0x8123, savedSetting.CapabilityId);
        Assert.Equal((ushort) 4, savedSetting.ItemType);
        Assert.Equal("300", savedSetting.Value);
        Assert.Equal("CapXResolution", savedSetting.CapabilityName);
        Assert.Equal("Fix32", savedSetting.ItemTypeName);
        Assert.Equal("Scan", reloaded.WiaDriverSettings!.SelectedItemName);
        Assert.Equal(42, reloaded.WiaDriverSettings.DeviceProperties[0x1234]);
        Assert.Equal("custom", reloaded.WiaDriverSettings.ItemProperties[0x5678]);
    }

    [Fact]
    public void LoadOneProfile()
    {
        var xml = ConfigData.SingleProfile;
        File.WriteAllText(_userPath, xml);

        var profiles = _profileManager.Profiles;
        Assert.Single(profiles);

        Assert.Equal("test_driver", profiles[0].DriverName);
        Assert.Equal("test_id", profiles[0].Device?.ID);
        Assert.Equal(2, profiles[0].Version);
        Assert.Null(profiles[0].UpgradedFrom);
    }

    [Fact]
    public void LoadVeryOldProfile()
    {
        var xml = ConfigData.VeryOldProfile;
        File.WriteAllText(_userPath, xml);

        var profiles = _profileManager.Profiles;
        Assert.Single(profiles);

        Assert.Equal("wia", profiles[0].DriverName);
        Assert.Equal("test_id", profiles[0].Device?.ID);
        Assert.Equal(200, profiles[0].Resolution.Dpi);
        Assert.Equal(2, profiles[0].Version);
        Assert.Equal(0, profiles[0].UpgradedFrom);
    }

    [Fact]
    public void LoadOldProfile()
    {
        var xml = ConfigData.OldProfile;
        File.WriteAllText(_userPath, xml);

        var profiles = _profileManager.Profiles;
        Assert.Single(profiles);

        Assert.Equal("wia", profiles[0].DriverName);
        Assert.Equal("test_id", profiles[0].Device?.ID);
        Assert.Equal(200, profiles[0].Resolution.Dpi);
        Assert.Equal(2, profiles[0].Version);
        Assert.Equal(1, profiles[0].UpgradedFrom);
    }

    [Fact]
    public void LoadOldTwainProfile()
    {
        var xml = ConfigData.OldTwainProfile;
        File.WriteAllText(_userPath, xml);

        var profiles = _profileManager.Profiles;
        Assert.Single(profiles);

        Assert.Equal("twain", profiles[0].DriverName);
        Assert.Equal("test_id", profiles[0].Device?.ID);
        Assert.True(profiles[0].UseNativeUI);
        Assert.Equal(2, profiles[0].Version);
        Assert.Equal(1, profiles[0].UpgradedFrom);
    }

    // TODO: Add tests for LockSystemProfiles etc
}