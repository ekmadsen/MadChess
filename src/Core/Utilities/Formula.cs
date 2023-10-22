using System;
using System.Diagnostics;

namespace ErikTheCoder.MadChess.Core.Utilities;

public static class Formula
{
    public static int GetLinearlyInterpolatedValue(int minValue, int maxValue, int correlatedValue, int minCorrelatedValue, int maxCorrelatedValue)
    {
        Debug.Assert(maxValue >= minValue);
        Debug.Assert(maxCorrelatedValue >= minCorrelatedValue);

        var valueRange = maxValue - minValue;
        double correlatedRange = maxCorrelatedValue - minCorrelatedValue;
        correlatedValue = Math.Clamp(correlatedValue, minCorrelatedValue, maxCorrelatedValue);
        var fraction = (correlatedValue - minCorrelatedValue) / correlatedRange;

        return (int)(minValue + (fraction * valueRange));
    }

    public static int GetNonLinearBonus(double bonus, double scale, double power, int constant) => (int)(scale * Math.Pow(bonus, power)) + constant;
}