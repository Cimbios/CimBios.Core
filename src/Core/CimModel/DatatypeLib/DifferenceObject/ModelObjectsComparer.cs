using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDatatypeLib;

public static class ModelObjectsComparer
{
    public static UpdatingDifferenceObject Compare (IReadOnlyModelObject leftObject, 
        IReadOnlyModelObject rightObject, bool strictlyClassAlign = false)
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
            if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute)
            {
                var leftVal = leftObject.GetAttribute(metaProperty);
                var rightVal = rightObject.GetAttribute(metaProperty);   

                if (metaProperty.PropertyDatatype?.IsCompound == true)
                {
                    var leftCompound = leftVal as IReadOnlyModelObject
                        ?? new WeakModelObject(leftObject.OID, 
                            leftObject.MetaClass);

                    var rightCompound = rightVal as IReadOnlyModelObject
                        ?? new WeakModelObject(rightObject.OID, 
                            rightObject.MetaClass);

                    var compoundDiff = Compare(leftCompound, rightCompound);
                    if (compoundDiff.ModifiedProperties.Count != 0)
                    {
                        diff.ChangeAttribute(metaProperty, 
                            leftCompound, rightCompound);
                    }
                }            
                else if (metaProperty.PropertyDatatype?.IsEnum == true)
                {
                    var leftEnumValue = leftObject.GetAttribute(metaProperty) 
                        as EnumValueObject;
                    var rightEnumValue = rightObject.GetAttribute(metaProperty) 
                        as EnumValueObject;

                    if (leftEnumValue != rightEnumValue)
                    {
                        diff.ChangeAttribute(metaProperty, 
                            leftEnumValue, rightEnumValue);
                    }
                }
                else
                {
                    if (!(leftVal?.Equals(rightVal) ?? rightVal != null))
                    {
                        diff.ChangeAttribute(metaProperty, leftVal, rightVal);
                    }
                }
            }
            else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
            {
                var leftRef = leftObject.GetAssoc1To1(metaProperty);
                var rightRef = rightObject.GetAssoc1To1(metaProperty);

                if (/*leftRef?.MetaClass != rightRef?.MetaClass
                    ||*/ leftRef?.OID != rightRef?.OID)
                {
                    diff.ChangeAssoc1(metaProperty, leftRef, rightRef);
                }
            }
            else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
            {

            }
        }

        return diff;
    }
}
