using ArchUnitNet.Metrics.Common;

namespace ArchUnitNet.Metrics.Calculation;

/// <summary>
/// Calculates multiple cohesion metrics (LCOM variants).
/// LCOM = Lack of Cohesion of Methods. Low values = high cohesion = good.
/// </summary>
public class LCOMCalculator
{
    private readonly ClassInfo _classInfo;

    public LCOMCalculator(ClassInfo classInfo)
    {
        _classInfo = classInfo ?? throw new ArgumentNullException(nameof(classInfo));
    }

    /// <summary>
    /// LCOM1 (Henderson-Sellers): Based on method pairs not sharing fields.
    /// Range: [0, 2]. Values > 1 indicate low cohesion.
    /// Formula: LCOM1 = (a - e) / (a - 1) where a = method pairs, e = pairs sharing fields
    /// </summary>
    public double CalculateLCOM1()
    {
        if (_classInfo.MethodCount <= 1)
            return 0; // No cohesion issues with 0-1 methods

        var methodPairs = GetMethodPairsNotSharingFields();
        var totalPairs = GetTotalMethodPairs();

        if (totalPairs == 0)
            return 0;

        return (double)methodPairs / totalPairs;
    }

    /// <summary>
    /// LCOM96a (Chidamber & Kemerer variant): Counts method pairs sharing at least one field.
    /// Range: [0, 1]. Values closer to 0 = higher cohesion.
    /// </summary>
    public double CalculateLCOM96a()
    {
        if (_classInfo.MethodCount <= 1 || _classInfo.FieldCount == 0)
            return 0;

        var matrix = _classInfo.BuildFieldAccessMatrix();
        var methodCount = _classInfo.MethodCount;

        // Count pairs of methods that share at least one field access
        var pairsSharing = CountPairsSharing(matrix);

        // Total possible pairs
        var totalPairs = (methodCount * (methodCount - 1)) / 2;

        if (totalPairs == 0)
            return 0;

        // Formula: LCOM96a = (totalPairs - pairsSharing) / totalPairs
        return (double)(totalPairs - pairsSharing) / totalPairs;
    }

    /// <summary>
    /// LCOM96b: Simplified variant considering methods accessing no fields.
    /// Range: [0, 1]. Penalizes isolated methods more heavily than LCOM96a.
    /// </summary>
    public double CalculateLCOM96b()
    {
        if (_classInfo.MethodCount <= 1 || _classInfo.FieldCount == 0)
            return 0;

        var lcom96a = CalculateLCOM96a();
        var isolatedCount = _classInfo.IsolatedMethodCount;

        // If many methods don't use any fields, increase the score
        var isolationPenalty = (double)isolatedCount / _classInfo.MethodCount;

        return Math.Max(lcom96a, isolationPenalty);
    }

    /// <summary>
    /// LCOM1995 (Original Chidamber & Kemerer): Field usage based.
    /// High values indicate low cohesion.
    /// </summary>
    public double CalculateLCOM1995()
    {
        if (_classInfo.MethodCount <= 1)
            return 0;

        var matrix = _classInfo.BuildFieldAccessMatrix();
        var fieldCount = _classInfo.FieldCount;
        var methodCount = _classInfo.MethodCount;

        if (fieldCount == 0)
            return 1.0; // All methods isolated

        // Count method pairs sharing no fields
        var pairsNotSharing = 0;

        for (int i = 0; i < methodCount; i++)
        {
            for (int j = i + 1; j < methodCount; j++)
            {
                bool shareField = false;
                for (int f = 0; f < fieldCount; f++)
                {
                    if (matrix[i, f] && matrix[j, f])
                    {
                        shareField = true;
                        break;
                    }
                }

                if (!shareField)
                    pairsNotSharing++;
            }
        }

        var totalPairs = (methodCount * (methodCount - 1)) / 2;
        return (double)pairsNotSharing / totalPairs;
    }

    private int GetMethodPairsNotSharingFields()
    {
        var matrix = _classInfo.BuildFieldAccessMatrix();
        var methodCount = _classInfo.MethodCount;
        var fieldCount = _classInfo.FieldCount;
        var pairsNotSharing = 0;

        for (int i = 0; i < methodCount; i++)
        {
            for (int j = i + 1; j < methodCount; j++)
            {
                bool shareField = false;
                for (int f = 0; f < fieldCount; f++)
                {
                    if (matrix[i, f] && matrix[j, f])
                    {
                        shareField = true;
                        break;
                    }
                }

                if (!shareField)
                    pairsNotSharing++;
            }
        }

        return pairsNotSharing;
    }

    private int GetTotalMethodPairs()
    {
        var methodCount = _classInfo.MethodCount;
        return (methodCount * (methodCount - 1)) / 2;
    }

    private int CountPairsSharing(bool[,] matrix)
    {
        var methodCount = _classInfo.MethodCount;
        var fieldCount = _classInfo.FieldCount;
        var pairsSharing = 0;

        for (int i = 0; i < methodCount; i++)
        {
            for (int j = i + 1; j < methodCount; j++)
            {
                bool shareField = false;
                for (int f = 0; f < fieldCount; f++)
                {
                    if (matrix[i, f] && matrix[j, f])
                    {
                        shareField = true;
                        break;
                    }
                }

                if (shareField)
                    pairsSharing++;
            }
        }

        return pairsSharing;
    }
}
