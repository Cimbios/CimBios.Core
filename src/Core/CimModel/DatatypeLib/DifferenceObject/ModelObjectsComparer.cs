using CimBios.Core.CimModel.Schema;

namespace CimBios.Core.CimModel.CimDatatypeLib;

public static class ModelObjectsComparer
{
    public static UpdatingDifferenceObject Compare(IReadOnlyModelObject leftObject,
        IReadOnlyModelObject rightObject, bool strictlyClassAlign = false)
    {
        if (leftObject.MetaClass.Equals(rightObject.MetaClass) == false
            && strictlyClassAlign)
            throw new ArgumentException("Different meta classes!");

        var diff = new UpdatingDifferenceObject(leftObject.OID);

        var intersectedProps = leftObject.MetaClass.AllProperties
            .Intersect(rightObject.MetaClass.AllProperties)
            .ToList();

        foreach (var metaProperty in intersectedProps)
            if (metaProperty.PropertyKind == CimMetaPropertyKind.Attribute)
            {
                var leftVal = leftObject.GetAttribute(metaProperty);
                var rightVal = rightObject.GetAttribute(metaProperty);

                if (metaProperty.PropertyDatatype?.IsCompound == true)
                {
                    var leftCompound = leftVal as IReadOnlyModelObject
                                       ?? new WeakModelObject(leftObject.OID,
                                           metaProperty.PropertyDatatype,
                                           true);

                    var rightCompound = rightVal as IReadOnlyModelObject
                                        ?? new WeakModelObject(rightObject.OID,
                                            metaProperty.PropertyDatatype,
                                            true);

                    var compoundDiff = Compare(leftCompound, rightCompound, true);
                    if (compoundDiff.ModifiedProperties.Count != 0)
                        diff.ChangeAttribute(metaProperty,
                            leftCompound, rightCompound);
                }
                else if (metaProperty.PropertyDatatype?.IsEnum == true)
                {
                    var leftEnumValue = leftObject.GetAttribute(metaProperty)
                        as EnumValueObject;
                    var rightEnumValue = rightObject.GetAttribute(metaProperty)
                        as EnumValueObject;

                    if (leftEnumValue != rightEnumValue)
                        diff.ChangeAttribute(metaProperty,
                            leftEnumValue, rightEnumValue);
                }
                else
                {
                    if (!(leftVal?.Equals(rightVal) ?? rightVal != null)
                        || !(rightVal?.Equals(leftVal) ?? leftVal != null))
                        diff.ChangeAttribute(metaProperty, leftVal, rightVal);
                }
            }
            else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
            {
                var leftRef = leftObject.GetAssoc1To1(metaProperty);
                var rightRef = rightObject.GetAssoc1To1(metaProperty);
                var leftRefOID = leftRef?.OID;
                var rightRefOID = rightRef?.OID;

                if (!(leftRefOID != null && leftRefOID.Equals(rightRefOID))
                    || !(rightRefOID != null && rightRefOID.Equals(leftRefOID)))
                    diff.ChangeAssoc1(metaProperty, leftRef, rightRef);
            }
            else if (metaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
            {
                var leftRefs = leftObject.GetAssoc1ToM(metaProperty)
                    .ToDictionary(k => k.OID, v => v);
                var rightRefs = rightObject.GetAssoc1ToM(metaProperty)
                    .ToDictionary(k => k.OID, v => v);

                var lExceptRefs = leftRefs.Keys.Except(rightRefs.Keys);
                var rExceptRefs = rightRefs.Keys.Except(leftRefs.Keys);

                foreach (var lexcept in lExceptRefs) diff.RemoveFromAssocM(metaProperty, leftRefs[lexcept]);

                foreach (var rexcept in rExceptRefs) diff.AddToAssocM(metaProperty, rightRefs[rexcept]);
            }

        return diff;
    }
}