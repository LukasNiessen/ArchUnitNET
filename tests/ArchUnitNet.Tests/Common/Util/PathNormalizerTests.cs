using ArchUnitNet.Common.Util;

namespace ArchUnitNet.Tests.Common.Util;

public class PathNormalizerTests
{
    public class NormalizeTests
    {
        [Theory]
        [InlineData("src/Common/Error", "src/Common/Error")]
        [InlineData("src\\Common\\Error", "src/Common/Error")]
        [InlineData("src/Common\\Error", "src/Common/Error")]
        [InlineData("src\\Common/Error", "src/Common/Error")]
        public void Normalize_ConvertBackslashesToForwardSlashes(string input, string expected)
        {
            var result = PathNormalizer.Normalize(input);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("./src/Common", "src/Common")]
        [InlineData("./src/./Common", "src/Common")]
        [InlineData("src/./Common", "src/Common")]
        public void Normalize_RemovesDotDirectory(string input, string expected)
        {
            var result = PathNormalizer.Normalize(input);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("src/../Common", "Common")]
        [InlineData("src/Common/../Error", "src/Error")]
        [InlineData("src/Common/../../Error", "Error")]
        [InlineData("a/b/c/../../d", "a/d")]
        public void Normalize_ResolvesParentDirectory(string input, string expected)
        {
            var result = PathNormalizer.Normalize(input);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("src//Common", "src/Common")]
        [InlineData("src///Common", "src/Common")]
        [InlineData("src////Common", "src/Common")]
        public void Normalize_RemovesDuplicateSlashes(string input, string expected)
        {
            var result = PathNormalizer.Normalize(input);
            result.Should().Be(expected);
        }

        [Fact]
        public void Normalize_HandlesUNCPath()
        {
            var uncPath = "\\\\server\\share\\folder";
            var result = PathNormalizer.Normalize(uncPath);
            result.Should().Be("//server/share/folder");
        }

        [Fact]
        public void Normalize_ThrowsOnNullPath()
        {
            var action = () => PathNormalizer.Normalize(null!);
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Normalize_ThrowsOnEmptyPath()
        {
            var action = () => PathNormalizer.Normalize("");
            action.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData("C:\\src\\Common", "C:/src/Common")]
        [InlineData("C:/src/Common", "C:/src/Common")]
        public void Normalize_HandlesAbsoluteWindowsPaths(string input, string expected)
        {
            var result = PathNormalizer.Normalize(input);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("/src/Common", "/src/Common")]
        [InlineData("/src\\Common", "/src/Common")]
        public void Normalize_HandlesAbsoluteUnixPaths(string input, string expected)
        {
            var result = PathNormalizer.Normalize(input);
            result.Should().Be(expected);
        }
    }

    public class RemoveTrailingSlashTests
    {
        [Theory]
        [InlineData("src/Common/", "src/Common")]
        [InlineData("src/Common", "src/Common")]
        [InlineData("/", "/")]
        [InlineData("", "")]
        public void RemoveTrailingSlash_RemovesSlashIfPresent(string input, string expected)
        {
            var result = PathNormalizer.RemoveTrailingSlash(input);
            result.Should().Be(expected);
        }
    }

    public class EnsureTrailingSlashTests
    {
        [Theory]
        [InlineData("src/Common", "src/Common/")]
        [InlineData("src/Common/", "src/Common/")]
        [InlineData("", "")]
        public void EnsureTrailingSlash_AddsSlashIfMissing(string input, string expected)
        {
            var result = PathNormalizer.EnsureTrailingSlash(input);
            result.Should().Be(expected);
        }
    }

    public class GetDirectoryTests
    {
        [Theory]
        [InlineData("src/Common/Error.cs", "src/Common")]
        [InlineData("src/Common", "src")]
        [InlineData("Error.cs", "")]
        [InlineData("", "")]
        [InlineData("/Error.cs", "/")]
        public void GetDirectory_ReturnsDirectoryPart(string input, string expected)
        {
            var result = PathNormalizer.GetDirectory(input);
            result.Should().Be(expected);
        }
    }

    public class GetFileNameTests
    {
        [Theory]
        [InlineData("src/Common/Error.cs", "Error.cs")]
        [InlineData("src/Common", "Common")]
        [InlineData("Error.cs", "Error.cs")]
        [InlineData("", "")]
        public void GetFileName_ReturnsFileNamePart(string input, string expected)
        {
            var result = PathNormalizer.GetFileName(input);
            result.Should().Be(expected);
        }
    }

    public class GetRelativePathTests
    {
        [Theory]
        [InlineData("src", "src/Common/Error.cs", "Common/Error.cs")]
        [InlineData("src/", "src/Common/Error.cs", "Common/Error.cs")]
        [InlineData("src", "src", "")]
        public void GetRelativePath_ReturnsRelativePath(string baseDir, string targetPath, string expected)
        {
            var result = PathNormalizer.GetRelativePath(baseDir, targetPath);
            result.Should().Be(expected);
        }

        [Fact]
        public void GetRelativePath_ThrowsWhenTargetNotUnderBase()
        {
            var action = () => PathNormalizer.GetRelativePath("src", "tests/Common/Error.cs");
            action.Should().Throw<ArgumentException>();
        }
    }

    public class IsAbsoluteTests
    {
        [Theory]
        [InlineData("/src/Common", true)]
        [InlineData("C:/src/Common", true)]
        [InlineData("C:\\src\\Common", true)]
        [InlineData("src/Common", false)]
        [InlineData("", false)]
        public void IsAbsolute_DetectsAbsolutePath(string input, bool expected)
        {
            var result = PathNormalizer.IsAbsolute(input);
            result.Should().Be(expected);
        }
    }

    public class EdgeCasesTests
    {
        [Fact]
        public void Complex_MixedSeparatorsAndRelativeReferences()
        {
            var input = "src\\Common\\..\\Files\\./FluentApi";
            var result = PathNormalizer.Normalize(input);
            result.Should().Be("src/Files/FluentApi");
        }

        [Fact]
        public void Complex_MultipleParentDirectoryTraversals()
        {
            var input = "a/b/c/d/../../../../e";
            var result = PathNormalizer.Normalize(input);
            result.Should().Be("e");
        }

        [Fact]
        public void Complex_ExcessiveParentDirectoryReferences()
        {
            // Going up more than the depth — should just stop at root
            var input = "a/b/../../../../../../c";
            var result = PathNormalizer.Normalize(input);
            result.Should().Be("c");
        }
    }
}
