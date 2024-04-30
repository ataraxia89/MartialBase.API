// <copyright file="ModelStateTools.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Tools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc.ModelBinding;

using Newtonsoft.Json;

namespace MartialBase.API.Tools
{
    /// <summary>
    /// Common tools used to process structured ModelState objects.
    /// </summary>
    public static class ModelStateTools
    {
        /// <summary>
        /// Takes a <see cref="ModelStateDictionary"/> object and extracts the errors into a <see cref="Dictionary{TKey,TValue}"/>.
        /// </summary>
        /// <param name="modelState">The <see cref="ModelStateDictionary"/> to be referenced.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> of <see cref="string"/>-based keys and array values.</returns>
        public static Dictionary<string, string[]> PrepareModelStateErrors(ModelStateDictionary modelState)
        {
            var modelStateErrors = new Dictionary<string, string[]>();

            foreach (KeyValuePair<string, ModelStateEntry> error in modelState.Where(ms =>
                         ms.Value.ValidationState == ModelValidationState.Invalid))
            {
                modelStateErrors.Add(error.Key, error.Value.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            return modelStateErrors;
        }

        /// <summary>
        /// Takes a serialized <see cref="string"/> of ModelState errors and returns a structured <see cref="Dictionary{TKey,TValue}"/> of the errors.
        /// </summary>
        /// <param name="serializedModelStateErrors">The serialized ModelState errors to be processed.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> of <see cref="string"/>-based keys and array values.</returns>
        public static Dictionary<string, string[]> ParseModelStateErrors(string serializedModelStateErrors) =>
            JsonConvert.DeserializeObject<Dictionary<string, string[]>>(serializedModelStateErrors);
    }
}
