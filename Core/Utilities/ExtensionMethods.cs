// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using JetBrains.Annotations;


namespace ErikTheCoder.MadChess.Core.Utilities;


public static class ExtensionMethods
{
    [UsedImplicitly]
    [ContractAnnotation("text: null => true")]
    public static bool IsNullOrEmpty(this string text) => string.IsNullOrEmpty(text);
}