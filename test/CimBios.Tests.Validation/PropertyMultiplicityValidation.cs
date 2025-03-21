using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.Validation;
using CimBios.Tests.Infrastructure;

namespace CimBios.Tests.Validation;

public class PropertyMultiplicityValidation
{
    [Fact]
    public void StrictlyOne()
    {
        var cimDocument = ModelLoader.CreateCimModelInstance();
        var rule = GetMultiplicityValidationRule();

        var testCNode = cimDocument.CreateObject<ConnectivityNode>(cimDocument
            .OIDDescriptorFactory.Create("_TestCN"));    

        var result1 = rule.Execute(testCNode);
        Assert.NotNull(result1
            .FirstOrDefault(r => r.ResultType == ValidationResultKind.Fail));

        var testBay = cimDocument.CreateObject<Bay>(cimDocument
            .OIDDescriptorFactory.Create("_TestBay"));   
 
        testBay.AddToConnectivityNodes(testCNode);

        var result2 = rule.Execute(testCNode);
        Assert.NotNull(result2
            .FirstOrDefault(r => r.ResultType == ValidationResultKind.Pass));   
    }

    [Fact]
    public void OneN()
    {
        var cimDocument = ModelLoader.CreateCimModelInstance();
        var rule = GetMultiplicityValidationRule();

        var IrregularIntervalScheduleMetaClass = cimDocument.Schema
            .TryGetResource<ICimMetaClass>(
                new ("http://iec.ch/TC57/CIM100#IrregularIntervalSchedule"));

        var IrregularTimePointMetaClass = cimDocument.Schema
            .TryGetResource<ICimMetaClass>(
                new ("http://iec.ch/TC57/CIM100#IrregularTimePoint"));

        if (IrregularIntervalScheduleMetaClass == null 
            || IrregularTimePointMetaClass == null)
        {
            Assert.Fail();
        }

        var testSchedule = cimDocument.CreateObject(cimDocument
            .OIDDescriptorFactory.Create("_TestSchedule"), 
            IrregularIntervalScheduleMetaClass);    

        var result1 = rule.Execute(testSchedule);
        Assert.NotNull(result1
            .FirstOrDefault(r => r.ResultType == ValidationResultKind.Fail));

        var testPoint = cimDocument.CreateObject(cimDocument
            .OIDDescriptorFactory.Create("_TestPoint"), 
            IrregularTimePointMetaClass);   
 
        testSchedule.AddAssoc1ToM("TimePoints", testPoint);

        var result2 = rule.Execute(testSchedule);
        Assert.NotNull(result2
            .FirstOrDefault(r => r.ResultType == ValidationResultKind.Pass));   
    }

    private static IValidationRule GetMultiplicityValidationRule()
    {
        var manager = new ValidationManager();

        var rules = manager.GetValidationRules;

        var multiplicityRule = rules.Where(
            r => r.GetType() ==
            typeof(PropertyMultiplicityValidationRule)
        ).Single();

        return multiplicityRule;
    }
}