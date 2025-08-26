namespace CimBios.Core.CimModel.CimDatatypeLib.OID;

/// <summary>
///     Model object identifier descriptor interface.
/// </summary>
public interface IOIDDescriptor : IComparable, IEquatable<IOIDDescriptor>
{
    /// <summary>
    ///     Full absolute uri formatted descriptor.
    /// </summary>
    public Uri AbsoluteOID { get; }

    /// <summary>
    ///     Is empty value OID.
    /// </summary>
    public bool IsEmpty { get; }

    /// <summary>
    ///     Not null string representation.
    /// </summary>
    /// <returns>Text return.</returns>
    public string ToString();
}

/// <summary>
///     OID Descriptor creation factory interface.
/// </summary>
public interface IOIDDescriptorFactory
{
    public string Namespace { get; }

    public IOIDDescriptor Create();
    public IOIDDescriptor Create(string value);
    public IOIDDescriptor Create(Uri value);

    public IOIDDescriptor? TryCreate();
    public IOIDDescriptor? TryCreate(string value);
    public IOIDDescriptor? TryCreate(Uri value);
}