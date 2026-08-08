using ArchUnitNet.Common.Util;

namespace ArchUnitNet.Tests.Common.Util;

public class ImportKindTests
{
    [Fact]
    public void Using_HasCorrectValue()
    {
        var kind = ImportKind.Using;
        kind.Should().Be(ImportKind.Using);
    }

    [Fact]
    public void GetDescription_ReturnsCorrectDescription()
    {
        ImportKind.Using.GetDescription().Should().Be("using statement");
        ImportKind.StaticUsing.GetDescription().Should().Be("static using");
        ImportKind.GlobalUsing.GetDescription().Should().Be("global using");
        ImportKind.AliasUsing.GetDescription().Should().Be("alias using");
        ImportKind.ExternAlias.GetDescription().Should().Be("extern alias");
    }

    [Fact]
    public void CanCombineFlags()
    {
        var combined = ImportKind.Using | ImportKind.StaticUsing;
        combined.Should().HaveFlag(ImportKind.Using);
        combined.Should().HaveFlag(ImportKind.StaticUsing);
    }

    [Fact]
    public void GetIndividualKinds_ReturnsAllCombinedKinds()
    {
        var combined = ImportKind.Using | ImportKind.StaticUsing | ImportKind.GlobalUsing;
        var individual = combined.GetIndividualKinds().ToList();

        individual.Should().Contain(ImportKind.Using);
        individual.Should().Contain(ImportKind.StaticUsing);
        individual.Should().Contain(ImportKind.GlobalUsing);
        individual.Should().HaveCount(3);
    }

    [Fact]
    public void GetIndividualKinds_EmptyWhenNoKindsSet()
    {
        var empty = (ImportKind)0;
        var individual = empty.GetIndividualKinds().ToList();
        individual.Should().BeEmpty();
    }
}
