// <copyright file="XmlDocumentFilter.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MartialBase.API.Utilities
{
    public class XmlDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (var path in swaggerDoc.Paths)
            {
                foreach (var operation in path.Value.Operations)
                {
                    operation.Value.Description = Utilities.RemoveXMLTags(operation.Value.Description);
                    operation.Value.Description = Utilities.SimplifyObjectReferences(operation.Value.Description);

                    operation.Value.Summary = Utilities.RemoveXMLTags(operation.Value.Summary);
                    operation.Value.Summary = Utilities.SimplifyObjectReferences(operation.Value.Summary);

                    if (operation.Value.RequestBody != null)
                    {
                        operation.Value.RequestBody.Description = Utilities.RemoveXMLTags(operation.Value.RequestBody.Description);
                        operation.Value.RequestBody.Description = Utilities.SimplifyObjectReferences(operation.Value.RequestBody.Description);
                    }

                    foreach (var parameter in operation.Value.Parameters)
                    {
                        parameter.Description = Utilities.RemoveXMLTags(parameter.Description);
                        parameter.Description = Utilities.SimplifyObjectReferences(parameter.Description);
                    }
                }
            }
        }
    }
}
