/*
*    CimBios.Core - Common Information Model (IEC61970) I/O Library
*    Copyright (C) 2025 Yuri A. Kovalenko a.k.a belizahrt <belizahrt@gmail.com>
*
*    This program is free software: you can redistribute it and/or modify
*    it under the terms of the GNU General Public License as published by
*    the Free Software Foundation, either version 3 of the License, or
*    (at your option) any later version.
*
*    This program is distributed in the hope that it will be useful,
*    but WITHOUT ANY WARRANTY; without even the implied warranty of
*    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*    GNU General Public License for more details.
*
*    You should have received a copy of the GNU General Public License
*    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections.Concurrent;
using CimBios.Core.CimModel.DataModel;
using CimBios.Core.CimModel.Validation.InternalRules;

namespace CimBios.Core.CimModel.Validation;

///
public static class ValidationCimDataModelExtensions
{
    ///
    public static IEnumerable<IValidationResult> Validate(
        this ICimDataModel dataModel, IEnumerable<IValidationRule> rulesSet, 
        bool executeInternalRules = true)
    {
        var results = new BlockingCollection<IValidationResult>();

        var executeRules = new List<IValidationRule>();
        if (executeInternalRules)
        {
            executeRules.AddRange(InternalValidationRulesBuilder.GetRules());
        }

        executeRules.AddRange(rulesSet);

        dataModel.GetAllObjects().AsParallel().ForAll(modelObject =>
        {
            foreach (var rule in executeRules)
            {
                if (rule.NeedExecute(modelObject) == false)
                {
                    continue;
                }

                var executionResults = rule.Execute(modelObject);
                foreach (var result in executionResults
                    .Where(r => r.ResultType != ValidationResultKind.Pass))
                {
                    results.Add(result);
                }
            }
        });

        return results;
    }

    ///
    public static async Task<IEnumerable<IValidationResult>> ValidateAsync(
        this ICimDataModel dataModel, IEnumerable<IValidationRule> rulesSet, 
        bool executeInternalRules = true)
    {
        var result = await Task.Run(
            () => dataModel.Validate(rulesSet, executeInternalRules));

        return result;
    }
}

///
internal static class InternalValidationRulesBuilder
{
    internal static ICollection<IValidationRule> GetRules()
    {
        return
        [
            new PropertyMultiplicityValidationRule(),
            new HasInverseReferenceValidationRule(),
            //
        ];
    }
}
