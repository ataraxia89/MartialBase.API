// <copyright file="IMartialBaseUserHelper.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Models.DTOs.Organisations;

namespace MartialBase.API.AuthTools.Interfaces
{
    public interface IMartialBaseUserHelper
    {
        bool VerifyUserHasAnyAcceptedScope(HttpContext context, params string[] acceptedScopes);

        Task<Guid> GetUserIdForPersonAsync(Guid personId);

        /// <summary>
        /// Retrieves the requesting user's Person ID using the details contained in the auth bearer token.
        /// </summary>
        /// <param name="request">The HTTP request containing the auth token.</param>
        /// <returns>The user's Person ID.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when the bearer token in the HTTP request doesn't contain a valid user ID.</exception>
        Task<Guid?> GetPersonIdFromHttpRequestAsync(HttpRequest request);

        Task<List<string>> GetUserRolesForPersonAsync(Guid personId);

        /// <summary>
        /// Checks whether a user has member access to a School using the details contained in the auth bearer token.
        /// </summary>
        /// <param name="requestingUserPersonId">The Person ID of the requesting user.</param>
        /// <param name="schoolId">The ID of the School to check membership against.</param>
        /// <returns>
        /// <para>An ObjectResult containing the relevant status code and ErrorResponseCode (if necessary).</para>
        /// <para>
        /// Possible status codes:
        /// <list type="bullet">
        ///   <item>200 OK</item>
        ///   <item>403 Forbidden</item>
        /// </list>
        /// </para>
        /// </returns>
        /// <exception cref="EntityIdNotFoundException">Thrown when the provided School ID is not found.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the bearer token in the HTTP request doesn't contain a valid user ID.</exception>
        Task ValidateUserHasMemberAccessToSchoolAsync(
            Guid requestingUserPersonId,
            Guid schoolId);

        /// <summary>
        /// Checks whether a user has admin access to a School using the details contained in the auth bearer token.
        /// </summary>
        /// <param name="requestingUserPersonId">The Person ID of the requesting user.</param>
        /// <param name="schoolId">The ID of the School to check admin access against.</param>
        /// <returns>
        /// <para>An ObjectResult containing the relevant status code and ErrorResponseCode (if necessary).</para>
        /// <para>
        /// Possible status codes:
        /// <list type="bullet">
        ///   <item>200 OK</item>
        ///   <item>403 Forbidden</item>
        /// </list>
        /// </para>
        /// </returns>
        /// <exception cref="EntityIdNotFoundException">Thrown when the provided School ID is not found.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the bearer token in the HTTP request doesn't contain a valid user ID.</exception>
        Task ValidateUserHasAdminAccessToSchoolAsync(
            Guid requestingUserPersonId,
            Guid schoolId);

        /// <summary>
        /// Checks whether a user has member access to an Organisation using the details contained in the auth bearer token.
        /// </summary>
        /// <param name="requestingUserPersonId">The Person ID of the requesting user.</param>
        /// <param name="organisationId">The ID of the Organisation to check membership against.</param>
        /// <returns>
        /// <para>An ObjectResult containing the relevant status code and ErrorResponseCode (if necessary).</para>
        /// <para>
        /// Possible status codes:
        /// <list type="bullet">
        ///   <item>200 OK</item>
        ///   <item>403 Forbidden</item>
        /// </list>
        /// </para>
        /// </returns>
        /// <exception cref="EntityIdNotFoundException">Thrown when the provided Organisation ID is not found.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the bearer token in the HTTP request doesn't contain a valid user ID.</exception>
        Task ValidateUserHasMemberAccessToOrganisationAsync(
            Guid requestingUserPersonId,
            Guid organisationId);

        /// <summary>
        /// Checks whether a user has admin access to an Organisation using the details contained in the auth bearer token.
        /// </summary>
        /// <param name="requestingUserPersonId">The Person ID of the requesting user.</param>
        /// <param name="organisationId">The ID of the Organisation to check admin access against.</param>
        /// <returns>
        /// <para>An ObjectResult containing the relevant status code and ErrorResponseCode (if necessary).</para>
        /// <para>
        /// Possible status codes:
        /// <list type="bullet">
        ///   <item>200 OK</item>
        ///   <item>403 Forbidden</item>
        /// </list>
        /// </para>
        /// </returns>
        /// <exception cref="EntityIdNotFoundException">Thrown when the provided Organisation ID is not found.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the bearer token in the HTTP request doesn't contain a valid user ID.</exception>
        Task ValidateUserHasAdminAccessToOrganisationAsync(
            Guid requestingUserPersonId,
            Guid organisationId);

        /// <summary>
        /// Checks whether the requesting user has access to a specified Person, for example by being an Organisation admin.
        /// </summary>
        /// <param name="requestingUserPersonId">The Person ID of the requesting user.</param>
        /// <param name="personId">The ID of the Person to check access against.</param>
        /// <returns>
        /// <para>An ObjectResult containing the relevant status code and ErrorResponseCode (if necessary).</para>
        /// <para>
        /// Possible status codes:
        /// <list type="bullet">
        ///   <item>200 OK</item>
        ///   <item>403 Forbidden</item>
        /// </list>
        /// </para>
        /// </returns>
        /// <exception cref="EntityIdNotFoundException">Thrown when the provided Person ID is not found.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the bearer token in the HTTP request doesn't contain a valid user ID.</exception>
        Task ValidateUserHasAccessToPersonAsync(
            Guid requestingUserPersonId,
            Guid personId);

        Task<List<OrganisationDTO>> FilterOrganisationsByMemberAccess(
            IEnumerable<OrganisationDTO> organisations,
            Guid requestingUserPersonId);
    }
}
