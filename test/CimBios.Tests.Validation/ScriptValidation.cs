using CimBios.Core.CimModel.CimDatatypeLib.CIM17Types;
using CimBios.Core.CimModel.Validation;
using CimBios.Core.CimModel.Validation.Script;
using CimBios.Tests.Infrastructure;
using CimBios.Core.CimModel.Schema;
using CimBios.Core.CimModel.CimDatatypeLib.OID;

namespace CimBios.Tests.Validation;

public class ScriptValidation
{
    private static string ChekingVoltageLevelBaseVoltageNullCode =
        "dmFyIG1vID0gTW9kZWxPYmplY3QgYXMgVm9sdGFnZUxldmVsOw0KaWYgKG1vPy5CYXNlVm9" +
        "sdGFnZSA9PSBudWxsKQ0Kew0KcmV0dXJuIG5ldyBMaXN0PElWYWxpZGF0aW9uUmVzdWx0Pi" +
        "gpDQp7DQpuZXcgTW9kZWxPYmplY3RWYWxpZGF0aW9uUmVzdWx0KA0KVmFsaWRhdGlvblJlc" +
        "3VsdEtpbmQuRmFpbCwNCiJCYXNlVm9sdGFnZSBpcyBudWxsIiwNCk1vZGVsT2JqZWN0DQop" +
        "DQp9Ow0KfQ0KZWxzZQ0Kew0KcmV0dXJuIG5ldyBMaXN0PElWYWxpZGF0aW9uUmVzdWx0Pig" +
        "pDQp7DQpuZXcgUGFzc1ZhbGlkYXRpb25SZXN1bHQoDQoiQmFzZVZvbHRhZ2UgaXMgbm90IG" +
        "51bGwiDQopDQp9Ow0KfQ==";
    
    [Fact]
    public void BaseVoltageIsNull()
    {
        var voltageLevel = CreateVoltageLevelWithoutBaseVoltage();
        
        var validationResults = MakeRule()
            .Execute(voltageLevel).ToList();

        if (validationResults.FirstOrDefault() is null or ScriptExceptionValidationResult)
        {
            Assert.Fail("Validation Failed: Script didn't run!");
        }

        Assert.True(
            validationResults.First().ResultType == ValidationResultKind.Fail
        );
    }

    [Fact]
    public void BaseVoltageIsNotNull()
    {
        var voltageLevel = CreateVoltageLevelWithBaseVoltage();
        
        var validationResults = MakeRule()
            .Execute(voltageLevel).ToList();

        if (validationResults.FirstOrDefault() is null or ScriptExceptionValidationResult)
        {
            Assert.Fail("Validation Failed: Script didn't run!");
        }

        Assert.True(
            validationResults.First().ResultType == ValidationResultKind.Pass
        );
    }

    private VoltageLevel CreateVoltageLevelWithBaseVoltage()
    {
        var descriptorVoltageLevel = new TextDescriptorFactory().Create(
            "http://iec.ch/TC57/CIM100#VoltageLevel");

        var descriptorBaseVoltage = new TextDescriptorFactory().Create(
            "http://iec.ch/TC57/CIM100#BaseVoltage");

        var cimDocument = ModelLoader.CreateCimModelInstance();

        var voltageLevel = cimDocument.CreateObject<VoltageLevel>(
            descriptorVoltageLevel);

        var baseVoltage = cimDocument.Schema
            .TryGetResource<ICimMetaClass>(
                new("http://iec.ch/TC57/CIM100#BaseVoltage"));

        if (baseVoltage == null)
        {
            throw new NullReferenceException();
        }

        voltageLevel.BaseVoltage = new BaseVoltage(
            descriptorBaseVoltage, baseVoltage);
        voltageLevel.BaseVoltage.nominalVoltage = 110;

        return voltageLevel;
    }

    private VoltageLevel CreateVoltageLevelWithoutBaseVoltage()
    {
        var descriptorVoltageLevel = new TextDescriptorFactory().Create(
            "http://iec.ch/TC57/CIM100#VoltageLevel");

        var cimDocument = ModelLoader.CreateCimModelInstance();

        var voltageLevel = cimDocument.CreateObject<VoltageLevel>(
            descriptorVoltageLevel);

        return voltageLevel;
    }

    private IValidationRule MakeRule()
    {
        var rule = new ScriptValidationRule();
        
        var decode64 = System.Convert.FromBase64String(
            ChekingVoltageLevelBaseVoltageNullCode);
        rule.Code = System.Text.Encoding.UTF8.GetString(decode64);

        return rule;
    }
}
