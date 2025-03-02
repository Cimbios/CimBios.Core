namespace CimBios.Core.CimModel.CimDatatypeLib;

public static class ModelObjectsComparer
{
    public static UpdatingDifferenceObject Compare (IModelObject leftObject, 
        IModelObject rightObject, bool strictlyClassAlign = false)
    {
        if (leftObject.MetaClass != rightObject.MetaClass
            && strictlyClassAlign == true)
        {
            throw new ArgumentException("Different meta classes!");
        }

        var diff = new UpdatingDifferenceObject(leftObject.OID);

        var intersectedProps = leftObject.MetaClass.AllProperties
            .Intersect(rightObject.MetaClass.AllProperties)
            .ToList();

        foreach (var metaProperty in intersectedProps)
        {
            if (metaProperty.PropertyKind == Schema.CimMetaPropertyKind.Attribute)
            {

            }
            else if (metaProperty.PropertyKind == Schema.CimMetaPropertyKind.Assoc1To1)
            {
                var leftRef = leftObject.GetAssoc1To1(metaProperty);
                var rightRef = rightObject.GetAssoc1To1(metaProperty);

                if (/*leftRef?.MetaClass != rightRef?.MetaClass
                    ||*/ leftRef?.OID != rightRef?.OID)
                {
                    diff.ChangeAssoc1(metaProperty, leftRef, rightRef);
                }
            }
        }

        return diff;
    }
}
