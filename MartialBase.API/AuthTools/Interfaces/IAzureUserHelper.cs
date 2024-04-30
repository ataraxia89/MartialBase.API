// <copyright file="IAzureUserHelper.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

namespace MartialBase.API.AuthTools.Interfaces
{
    public interface IAzureUserHelper
    {
        Task<bool> AzureUserHasRequiredRoleAsync(Guid azureUserId, string allowedRole);

        Task<Guid?> GetPersonIdForAzureUserAsync(Guid azureUserId, string invitationCode);

        /// <summary>
        /// Retrieves the user's Azure ID and invitation code from the JWT bearer token in HTTP headers.
        /// </summary>
        /// <param name="request">The HTTP request containing the auth token.</param>
        /// <returns>A Tuple object containing the user's Azure ID and invitation code.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when the bearer token in the HTTP request doesn't contain a valid user ID.</exception>
        (Guid AzureId, string InvitationCode) GetAzureIdAndInvitationCodeFromHttpRequest(HttpRequest request);

        Task UpdateAzureUserRoles(
            Guid martialBaseUserId);
    }
}
