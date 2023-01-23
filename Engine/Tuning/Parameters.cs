// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2022.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace ErikTheCoder.MadChess.Engine.Tuning;


// Derive from Collection class in order to intercept insertions.
public sealed class Parameters : Collection<Parameter>
{
    private readonly Dictionary<string, int> _namesToIndices;


    public Parameters() => _namesToIndices = new Dictionary<string, int>();


    public Parameter this[string name] => this[_namesToIndices[name]];


    public Parameters DuplicateWithSameValues()
    {
        var parameters = new Parameters();

        for (var index = 0; index < Count; index++)
            parameters.Add(this[index].DuplicateWithSameValue());

        return parameters;
    }


    public Parameters DuplicateWithRandomValues()
    {
        var parameters = new Parameters();

        for (var index = 0; index < Count; index++)
            parameters.Add(this[index].DuplicateWithRandomValue());

        return parameters;
    }


    public void CopyValuesTo(Parameters parameters)
    {
        for (var index = 0; index < Count; index++)
            parameters[index].Value = this[index].Value;
    }


    protected override void InsertItem(int index, Parameter parameter)
    {
        base.InsertItem(index, parameter);
        _namesToIndices[parameter?.Name ?? string.Empty] = index;
    }
}