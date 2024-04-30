// <copyright file="AzureUserHelper.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.IdentityModel.Tokens.Jwt;

using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Data.Caching.Interfaces;
using MartialBase.API.Data.Repositories.Interfaces;

using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Net.Http.Headers;

using Constants = Microsoft.Identity.Web.Constants;

namespace MartialBase.API.AuthTools
{
    public class AzureUserHelper : IAzureUserHelper
    {
        private readonly IMartialBaseUsersRepository _martialBaseUsersRepository;
        private readonly IMartialBaseUserRolesRepository _martialBaseUserRolesRepository;
        private readonly IConfiguration _configuration;
        private readonly IScopedCache _scopedCache;

        public AzureUserHelper(
            IMartialBaseUsersRepository martialBaseUsersRepository,
            IMartialBaseUserRolesRepository martialBaseUserRolesRepository,
            IConfiguration configuration,
            IScopedCache scopedCache)
        {
            _martialBaseUsersRepository = martialBaseUsersRepository;
            _martialBaseUserRolesRepository = martialBaseUserRolesRepository;
            _configuration = configuration;
            _scopedCache = scopedCache;
        }

        public async Task<bool> AzureUserHasRequiredRoleAsync(Guid azureUserId, string allowedRole) =>
            await _martialBaseUserRolesRepository.AzureUserHasRequiredRoleAsync(azureUserId, allowedRole);

        public async Task<Guid?> GetPersonIdForAzureUserAsync(Guid azureUserId, string invitationCode) =>
            await _scopedCache.GetOrSetPersonIdForAzureUserAsync(
                azureUserId,
                async () => await _martialBaseUsersRepository.GetPersonIdForAzureUserAsync(
                    azureUserId,
                    invitationCode));

        /// <inheritdoc />
        public (Guid AzureId, string InvitationCode) GetAzureIdAndInvitationCodeFromHttpRequest(HttpRequest request)
        {
            string accessToken = request.Headers[HeaderNames.Authorization].ToString().Substring(7);
            var token = new JwtSecurityToken(accessToken);
            string userIdString = token.Claims.FirstOrDefault(c => c.Type == ClaimConstants.Oid)?.Value;
            string invitationCode = token.Claims.FirstOrDefault(c => c.Type == "extension_InvitationCode")?.Value;

            if (userIdString == null)
            {
                throw new UnauthorizedAccessException("Auth token does not contain a valid user ID.");
            }

            return (new Guid(userIdString), invitationCode);
        }

        /// <inheritdoc />
        public async Task UpdateAzureUserRoles(
            Guid martialBaseUserId)
        {
            string azureId = _martialBaseUsersRepository.GetAzureIdForUserAsync(martialBaseUserId).ToString();

            if (string.IsNullOrEmpty(azureId))
            {
                return;
            }

            var userRoles = (await _martialBaseUserRolesRepository
                    .GetRolesForUserAsync(martialBaseUserId))
                .Select(ur => ur.Name + "@roles.martialbase.net")
                .ToList();

            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(_configuration[$"{Constants.AzureAdB2C}:ClientId"])
                .WithTenantId(_configuration[$"{Constants.AzureAdB2C}:Domain"]) // TODO: Can TenantId be added to the configuration instead?
                .WithClientSecret(_configuration[$"{Constants.AzureAdB2C}:ClientSecret"])
                .Build();

            IAuthenticationProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);

            var graphClient = new GraphServiceClient(authProvider);

            User azureUser = await graphClient.Users[azureId]
                .Request()
                .Select(e => new
                {
                    e.OtherMails
                })
                .GetAsync();

            azureUser.OtherMails = userRoles;

            await graphClient.Users[azureId]
                .Request()
                .UpdateAsync(azureUser);
        }
    }
}
