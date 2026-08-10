using ArchUnitNet.Metrics.Calculation;
using ArchUnitNet.Metrics.Common;
using Xunit;

namespace ArchUnitNet.Tests.Metrics.Calculation;

public class LCOMCalculatorTests
{
    [Fact]
    public void CalculateLCOM96a_HighCohesion_ReturnsLowValue()
    {
        // Arrange: All methods access all fields (perfect cohesion)
        var fields = new[]
        {
            new FieldInfo("x", "int"),
            new FieldInfo("y", "int")
        };
        var methods = new[]
        {
            new MethodInfo("method1", new HashSet<string> { "x", "y" }),
            new MethodInfo("method2", new HashSet<string> { "x", "y" }),
            new MethodInfo("method3", new HashSet<string> { "x", "y" })
        };
        var classInfo = new ClassInfo("HighCohesionClass", fields, methods);
        var calculator = new LCOMCalculator(classInfo);

        // Act
        var lcom96a = calculator.CalculateLCOM96a();

        // Assert
        Assert.Equal(0.0, lcom96a, precision: 2);
    }

    [Fact]
    public void CalculateLCOM96a_LowCohesion_ReturnsHighValue()
    {
        // Arrange: Methods access disjoint sets of fields (low cohesion)
        var fields = new[]
        {
            new FieldInfo("x", "int"),
            new FieldInfo("y", "int"),
            new FieldInfo("z", "int"),
            new FieldInfo("w", "int")
        };
        var methods = new[]
        {
            new MethodInfo("method1", new HashSet<string> { "x", "y" }),
            new MethodInfo("method2", new HashSet<string> { "z", "w" })
        };
        var classInfo = new ClassInfo("LowCohesionClass", fields, methods);
        var calculator = new LCOMCalculator(classInfo);

        // Act
        var lcom96a = calculator.CalculateLCOM96a();

        // Assert
        Assert.Equal(1.0, lcom96a, precision: 2);
    }

    [Fact]
    public void CalculateLCOM96a_MixedCohesion_ReturnsMidValue()
    {
        // Arrange: Some methods share fields, some don't
        var fields = new[]
        {
            new FieldInfo("x", "int"),
            new FieldInfo("y", "int"),
            new FieldInfo("z", "int")
        };
        var methods = new[]
        {
            new MethodInfo("method1", new HashSet<string> { "x", "y" }),
            new MethodInfo("method2", new HashSet<string> { "x", "z" }),
            new MethodInfo("method3", new HashSet<string> { "y", "z" })
        };
        var classInfo = new ClassInfo("MixedCohesionClass", fields, methods);
        var calculator = new LCOMCalculator(classInfo);

        // Act
        var lcom96a = calculator.CalculateLCOM96a();

        // Assert
        Assert.InRange(lcom96a, 0.2, 0.8);
    }

    [Fact]
    public void CalculateLCOM96a_NoFields_ReturnsZero()
    {
        // Arrange: Class with no fields
        var methods = new[]
        {
            new MethodInfo("method1", new HashSet<string>()),
            new MethodInfo("method2", new HashSet<string>())
        };
        var classInfo = new ClassInfo("NoFieldsClass", fields: null, methods);
        var calculator = new LCOMCalculator(classInfo);

        // Act
        var lcom96a = calculator.CalculateLCOM96a();

        // Assert
        Assert.Equal(0.0, lcom96a);
    }

    [Fact]
    public void CalculateLCOM96a_SingleMethod_ReturnsZero()
    {
        // Arrange: Class with only one method
        var fields = new[]
        {
            new FieldInfo("x", "int"),
            new FieldInfo("y", "int")
        };
        var methods = new[]
        {
            new MethodInfo("method1", new HashSet<string> { "x" })
        };
        var classInfo = new ClassInfo("SingleMethodClass", fields, methods);
        var calculator = new LCOMCalculator(classInfo);

        // Act
        var lcom96a = calculator.CalculateLCOM96a();

        // Assert
        Assert.Equal(0.0, lcom96a);
    }

    [Fact]
    public void CalculateLCOM96b_WithIsolatedMethods_ReturnsPenalizedValue()
    {
        // Arrange: Some methods don't access any fields
        var fields = new[]
        {
            new FieldInfo("x", "int"),
            new FieldInfo("y", "int")
        };
        var methods = new[]
        {
            new MethodInfo("method1", new HashSet<string> { "x", "y" }),
            new MethodInfo("method2", new HashSet<string>()), // Isolated
            new MethodInfo("method3", new HashSet<string>()) // Isolated
        };
        var classInfo = new ClassInfo("IsolatedMethodsClass", fields, methods);
        var calculator = new LCOMCalculator(classInfo);

        // Act
        var lcom96b = calculator.CalculateLCOM96b();

        // Assert
        Assert.True(lcom96b > 0, "Isolated methods should increase LCOM96b");
        Assert.InRange(lcom96b, 0.6, 1.0);
    }

    [Fact]
    public void CalculateLCOM1_HighCohesion_ReturnsLowValue()
    {
        // Arrange: All methods share fields
        var fields = new[]
        {
            new FieldInfo("x", "int"),
            new FieldInfo("y", "int")
        };
        var methods = new[]
        {
            new MethodInfo("method1", new HashSet<string> { "x" }),
            new MethodInfo("method2", new HashSet<string> { "x" }),
            new MethodInfo("method3", new HashSet<string> { "x" })
        };
        var classInfo = new ClassInfo("SharedFieldClass", fields, methods);
        var calculator = new LCOMCalculator(classInfo);

        // Act
        var lcom1 = calculator.CalculateLCOM1();

        // Assert
        Assert.Equal(0.0, lcom1, precision: 2);
    }

    [Fact]
    public void CalculateLCOM1_LowCohesion_ReturnsHighValue()
    {
        // Arrange: Methods don't share any fields
        var fields = new[]
        {
            new FieldInfo("x", "int"),
            new FieldInfo("y", "int")
        };
        var methods = new[]
        {
            new MethodInfo("method1", new HashSet<string> { "x" }),
            new MethodInfo("method2", new HashSet<string> { "y" })
        };
        var classInfo = new ClassInfo("DisjointFieldsClass", fields, methods);
        var calculator = new LCOMCalculator(classInfo);

        // Act
        var lcom1 = calculator.CalculateLCOM1();

        // Assert
        Assert.Equal(1.0, lcom1, precision: 2);
    }

    [Fact]
    public void FieldAccessMatrix_BuildsCorrectMatrix()
    {
        // Arrange
        var fields = new[]
        {
            new FieldInfo("x", "int"),
            new FieldInfo("y", "int")
        };
        var methods = new[]
        {
            new MethodInfo("method1", new HashSet<string> { "x" }),
            new MethodInfo("method2", new HashSet<string> { "y" })
        };
        var classInfo = new ClassInfo("MatrixTestClass", fields, methods);

        // Act
        var matrix = classInfo.BuildFieldAccessMatrix();

        // Assert
        Assert.Equal(2, matrix.GetLength(0)); // 2 methods
        Assert.Equal(2, matrix.GetLength(1)); // 2 fields
        Assert.True(matrix[0, 0]); // method1 accesses x
        Assert.False(matrix[0, 1]); // method1 doesn't access y
        Assert.False(matrix[1, 0]); // method2 doesn't access x
        Assert.True(matrix[1, 1]); // method2 accesses y
    }
}
