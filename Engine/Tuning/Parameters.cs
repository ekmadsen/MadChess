// +------------------------------------------------------------------------------+
// |                                                                              |
// |     MadChess is developed by Erik Madsen.  Copyright 2019.                   |
// |     MadChess is free software.  It is distributed under the GNU General      |
// |     Public License Version 3 (GPLv3).  See LICENSE file for details.         |
// |     See https://www.madchess.net/ for user and developer guides.             |
// |                                                                              |
// +------------------------------------------------------------------------------+


using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace ErikTheCoder.MadChess.Engine.Tuning
{
    // Derive from Collection class in order to intercept insertions.
    public sealed class Parameters : Collection<Parameter>
    {
        private readonly Dictionary<string, int> _namesToIndices;


        public Parameters() => _namesToIndices = new Dictionary<string, int>();


        public Parameter this[string Name] => this[_namesToIndices[Name]];


        public Parameters DuplicateWithSameValues()
        {
            Parameters parameters = new Parameters();
            for (int index = 0; index < Count; index++) parameters.Add(this[index].DuplicateWithSameValue());
            return parameters;
        }


        public Parameters DuplicateWithRandomValues()
        {
            Parameters parameters = new Parameters();
            for (int index = 0; index < Count; index++) parameters.Add(this[index].DuplicateWithRandomValue());
            return parameters;
        }


        public void CopyValuesTo(Parameters Parameters)
        {
            for (int index = 0; index < Count; index++) Parameters[index].Value = this[index].Value;
        }


        protected override void InsertItem(int Index, Parameter Parameter)
        {
            base.InsertItem(Index, Parameter);
            _namesToIndices[Parameter.Name] = Index;
        }
    }
}
