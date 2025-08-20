using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using CimBios.Core.CimModel.CimDataModel;
using CimBios.Core.CimModel.Validation;
using CimBios.Core.CimModel.Validation.UserCustomRules;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Services;

public class ValidationService
{
    public ValidationService()
    {

    }

    public IEnumerable<IValidationResult> ValidateDataModel(
        bool includeInternalRules = true,
        bool includeCustomRules = true)
    {
        var model = GlobalServices.LoaderService.DataContext
            ?? throw new NullReferenceException("DataContext is null");

        var rules = new List<IValidationRule>();
        if (includeCustomRules)
        {
            rules.AddRange(CustomValidationRulesBuilder.GetRules());
        }

        return model.Validate(rules, includeInternalRules);
    }
}
