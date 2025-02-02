namespace CimBios.Core.CimModel.RdfSerializer;

/// <summary>
/// Rdf serializer settings.
/// </summary>
/// <param name="unknownClassesAllowed">Create auto class instances 
/// of unknown classes.</param>
/// <param name="unknownPropertiesAllowed">Create and read 
/// auto property instances.</param>
/// <param name="includeUnresolvedReferences">Include unresolved references 
/// for objects while read/write.</param>
/// <param name="iriMode">rdf:_iri_ mode.</param>
public sealed class RdfSerializerSettings(
    bool unknownClassesAllowed = false,
    bool unknownPropertiesAllowed = false,
    bool includeUnresolvedReferences = true,
    RdfSerializerIRIMode iriMode = RdfSerializerIRIMode.About)
{
    /// <summary>
    /// Create auto class instances of unknown classes.
    /// </summary>
    public bool UnknownClassesAllowed { get; set; } 
        = unknownClassesAllowed;

    /// <summary>
    /// Create and read auto property instances.
    /// </summary>
    public bool UnknownPropertiesAllowed { get; set; } 
        = unknownPropertiesAllowed;

    /// <summary>
    /// Include unresolved references for objects while read/write.
    /// </summary>
    public bool IncludeUnresolvedReferences { get; set; } 
        = includeUnresolvedReferences;

    /// <summary>
    /// rdf:_iri_ mode.
    /// </summary>
    public RdfSerializerIRIMode IRIMode  { get; set; } = iriMode;
}

/// <summary>
/// rdf:_iri_ mode.
/// </summary>
public enum RdfSerializerIRIMode
{
    About,
    ID
}
