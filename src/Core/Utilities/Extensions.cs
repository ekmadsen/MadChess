// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System.Collections.Generic;
using JetBrains.Annotations;


namespace ErikTheCoder.MadChess.Core.Utilities;


public static class Extensions
{
    [ContractAnnotation("text: null => true")]
    public static bool IsNullOrEmpty(this string text) => string.IsNullOrEmpty(text);


    [ContractAnnotation("list: null => true")]
    public static bool IsNullOrEmpty<T>(this List<T> list) => (list == null) || (list.Count == 0);
}