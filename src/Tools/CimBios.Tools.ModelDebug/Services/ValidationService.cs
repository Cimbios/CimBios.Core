using System;
using System.Collections.Generic;
using CimBios.Core.CimModel.Validation;
using CimBios.Tools.ModelDebug.ViewModels;

namespace CimBios.Tools.ModelDebug.Services;

public class ValidationService
{
    public ValidationService()
    {
        MakeRulesSet();
    }

    public IEnumerable<IValidationResult> ValidateDataModel()
    {
        if (GlobalServices.LoaderService.DataContext == null)
            throw new NullReferenceException("DataContext is null");

        return GlobalServices.LoaderService
            .DataContext.Validate([]);
    }
    
    private void MakeRulesSet()
    {
        
    }
}
