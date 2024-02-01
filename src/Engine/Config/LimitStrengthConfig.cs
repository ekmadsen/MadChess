// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using JetBrains.Annotations;


namespace ErikTheCoder.MadChess.Engine.Config;


[UsedImplicitly]
public sealed class LimitStrengthConfig
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public LimitStrengthEvalConfig Evaluation { get; set; }
    public LimitStrengthSearchConfig Search { get; set; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}
