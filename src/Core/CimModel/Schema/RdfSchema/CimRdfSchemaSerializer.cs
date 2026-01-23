/*
*    CimBios.Core - Common Information Model (IEC61970) I/O Library
*    Copyright (C) 2026 Yuri A. Kovalenko a.k.a belizahrt <belizahrt@gmail.com>
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
using System.Reflection;
using CimBios.Core.RdfIOLib;
using CimBios.Utils.MetaReflectionHelper;
using Serilog;

namespace CimBios.Core.CimModel.Schema.RdfSchema;

public class CimRdfSchemaSerializer(RdfReaderBase rdfReader, ILogger? logger=null)
    : ICimSchemaSerializer
{
    private const string RdfDescription = "http://www.w3.org/1999/02/22-rdf-syntax-ns#Description";
    
    private readonly Dictionary<string, Uri> _Namespaces = [];

    private readonly Dictionary<Uri, ICimMetaResource> _ObjectsCache
        = new(new RdfUriComparer());

    private readonly RdfReaderBase _RdfReader = rdfReader;

    private readonly MetaReflectionHelper _SerializeHelper
        = new(Assembly.GetExecutingAssembly());

    public ReadOnlyDictionary<string, Uri> Namespaces
        => _Namespaces.AsReadOnly();

    public void Load(TextReader reader)
    {
        logger?.ForContext<CimRdfSchemaSerializer>()
            .Information("Load schema from reader ...");

        _RdfReader.Load(reader);
    }

    public Dictionary<Uri, ICimMetaResource> Deserialize()
    {
        _Namespaces.Clear();
        _ObjectsCache.Clear();

        BuildInternalDatatypes();

        var descriptionTypedNodes = _RdfReader.ReadAll().ToArray();

        ReadObjects(descriptionTypedNodes);
        ResolveReferences(descriptionTypedNodes);

        BuildExternalDatatypes();

        ForwardReaderNamespaces();

        _RdfReader.Close();

        return _ObjectsCache;
    }

    /// <summary>
    ///     Move namespaces from reader doc.
    /// </summary>
    private void ForwardReaderNamespaces()
    {
        foreach (var item in _RdfReader.Namespaces)
            _Namespaces.Add(item.Key, item.Value);
    }

    /// <summary>
    ///     Serialization read of rdf description nodes.
    ///     <param name="descriptionTypedNodes">RdfNode object.</param>
    /// </summary>
    private void ReadObjects(IEnumerable<RdfNode> descriptionTypedNodes)
    {
        var unknownTypeNodes = new List<RdfNode>();

        foreach (var node in descriptionTypedNodes)
        {
            if (node.TypeIdentifier.AbsoluteUri == RdfDescription) continue;
            
            if (_ObjectsCache.ContainsKey(node.Identifier))
            {
                logger?.ForContext<CimRdfSchemaSerializer>()
                    .Warning("Duplicate definition of {iri} schema resource skipped",
                        node.Identifier);

                continue;
            }
            
            if (_SerializeHelper.TryGetTypeInfo(node.TypeIdentifier
                        .AbsoluteUri.ToLower(), out var typeInfo)
                    && typeInfo != null)
            {
                if (Activator.CreateInstance(typeInfo, node.Identifier)
                    is ICimRdfDescription instance)
                    _ObjectsCache.Add(node.Identifier, instance);
                else
                    logger?.ForContext<CimRdfSchemaSerializer>()
                        .Warning("Failed to create {iri} schema resource",
                            node.Identifier);
            }
            else
            {
                unknownTypeNodes.Add(node);
            }
        }

        CreateIndividuals(unknownTypeNodes);
    }

    /// <summary>
    ///     Create schema individuals.
    ///     <param name="nodes">List of description RDF nodes.</param>
    /// </summary>
    private void CreateIndividuals(IEnumerable<RdfNode> nodes)
    {
        foreach (var node in nodes)
        {
            if ((_ObjectsCache.TryGetValue(node.TypeIdentifier, out var entity)
                 && entity is CimRdfsClass metaClass) == false)
            {
                logger?.ForContext<CimRdfSchemaSerializer>().Debug("Unknown node type {@node}", node);
                continue;
            }

            _ObjectsCache.Add(node.Identifier,
                new CimRdfsIndividual(node.Identifier)
                {
                    InstanceOf = metaClass
                });
        }
    }

    /// <summary>
    ///     Fill property reference instances.
    ///     <param name="descriptionTypedNodes">List of description RDF nodes.</param>
    /// </summary>
    private void ResolveReferences(IEnumerable<RdfNode> descriptionTypedNodes)
    {
        foreach (var node in descriptionTypedNodes)
        {
            if (_ObjectsCache.TryGetValue(node.Identifier,
                    out var metaDescription) == false
                || metaDescription is ICimRdfDescription == false)
            {
                logger?.ForContext<CimRdfSchemaSerializer>()
                    .Error("Resolve error for {iri}: not found in dictionary",
                        node.Identifier);

                continue;
            }

            foreach (var triple in node.Triples)
            {
                var result = _SerializeHelper
                    .TryGetMemberInfo(triple.Predicate.AbsoluteUri.ToLower(),
                        out var memberInfo);

                if (result == false || memberInfo == null)
                {
                    logger?.ForContext<CimRdfSchemaSerializer>()
                        .Debug("Resolve error for {iri}: triple {triple} not found in serializable entities",
                            node.Identifier, triple.Predicate);

                    continue;
                }

                var attribute = memberInfo
                    .GetCustomAttribute<CimSchemaSerializableAttribute>(true);
                var value = triple.Object;

                if (attribute == null)
                {
                    logger?.ForContext<CimRdfSchemaSerializer>()
                        .Debug("Resolve error for {iri}: triple {triple} not found in serializable entities",
                            node.Identifier, triple.Predicate);

                    continue;
                }

                if (attribute.FieldType == MetaFieldType.ByRef
                    && value is RdfTripleObjectUriContainer valueRefUriContainer)
                {
                    if (_ObjectsCache.TryGetValue(valueRefUriContainer.UriObject,
                            out var description))
                        _SerializeHelper.SetMetaMemberValue(metaDescription,
                            memberInfo, description);
                    else
                        logger?.ForContext<CimRdfSchemaSerializer>()
                            .Error("Resolve error for {iri}: triple {triple} reference target {target} not found in dictionary",
                                node.Identifier, triple.Predicate, valueRefUriContainer.UriObject);
                }
                else if (attribute.FieldType == MetaFieldType.Value
                            && value is RdfTripleObjectLiteralContainer literalContainer)
                {
                    _SerializeHelper.SetMetaMemberValue(metaDescription,
                        memberInfo, literalContainer.LiteralObject);
                }
                else if (attribute.FieldType == MetaFieldType.Enum
                            && value is RdfTripleObjectUriContainer valueEnumUriContainer)
                {
                    _SerializeHelper.TryGetMemberInfo(valueEnumUriContainer
                        .UriObject.AbsoluteUri.ToLower(), out var field);
                    _SerializeHelper.TryGetTypeInfo(attribute.Identifier.ToLower(),
                        out var enumClass);

                    if (field != null && enumClass != null)
                    {
                        var enumValue = Enum.Parse(enumClass, field.Name);
                        _SerializeHelper.SetMetaMemberValue(metaDescription,
                            memberInfo, enumValue);
                    }
                    else
                    {
                        logger?.ForContext<CimRdfSchemaSerializer>()
                            .Debug("Resolve error for {iri}: triple {triple} reference enum {enum} is not serializable",
                                node.Identifier, triple.Predicate, valueEnumUriContainer.UriObject);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Build internal schema datatypes.
    /// </summary>
    private void BuildInternalDatatypes()
    {
        foreach (var typeUri in XmlDatatypesMapping.UriSystemTypes.Keys)
        {
            var datatype = XmlDatatypesMapping.UriSystemTypes[typeUri];

            var uri = new Uri(typeUri);
            RdfUtils.TryGetEscapedIdentifier(uri, out var label);
            var metaDatatype = new CimRdfsDatatype(uri)
            {
                SystemType = datatype,
                Label = label,
                Comment = "Build-in xsd datatype."
            };

            _ObjectsCache.Add(metaDatatype.BaseUri, metaDatatype);
        }

        // Build rdf:Statement
        var rdfStatement = new CimRdfsClass(CimRdfSchemaStrings.RdfStatement)
        {
            Label = "Statement"
        };
        rdfStatement.Stereotypes.Add(UMLStereotype.CIMAbstract);
        _ObjectsCache.Add(rdfStatement.BaseUri, rdfStatement);
    }

    /// <summary>
    ///     Build external schema-in datatypes.
    /// </summary>
    private void BuildExternalDatatypes()
    {
        foreach (var metaClass in _ObjectsCache.Values
                     .OfType<CimRdfsClass>().Where(o => o.IsDatatype))
        {
            var uri = metaClass.BaseUri;
            if (XmlDatatypesMapping.UriSystemTypes.ContainsKey(uri.AbsoluteUri)) continue;

            var valueProperty = _ObjectsCache.Values.OfType<CimRdfsProperty>()
                .Where(p => RdfUtils.RdfUriEquals(
                    p.BaseUri, new Uri(uri.AbsoluteUri + ".value")))
                .FirstOrDefault();

            var type = typeof(string);

            if (valueProperty?.Datatype is CimRdfsDatatype cimRdfsDatatype
                && cimRdfsDatatype.SystemType != null)
                type = cimRdfsDatatype.SystemType;
            else
                logger?.ForContext<CimRdfSchemaSerializer>()
                    .Error("Datatype {type} has not .value definition",
                        metaClass.BaseUri);

            var metaDatatype = new CimRdfsDatatype(metaClass)
            {
                SystemType = type
            };

            foreach (var targetProperty in _ObjectsCache.Values
                         .OfType<CimRdfsProperty>()
                         .Where(p => p.Datatype == metaClass))
                targetProperty.Datatype = metaDatatype;

            _ObjectsCache[uri] = metaDatatype;
        }
    }
}

/// <summary>
///     Pre-defined schemas URIs.
/// </summary>
public static class CimRdfSchemaStrings
{
    public static Uri RdfDescription =
        new("http://www.w3.org/1999/02/22-rdf-syntax-ns#Description");

    public static Uri RdfsResource =
        new("http://www.w3.org/2000/01/rdf-schema#Resource");

    public static Uri RdfType =
        new("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");

    public static Uri RdfsClass =
        new("http://www.w3.org/2000/01/rdf-schema#Class");

    public static Uri RdfProperty =
        new("http://www.w3.org/1999/02/22-rdf-syntax-ns#Property");

    public static Uri RdfStatement =
        new("http://www.w3.org/1999/02/22-rdf-syntax-ns#Statement");
}

public class CimRdfSchemaSerializerFactory (RdfReaderBase rdfReader,
    ILogger? logger = null) : ICimSchemaSerializerFactory
{
    public ICimSchemaSerializer CreateSerializer()
    {
        return new CimRdfSchemaSerializer(rdfReader, logger);
    }
}
