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

using System.Collections.ObjectModel;

namespace CimBios.Core.CimModel.Schema;

/// <summary>
/// Root interface cim schema serializer.
/// </summary>
public interface ICimSchemaSerializer
{
    /// <summary>
    /// Prefix to namespace URI mapping for schema.
    /// </summary>
    public ReadOnlyDictionary <string, Uri> Namespaces { get; }

    /// <summary>
    /// Load raw schema text data.
    /// </summary>
    public void Load(TextReader reader);

    /// <summary>
    /// Deserialize data to CIM schema.
    /// </summary>
    public Dictionary<Uri, ICimMetaResource> Deserialize();
}

public interface ICimSchemaSerializerFactory
{
    /// <summary>
    /// Factory method creates concrete serializer instance.
    /// </summary>
    /// <param name="rdfReader">Rdf reader provider.</param>
    /// <returns>Serializer instance.</returns>
    public ICimSchemaSerializer CreateSerializer();
}
