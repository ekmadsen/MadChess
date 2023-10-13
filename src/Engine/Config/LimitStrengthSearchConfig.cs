// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2023.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using JetBrains.Annotations;


namespace ErikTheCoder.MadChess.Engine.Config;


[UsedImplicitly]
public sealed class LimitStrengthSearchConfig
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public double NpsScale { get; set; }
    public double NpsPower { get; set; }
    public int NpsConstant { get; set; }
    public double MoveErrorScale { get; set; }
    public double MoveErrorPower { get; set; }
    public int MoveErrorConstant { get; set; }
    public double BlunderErrorScale { get; set; }
    public double BlunderErrorPower { get; set; }
    public int BlunderErrorConstant { get; set; }
    public double BlunderPer1024Scale { get; set; }
    public double BlunderPer1024Power { get; set; }
    public int BlunderPer1024Constant { get; set; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}
