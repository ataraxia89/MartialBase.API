// <copyright file="ExceptionHandlingMiddleware.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Net;

using MartialBase.API.Data.Exceptions;
using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Models.Enums;

namespace MartialBase.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IWebHostEnvironment environment)
        {
            try
            {
                await _next(context);
            }
            catch (ErrorResponseCodeException ex)
            {
                var responseText = environment.EnvironmentName == "Development"
                    ? $"{(int)ex.ErrorResponseCode}: {ex.ErrorResponseCode}"
                    : ((int)ex.ErrorResponseCode).ToString();

                context.Response.ContentType = "application/text";
                context.Response.StatusCode = (int)ex.StatusCode;

                await context.Response.WriteAsync(responseText);
            }
            catch (EntityNotFoundException ex)
            {
                context.Response.ContentType = "application/text";
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                await context.Response.WriteAsync(ex.Message);
            }
            catch (OrphanPersonEntityException ex)
            {
                var responseText = environment.EnvironmentName == "Development"
                    ? $"{(int)ErrorResponseCode.OrphanPersonEntity}: {ErrorResponseCode.OrphanPersonEntity} ({ex.Message})"
                    : ((int)ErrorResponseCode.OrphanPersonEntity).ToString();

                context.Response.ContentType = "application/text";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                await context.Response.WriteAsync(responseText);
            }
            catch (OrphanSchoolEntityException ex)
            {
                var responseText = environment.EnvironmentName == "Development"
                    ? $"{(int)ErrorResponseCode.OrphanSchoolEntity}: {ErrorResponseCode.OrphanSchoolEntity} ({ex.Message})"
                    : ((int)ErrorResponseCode.OrphanSchoolEntity).ToString();

                context.Response.ContentType = "application/text";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                await context.Response.WriteAsync(responseText);
            }
            catch (DbContextSaveChangesException ex)
            {
                context.Response.ContentType = "application/text";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                await context.Response.WriteAsync(ex.Message);
            }
            catch (Exception ex)
            {
                // TODO: 1273 - Get code coverage on this section
                var responseText = environment.EnvironmentName == "Development"
                    ? ex.Message
                    : string.Empty;

                context.Response.ContentType = "application/text";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                await context.Response.WriteAsync(responseText);
            }
        }
    }
}
