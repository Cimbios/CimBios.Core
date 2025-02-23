using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDatatypeLib;

/// <summary>
/// Interface provides statements parse type properies storage.
/// </summary>
public interface IStatementsContainer
{
    /// <summary>
    /// Dictionary of statements with key property.
    /// </summary>
    IReadOnlyDictionary<ICimMetaProperty, ICollection<IModelObject>> Statements 
    { get; }

    /// <summary>
    /// Add new statement to container.
    /// </summary>
    /// <param name="statementProperty">Meta property with statement type.</param>
    /// <param name="statement">Model object statement.</param>
    public void AddToStatements(ICimMetaProperty statementProperty,
        IModelObject statement);

    public void RemoveFromStatements(ICimMetaProperty statementProperty,
        IModelObject statement);
}
