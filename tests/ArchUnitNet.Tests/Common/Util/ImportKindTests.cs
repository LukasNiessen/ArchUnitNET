using ArchUnitNet.Common.Util;
using Xunit;

namespace ArchUnitNet.Tests.Common.Util;

public class ImportKindTests
{
    [Fact]
    public void Using_HasCorrectValue()
    {
        var kind = ImportKind.Using;
        Assert.Equal(ImportKind.Using, kind);
    }

    [Fact]
    public void GetDescription_ReturnsCorrectDescription()
    {
        Assert.Equal("using statement", ImportKind.Using.GetDescription());
        Assert.Equal("static using", ImportKind.StaticUsing.GetDescription());
        Assert.Equal("global using", ImportKind.GlobalUsing.GetDescription());
        Assert.Equal("alias using", ImportKind.AliasUsing.GetDescription());
        Assert.Equal("extern alias", ImportKind.ExternAlias.GetDescription());
    }

    [Fact]
    public void CanCombineFlags()
    {
        var combined = ImportKind.Using | ImportKind.StaticUsing;
        Assert.True(combined.HasFlag(ImportKind.Using));
        Assert.True(combined.HasFlag(ImportKind.StaticUsing));
    }

    [Fact]
    public void GetIndividualKinds_ReturnsAllCombinedKinds()
    {
        var combined = ImportKind.Using | ImportKind.StaticUsing | ImportKind.GlobalUsing;
        var individual = combined.GetIndividualKinds().ToList();

        Assert.Contains(ImportKind.Using, individual);
        Assert.Contains(ImportKind.StaticUsing, individual);
        Assert.Contains(ImportKind.GlobalUsing, individual);
        Assert.Equal(3, individual.Count);
    }

    [Fact]
    public void GetIndividualKinds_EmptyWhenNoKindsSet()
    {
        var empty = (ImportKind)0;
        var individual = empty.GetIndividualKinds().ToList();
        Assert.Empty(individual);
    }
}
