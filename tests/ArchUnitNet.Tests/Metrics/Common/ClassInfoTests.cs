using ArchUnitNet.Metrics.Common;
using Xunit;

namespace ArchUnitNet.Tests.Metrics.Common;

public class ClassInfoTests
{
    [Fact]
    public void Constructor_WithNameOnly_CreatesEmptyClass()
    {
        // Act
        var classInfo = new ClassInfo("EmptyClass");

        // Assert
        Assert.Equal("EmptyClass", classInfo.Name);
        Assert.Empty(classInfo.Fields);
        Assert.Empty(classInfo.Methods);
        Assert.Equal(0, classInfo.FieldCount);
        Assert.Equal(0, classInfo.MethodCount);
    }

    [Fact]
    public void Constructor_WithFieldsAndMethods_StoresCorrectly()
    {
        // Arrange
        var fields = new[] { new FieldInfo("x", "int"), new FieldInfo("y", "int") };
        var methods = new[] { new MethodInfo("getX", new HashSet<string> { "x" }) };

        // Act
        var classInfo = new ClassInfo("TestClass", fields, methods);

        // Assert
        Assert.Equal(2, classInfo.FieldCount);
        Assert.Single(classInfo.Methods);
        Assert.Equal(2, classInfo.Fields.Count);
        Assert.Equal(1, classInfo.MethodCount);
    }

    [Fact]
    public void GetField_WithValidName_ReturnsField()
    {
        // Arrange
        var fields = new[] { new FieldInfo("x", "int"), new FieldInfo("y", "int") };
        var classInfo = new ClassInfo("TestClass", fields);

        // Act
        var field = classInfo.GetField("x");

        // Assert
        Assert.NotNull(field);
        Assert.Equal("x", field!.Name);
    }

    [Fact]
    public void GetField_WithInvalidName_ReturnsNull()
    {
        // Arrange
        var fields = new[] { new FieldInfo("x", "int") };
        var classInfo = new ClassInfo("TestClass", fields);

        // Act
        var field = classInfo.GetField("z");

        // Assert
        Assert.Null(field);
    }

    [Fact]
    public void GetMethod_WithValidName_ReturnsMethod()
    {
        // Arrange
        var methods = new[] { new MethodInfo("method1", new HashSet<string>()) };
        var classInfo = new ClassInfo("TestClass", methods: methods);

        // Act
        var method = classInfo.GetMethod("method1");

        // Assert
        Assert.NotNull(method);
        Assert.Equal("method1", method!.Name);
    }

    [Fact]
    public void IsolatedMethodCount_WithUnconnectedMethods_ReturnsCorrectCount()
    {
        // Arrange
        var fields = new[] { new FieldInfo("x", "int") };
        var methods = new[]
        {
            new MethodInfo("method1", new HashSet<string> { "x" }),
            new MethodInfo("method2", new HashSet<string>()), // Isolated
            new MethodInfo("method3", new HashSet<string>()) // Isolated
        };
        var classInfo = new ClassInfo("TestClass", fields, methods);

        // Act
        var isolatedCount = classInfo.IsolatedMethodCount;

        // Assert
        Assert.Equal(2, isolatedCount);
    }

    [Fact]
    public void BuildFieldAccessMatrix_CreatesCorrectMatrix()
    {
        // Arrange
        var fields = new[]
        {
            new FieldInfo("x", "int"),
            new FieldInfo("y", "int"),
            new FieldInfo("z", "int")
        };
        var methods = new[]
        {
            new MethodInfo("m1", new HashSet<string> { "x", "y" }),
            new MethodInfo("m2", new HashSet<string> { "y", "z" }),
            new MethodInfo("m3", new HashSet<string> { "x" })
        };
        var classInfo = new ClassInfo("TestClass", fields, methods);

        // Act
        var matrix = classInfo.BuildFieldAccessMatrix();

        // Assert
        Assert.Equal(3, matrix.GetLength(0)); // 3 methods
        Assert.Equal(3, matrix.GetLength(1)); // 3 fields

        // m1 accesses x and y
        Assert.True(matrix[0, 0]); // x
        Assert.True(matrix[0, 1]); // y
        Assert.False(matrix[0, 2]); // z

        // m2 accesses y and z
        Assert.False(matrix[1, 0]); // x
        Assert.True(matrix[1, 1]); // y
        Assert.True(matrix[1, 2]); // z

        // m3 accesses only x
        Assert.True(matrix[2, 0]); // x
        Assert.False(matrix[2, 1]); // y
        Assert.False(matrix[2, 2]); // z
    }

    [Fact]
    public void ToString_ReturnsClassName()
    {
        // Arrange
        var classInfo = new ClassInfo("MyClass");

        // Act
        var str = classInfo.ToString();

        // Assert
        Assert.Equal("MyClass", str);
    }
}

public class MethodInfoTests
{
    [Fact]
    public void Constructor_StoresNameAndFields()
    {
        // Act
        var methodInfo = new MethodInfo("method1", new HashSet<string> { "x", "y" });

        // Assert
        Assert.Equal("method1", methodInfo.Name);
        Assert.Equal(2, methodInfo.FieldAccessCount);
    }

    [Fact]
    public void AccessesField_WithValidField_ReturnsTrue()
    {
        // Arrange
        var methodInfo = new MethodInfo("method1", new HashSet<string> { "x", "y" });

        // Act
        var accesses = methodInfo.AccessesField("x");

        // Assert
        Assert.True(accesses);
    }

    [Fact]
    public void AccessesField_WithInvalidField_ReturnsFalse()
    {
        // Arrange
        var methodInfo = new MethodInfo("method1", new HashSet<string> { "x" });

        // Act
        var accesses = methodInfo.AccessesField("z");

        // Assert
        Assert.False(accesses);
    }

    [Fact]
    public void FieldAccessCount_ReturnsCorrectCount()
    {
        // Arrange
        var methodInfo = new MethodInfo("method1", new HashSet<string> { "x", "y", "z" });

        // Act
        var count = methodInfo.FieldAccessCount;

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public void ToString_ReturnsMethodName()
    {
        // Arrange
        var methodInfo = new MethodInfo("getData", new HashSet<string>());

        // Act
        var str = methodInfo.ToString();

        // Assert
        Assert.Equal("getData", str);
    }
}

public class FieldInfoTests
{
    [Fact]
    public void Constructor_StoresNameAndType()
    {
        // Act
        var fieldInfo = new FieldInfo("count", "int");

        // Assert
        Assert.Equal("count", fieldInfo.Name);
        Assert.Equal("int", fieldInfo.Type);
        Assert.False(fieldInfo.IsPublic);
    }

    [Fact]
    public void Constructor_WithPublicFlag_StoresCorrectly()
    {
        // Act
        var fieldInfo = new FieldInfo("data", "string", IsPublic: true);

        // Assert
        Assert.True(fieldInfo.IsPublic);
    }

    [Fact]
    public void ToString_ReturnsFieldName()
    {
        // Arrange
        var fieldInfo = new FieldInfo("value", "double");

        // Act
        var str = fieldInfo.ToString();

        // Assert
        Assert.Equal("value", str);
    }
}
