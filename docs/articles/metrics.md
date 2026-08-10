# Code Metrics Analysis

ArchUnitCSharp can analyze code quality metrics like LCOM (Lack of Cohesion of Methods) and cyclomatic complexity to ensure your classes maintain high cohesion.

## Overview

Metrics analysis works on compiled types extracted via Roslyn:

```csharp
var rule = Metrics()
    .Methods()                   // Analyze method metrics
    .LCOM96a()                  // Calculate LCOM96a score
    .ShouldBeLessThan(0.5);     // Enforce threshold

await rule.CheckAsync();
```

## LCOM (Lack of Cohesion of Methods)

LCOM measures how well methods in a class are related. **Lower LCOM = higher cohesion**.

### The Four LCOM Variants

ArchUnitCSharp supports all four LCOM formulas:

#### 1. LCOM1 (Chidamber & Kemerer, 1994)

**Formula**: Count pairs of methods with NO shared fields

```
LCOM1 = P - Q
where:
  P = number of method pairs with no common field access
  Q = number of method pairs with common field access
```

**Range**: 0 (perfect cohesion) to M*(M-1)/2 (no cohesion)

**Use case**: Conservative measure, good for detecting obvious problems

**Example**:
```
Class has 3 methods:
- Method1: accesses field1
- Method2: accesses field2
- Method3: accesses field3 (no overlap)

P = 3 (all pairs have no common fields)
Q = 0 (no pairs have common fields)
LCOM1 = 3
```

#### 2. LCOM96a (Hitz & Montazeri, 1996 - Variant A)

**Formula**: Normalized version of LCOM1

```
LCOM96a = (P - Q) / (M - 1)
where:
  M = number of methods
  P = method pairs with no common fields
  Q = method pairs with common fields
```

**Range**: 0.0 (perfect) to 1.0 (no cohesion)

**Use case**: Normalized, easier to set thresholds (typically 0.5)

**Example**:
```
Same class as above:
LCOM96a = (3 - 0) / (3 - 1) = 3/2 = 1.5 (clamped to 1.0 in normalized form)
```

#### 3. LCOM96b (Hitz & Montazeri, 1996 - Variant B)

**Formula**: Alternative normalization

```
LCOM96b = 1 - (Q / P) if P > 0, else 0
```

**Range**: 0.0 to 1.0

**Use case**: Emphasizes method grouping, complementary to LCOM96a

#### 4. LCOM1995 (Henderson-Sellers, 1996)

**Formula**: Field-access based normalization

```
LCOM1995 = (M - F/N) / (M - 1)
where:
  M = number of methods
  F = number of fields
  N = average number of fields accessed per method
```

**Range**: 0.0 to 1.0+

**Use case**: Accounts for class size, useful for large classes

## Basic Usage

### Threshold by LCOM96a

Most common pattern:

```csharp
var rule = Metrics()
    .Methods()
    .LCOM96a()
    .ShouldBeLessThan(0.5);

var violations = await rule.CheckAsync();
// violations: classes with LCOM96a >= 0.5 (low cohesion)
```

### Threshold by LCOM1

Conservative approach:

```csharp
var rule = Metrics()
    .Methods()
    .LCOM1()
    .ShouldBeLessThan(5);  // Threshold depends on class size

var violations = await rule.CheckAsync();
```

### Threshold by Count

Limit number of methods:

```csharp
var rule = Metrics()
    .Methods()
    .Count()
    .ShouldHaveAtMost(20);  // Enforce max method count

var violations = await rule.CheckAsync();
```

## Advanced Analysis

### Analyze Specific Types

```csharp
// Check a single class
var rule = Metrics()
    .Of(typeof(MyService))
    .Methods()
    .LCOM96a()
    .ShouldBeLessThan(0.5);

// Check all types in a namespace (future API)
var rule = Metrics()
    .InNamespace("MyApp.Services")
    .Methods()
    .LCOM96a()
    .ShouldBeLessThan(0.5);
```

### Different Thresholds by Violation Severity

```csharp
// Different rules for different criticality
var coreServicesRule = Metrics()
    .InNamespace("MyApp.Core")
    .Methods()
    .LCOM96a()
    .ShouldBeLessThan(0.3);  // Stricter

var utilityRule = Metrics()
    .InNamespace("MyApp.Utils")
    .Methods()
    .LCOM96a()
    .ShouldBeLessThan(0.7);  // Relaxed

var violations = new[]
{
    await coreServicesRule.CheckAsync(),
    await utilityRule.CheckAsync()
};
```

## Understanding Results

### Analyzing Low-Cohesion Classes

When LCOM is high (low cohesion), your class likely has multiple responsibilities:

```csharp
// Bad: Multiple independent method groups
public class UserService
{
    // Group 1: User management
    public void CreateUser(string name) { /* ... */ }
    public void UpdateUser(int id) { /* ... */ }
    
    // Group 2: Payment processing (unrelated!)
    public void ProcessPayment(decimal amount) { /* ... */ }
    public void RefundPayment(int transactionId) { /* ... */ }
    
    // Group 3: Email (also unrelated!)
    public void SendWelcomeEmail(string email) { /* ... */ }
    
    // LCOM96a = ~0.8 (high = low cohesion)
}
```

**Fix**: Split into smaller, focused classes:

```csharp
// Good: Each class has one responsibility
public class UserService
{
    public void CreateUser(string name) { /* ... */ }
    public void UpdateUser(int id) { /* ... */ }
}

public class PaymentProcessor
{
    public void ProcessPayment(decimal amount) { /* ... */ }
    public void RefundPayment(int transactionId) { /* ... */ }
}

public class EmailService
{
    public void SendWelcomeEmail(string email) { /* ... */ }
}

// Each has LCOM96a = ~0.2 (low = high cohesion)
```

### High-Cohesion Example

```csharp
// Good: Highly cohesive class
public class OrderCalculator
{
    private Order _order;
    private TaxService _taxService;
    
    // All methods work with the same data
    public decimal CalculateSubtotal() { /* uses _order */ }
    public decimal CalculateTax() { /* uses _order, _taxService */ }
    public decimal CalculateTotal() { /* uses _order */ }
    public bool IsEligibleForDiscount() { /* uses _order */ }
    
    // LCOM96a = ~0.1 (all methods share data)
}
```

## Performance Considerations

### Large Classes

For classes with 100+ methods, LCOM calculation can be slow. Optimize:

```csharp
// Option 1: Set higher threshold for large classes
var rule = Metrics()
    .Methods()
    .LCOM96a()
    .ShouldBeLessThan(0.8);  // Relaxed for legacy code

// Option 2: Analyze specific namespaces only
var rule = Metrics()
    .InNamespace("MyApp.Core")
    .Methods()
    .LCOM96a()
    .ShouldBeLessThan(0.5);

// Option 3: Exclude utility classes
var rule = Metrics()
    .OfTypesMatchingPattern(@".*Service$")  // Only service classes
    .Methods()
    .LCOM96a()
    .ShouldBeLessThan(0.5);
```

### Caching

Results are cached per analysis run:

```csharp
var rule = Metrics()
    .Methods()
    .LCOM96a()
    .ShouldBeLessThan(0.5);

var violations1 = await rule.CheckAsync();  // Full analysis
var violations2 = await rule.CheckAsync();  // Returns cached result
```

## Testing

Integrate metrics into your test suite:

```csharp
[TestFixture]
public class CodeQualityTests
{
    [Test]
    public async Task AllMethodsShouldHaveHighCohesion()
    {
        var rule = Metrics()
            .Methods()
            .LCOM96a()
            .ShouldBeLessThan(0.5);

        var violations = await rule.CheckAsync();
        Assert.That(violations, Is.Empty,
            $"Found {violations.Count} classes with low cohesion");
    }

    [Test]
    public async Task ServicesShouldHaveFewMethods()
    {
        var rule = Metrics()
            .InNamespace("MyApp.Services")
            .Methods()
            .Count()
            .ShouldHaveAtMost(20);

        var violations = await rule.CheckAsync();
        Assert.That(violations, Is.Empty,
            $"Found {violations.Count} oversized service classes");
    }

    [Test]
    public async Task NoClassShouldExceed100Methods()
    {
        var rule = Metrics()
            .Methods()
            .Count()
            .ShouldHaveAtMost(100);

        var violations = await rule.CheckAsync();
    }
}
```

## Recommended Thresholds

Based on industry practice:

| Threshold | Interpretation | Recommended For |
|-----------|-----------------|---|
| < 0.3 | Excellent cohesion | Core business logic |
| 0.3 - 0.5 | Good cohesion | Most application code |
| 0.5 - 0.7 | Fair cohesion | Utilities, helpers |
| > 0.7 | Poor cohesion | Refactor needed |

**Start with 0.5** and adjust based on your codebase maturity.

## Choosing Which LCOM Variant

| Variant | Best For | Threshold Guide |
|---------|----------|---|
| **LCOM1** | Conservative detection | > 5 (class size dependent) |
| **LCOM96a** | Normalized, comparable | < 0.5 (recommended) |
| **LCOM96b** | Alternative normalization | < 0.6 |
| **LCOM1995** | Large classes | < 0.4 |

**Recommendation**: Start with **LCOM96a** at threshold **0.5**.

## Limitations

1. **Magic numbers**: Find appropriate thresholds for your codebase
2. **Utility classes**: Even cohesive utility classes might show high LCOM
3. **Legacy code**: Threshold relaxation needed for older codebases
4. **Framework-generated code**: May have skewed metrics

## Comparison with Other Tools

| Tool | LCOM Variants | Threshold Tuning | IDE Integration |
|------|---|---|---|
| **ArchUnitCSharp** | 4 variants | Full control | Via rules |
| **NDepend** | Multiple | Limited | Visual Studio |
| **StyleCop** | None | N/A | Built-in |
| **Resharper** | Limited | Limited | Visual Studio |

---

See also:
- [Getting Started](getting-started.md) — Quick start guide
- [File-Based Rules](file-rules.md) — Dependency validation
- [Architecture Slicing](slicing.md) — Feature-based rules
- [Graph Visualization](graph-reporting.md) — Export dependencies
