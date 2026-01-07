using FluentAssertions;
using FrameworkAPI.Helpers;
using FrameworkAPI.Schema.Settings.DashboardSettings;
using Xunit;
using Settings = WuH.Ruby.Settings.Client;

namespace FrameworkAPI.Test.Helpers;

public class DashboardWidgetSettingsMapperTests
{
    [Fact]
    public void MapToInternalDashboardWidgetSettings()
    {
        // Arrange
        var dashboardWidgetSettings = new DashboardWidgetSettings()
        {
            WidgetCatalogId = "FakeId",
            MachineIds = ["EQ12345"],
            AdditionalSetting = "MySetting"
        };
        var expectedDashboardWidgetSettings = new Settings.Models.DashboardWidgetSettings()
        {
            WidgetCatalogId = "FakeId",
            MachineIds = ["EQ12345"],
            AdditionalSetting = "MySetting"
        };

        // Act
        var result = dashboardWidgetSettings.MapToInternalDashboardWidgetSettings();

        // Assert
        result.Should().BeEquivalentTo(expectedDashboardWidgetSettings);
    }
}