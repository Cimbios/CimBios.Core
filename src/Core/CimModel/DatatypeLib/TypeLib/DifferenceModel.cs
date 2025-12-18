/*
*    CimBios.Core - Common Information Model (IEC61970) I/O Library
*    Copyright (C) 2025 Yuri A. Kovalenko a.k.a belizahrt <belizahrt@gmail.com>
*
*    This program is free software: you can redistribute it and/or modify
*    it under the terms of the GNU General Public License as published by
*    the Free Software Foundation, either version 3 of the License, or
*    (at your option) any later version.
*
*    This program is distributed in the hope that it will be useful,
*    but WITHOUT ANY WARRANTY; without even the implied warranty of
*    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*    GNU General Public License for more details.
*
*    You should have received a copy of the GNU General Public License
*    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using CimBios.Core.CimModel.DatatypeLib.ModelObject;
using CimBios.Core.CimModel.DatatypeLib.OID;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.DatatypeLib.TypeLib;

/// <summary>
/// </summary>
[CimClass(ClassUri)]
public class DifferenceModel(IOIDDescriptor oid, ICimMetaClass metaClass)
    : Model(oid, metaClass)
{
    public new const string ClassUri
        = "http://iec.ch/TC57/61970-552/DifferenceModel/1#DifferenceModel";

    public ICollection<IModelObject> forwardDifferences
    {
        get
        {
            var statementProperty = TryGetMetaPropertyByName(
                nameof(forwardDifferences));

            if (statementProperty != null
                && Statements.TryGetValue(statementProperty, out var statements))
                return statements;

            return [];
        }
    }

    public void AddTo_forwardDifferences(IModelObject modelObject)
    {
        var statementProperty = TryGetMetaPropertyByName(
            nameof(forwardDifferences));
        
        if (statementProperty != null) 
            AddToStatements(statementProperty, modelObject);
    }

    public ICollection<IModelObject> reverseDifferences
    {
        get
        {
            var statementProperty = TryGetMetaPropertyByName(
                nameof(reverseDifferences));

            if (statementProperty != null
                && Statements.TryGetValue(statementProperty, out var statements))
                return statements;

            return [];
        }
    }
    
    public void AddTo_reverseDifferences(IModelObject modelObject)
    {
        var statementProperty = TryGetMetaPropertyByName(
            nameof(reverseDifferences));
        
        if (statementProperty != null) 
            AddToStatements(statementProperty, modelObject);
    }
}
