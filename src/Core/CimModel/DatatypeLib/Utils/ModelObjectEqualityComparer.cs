namespace CimBios.Core.CimModel.CimDatatypeLib.Utils;

/// <summary>
///     Equality comparer class for only model objects OID comparision.
/// </summary>
public class ModelObjectOIDEqualityComparer : IEqualityComparer<IModelObject>
{
    public bool Equals(IModelObject? left, IModelObject? right)
    {
        if (left == null && right == null) return true;

        if (left != null && right != null) return left.OID.Equals(right.OID);

        return false;
    }

    public int GetHashCode(IModelObject obj)
    {
        return obj.OID.GetHashCode();
    }
}