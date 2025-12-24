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
using CimBios.Core.CimModel.RdfSerializer.DeserializationResult;

namespace CimBios.Core.CimModel.RdfSerializer;

public interface IRdfSerializer
{
    /// <summary>
    /// Output base namespace URI.
    /// </summary>
    public Uri BaseUri { get; set; }
    
    /// <summary>
    ///     Rdf serializer settings.
    /// </summary>
    public RdfSerializerSettings Settings { get; init; }
    
    /// <summary>
    ///     Deserialize data provider data to IModelObject instances.
    ///     <returns>Deserializer result object.</returns>
    /// </summary>
    public IDeserializationResult Deserialize(StreamReader streamReader);
    
    /// <summary>
    ///     Serialize IModelObject instances to data provider source.
    ///     <param name="modelObjects">IModelObject collection for serialization.</param>
    /// </summary>
    public void Serialize(StreamWriter streamWriter,
        IEnumerable<IModelObject> modelObjects);
}
