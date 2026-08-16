using ArchUnitNet.Testing.Common;
using Xunit;

namespace ArchUnitNet.Tests.Testing.Common;

/// <summary>
/// Tests for the Colours utility class - ANSI colour codes and helpers.
/// </summary>
public class ColoursTests
{
    [Fact]
    public void Colours_Red_IsValidANSICode()
    {
        Assert.Equal("[31m", Colours.Red);
    }

    [Fact]
    public void Colours_Green_IsValidANSICode()
    {
        Assert.Equal("[32m", Colours.Green);
    }

    [Fact]
    public void Colours_Yellow_IsValidANSICode()
    {
        Assert.Equal("[33m", Colours.Yellow);
    }

    [Fact]
    public void Colours_Blue_IsValidANSICode()
    {
        Assert.Equal("[34m", Colours.Blue);
    }

    [Fact]
    public void Colours_Error_MapsToRed()
    {
        Assert.Equal(Colours.Red, Colours.Error);
    }

    [Fact]
    public void Colours_Success_MapsToGreen()
    {
        Assert.Equal(Colours.Green, Colours.Success);
    }

    [Fact]
    public void Colours_Warning_MapsToYellow()
    {
        Assert.Equal(Colours.Yellow, Colours.Warning);
    }

    [Fact]
    public void Colours_Info_MapsToBlue()
    {
        Assert.Equal(Colours.Blue, Colours.Info);
    }

    [Fact]
    public void Colours_Muted_MapsToGray()
    {
        Assert.Equal(Colours.Gray, Colours.Muted);
    }

    [Fact]
    public void Colorize_WithColorEnabled_WrapsTextWithCodes()
    {
        var result = Colours.Colorize("test", Colours.Red, enabled: true);
        Assert.Equal($"{Colours.Red}test{Colours.Reset}", result);
    }

    [Fact]
    public void Colorize_WithColorDisabled_ReturnsPlainText()
    {
        var result = Colours.Colorize("test", Colours.Red, enabled: false);
        Assert.Equal("test", result);
    }

    [Fact]
    public void Colorize_WithNullText_ReturnsEmptyString()
    {
        var result = Colours.Colorize(null, Colours.Red, enabled: true);
        Assert.Equal("", result);
    }

    [Fact]
    public void Colorize_WithEmptyText_ReturnsEmptyString()
    {
        var result = Colours.Colorize("", Colours.Red, enabled: true);
        Assert.Equal("", result);
    }

    [Fact]
    public void ColorizeBold_WithColorEnabled_IncludesBoldCode()
    {
        var result = Colours.ColorizeBold("test", Colours.Red, enabled: true);
        Assert.Equal($"{Colours.Red}{Colours.Bold}test{Colours.Reset}", result);
    }

    [Fact]
    public void ColorizeBold_WithColorDisabled_ReturnsPlainText()
    {
        var result = Colours.ColorizeBold("test", Colours.Red, enabled: false);
        Assert.Equal("test", result);
    }

    [Fact]
    public void Reset_IsValidANSICode()
    {
        Assert.Equal("[0m", Colours.Reset);
    }

    [Fact]
    public void Bold_IsValidANSICode()
    {
        Assert.Equal("[1m", Colours.Bold);
    }

    [Fact]
    public void Cyan_IsValidANSICode()
    {
        Assert.Equal("[36m", Colours.Cyan);
    }

    [Fact]
    public void Gray_IsValidANSICode()
    {
        Assert.Equal("[90m", Colours.Gray);
    }
}
