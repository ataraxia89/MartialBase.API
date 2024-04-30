// <copyright file="PersonIdRequiredAsyncFilter.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Net;

using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Data.Exceptions;
using MartialBase.API.Models.Enums;

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace MartialBase.API.AuthTools.Attributes
{
    public class PersonIdRequiredAsyncFilter : IAsyncAuthorizationFilter
    {
        /// <inheritdoc />
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var martialBaseUserHelper = context.HttpContext.RequestServices.GetService<IMartialBaseUserHelper>();

            Guid? requestingPersonId =
                await martialBaseUserHelper.GetPersonIdFromHttpRequestAsync(context.HttpContext.Request);

            if (requestingPersonId == null)
            {
                throw new ErrorResponseCodeException(
                    ErrorResponseCode.AzureUserNotRegistered,
                    HttpStatusCode.Forbidden);
            }

            List<string> userRoles = await martialBaseUserHelper.GetUserRolesForPersonAsync((Guid)requestingPersonId);

            context.HttpContext.Request.Headers["RequestingPersonId"] = new StringValues(requestingPersonId.ToString());
            context.HttpContext.Request.Headers["RequestingUserRoles"] = new StringValues(userRoles.ToArray());
        }
    }
}
