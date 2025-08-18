using System;
using System.Collections.Generic;
using CimBios.Core.CimModel.CimDatatypeLib;
using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using CimBios.Core.CimModel.Schema;

namespace CimBios.Tools.ModelDebug.Models;

public class CimObjectPropertyModel : TreeViewNodeModel
{
    public IModelObject ModelObject { get; }
    public ICimMetaProperty MetaProperty { get; }
    public string Value => GetStringValue();
    
    public CimObjectPropertyModel(IModelObject modelObject,
        ICimMetaProperty metaProperty, bool recursive=false)
    {
        ModelObject = modelObject;
        MetaProperty = metaProperty;

        SubscribeOnChanges();
        
        if (recursive) InitChildPropertyNodes();
        
        OnPropertyChanged(nameof(SubNodes));
    }

    private void SubscribeOnChanges()
    {
        ModelObject.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(ModelObject));
            OnPropertyChanged(nameof(Value));
        };
    }
    
    private void InitChildPropertyNodes()
    {
        if (MetaProperty.PropertyKind == CimMetaPropertyKind.Attribute)
        {
            var value = ModelObject.GetAttribute(MetaProperty);
            if (value is not IModelObject compound) return;
            
            foreach (var property in compound.MetaClass.AllProperties)
            {
                AddChild(new CimObjectPropertyModel(compound, property, true));
            }
        }
        else if (MetaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
        {
            var refsM = ModelObject.GetAssoc1ToM(MetaProperty);

            foreach (var refM in refsM)
            {
                AddChild(new CimAssocPropertyModel(refM, ModelObject, MetaProperty));
            }
        }
        else if (MetaProperty.PropertyKind == CimMetaPropertyKind.Statements
                 && ModelObject is IStatementsContainer statementsContainer)
        {
            foreach (var statementObject in statementsContainer.Statements[MetaProperty])
            {
                var statementNode = new CimStatementPropertyModel(
                    statementObject, ModelObject, MetaProperty);
                AddChild(statementNode);
                
                foreach (var property in statementObject.MetaClass.AllProperties)
                {
                    statementNode.AddChild(new CimObjectPropertyModel(statementObject, 
                        property, false));
                }
            }
        }
    }
    
    protected virtual string GetStringValue()
    {
        string stringValue;
        
        if (MetaProperty.PropertyKind == CimMetaPropertyKind.Attribute)
        {
            var value = ModelObject.GetAttribute(MetaProperty);
            if (value is IModelObject compound)
            {
                stringValue = compound.MetaClass.ShortName;
            }
            else
            {
                stringValue = value?.ToString() ?? "null";
            }
        }
        else if (MetaProperty.PropertyKind == CimMetaPropertyKind.Assoc1To1)
        {
            var ref1 = ModelObject.GetAssoc1To1(MetaProperty);
            if (ref1 is IdentifiedObject io)
                stringValue = $"{ref1.OID} ({io.name})";
            else
                stringValue = ref1?.OID.ToString() ?? "null";
        }
        else if (MetaProperty.PropertyKind == CimMetaPropertyKind.Assoc1ToM)
        {
            var refsM = ModelObject.GetAssoc1ToM(MetaProperty);
            stringValue = $"Count: {refsM.Length}";
        }
        else if (MetaProperty.PropertyKind == CimMetaPropertyKind.Statements
                 && ModelObject is IStatementsContainer statementsContainer)
        {
            var statements = statementsContainer.Statements;
            stringValue = $"Statements: {statements[MetaProperty].Count}";
        }
        else
        {
            stringValue = "NOT SUPPORTED";
        }

        return stringValue;
    }
}

public class CimAssocPropertyModel(IModelObject assocObject,
    IModelObject modelObject, ICimMetaProperty metaProperty) 
    : CimObjectPropertyModel(modelObject, metaProperty)
{
    public IModelObject AssocObject { get; } = assocObject;

    protected override string GetStringValue()
    {
        var value = AssocObject.OID.ToString();
        
        if (AssocObject is IdentifiedObject io)
            return $"{value} ({io.name})";
        
        return value;
    }
}

public class CimStatementPropertyModel(IModelObject statementObject,
    IModelObject modelObject, ICimMetaProperty metaProperty) 
    : CimObjectPropertyModel(modelObject, metaProperty)
{
    public IModelObject StatementObject { get; } = statementObject;
    
    protected override string GetStringValue() 
        => $"{StatementObject.MetaClass.ShortName} : {StatementObject.OID}";
}