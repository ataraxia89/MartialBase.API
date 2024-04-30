// <copyright file="MartialBaseUserHelper.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Net;

using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Data.Caching.Interfaces;
using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Exceptions;
using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Models.Enums;

using Microsoft.Identity.Web.Resource;

namespace MartialBase.API.AuthTools
{
    public class MartialBaseUserHelper : IMartialBaseUserHelper
    {
        private readonly IMartialBaseUsersRepository _martialBaseUsersRepository;
        private readonly IMartialBaseUserRolesRepository _martialBaseUserRolesRepository;
        private readonly IOrganisationsRepository _organisationsRepository;
        private readonly IPeopleRepository _peopleRepository;
        private readonly ISchoolsRepository _schoolsRepository;
        private readonly IScopedCache _scopedCache;
        private readonly IAzureUserHelper _azureUserHelper;

        public MartialBaseUserHelper(
            IMartialBaseUsersRepository martialBaseUsersRepository,
            IMartialBaseUserRolesRepository martialBaseUserRolesRepository,
            IOrganisationsRepository organisationsRepository,
            IPeopleRepository peopleRepository,
            ISchoolsRepository schoolsRepository,
            IScopedCache scopedCache,
            IAzureUserHelper azureUserHelper)
        {
            _martialBaseUsersRepository = martialBaseUsersRepository;
            _martialBaseUserRolesRepository = martialBaseUserRolesRepository;
            _organisationsRepository = organisationsRepository;
            _peopleRepository = peopleRepository;
            _schoolsRepository = schoolsRepository;
            _scopedCache = scopedCache;
            _azureUserHelper = azureUserHelper;
        }

        public bool VerifyUserHasAnyAcceptedScope(HttpContext context, params string[] acceptedScopes)
        {
            context.VerifyUserHasAnyAcceptedScope(acceptedScopes);

            return true;
        }

        public async Task<Guid> GetUserIdForPersonAsync(Guid personId) =>
            await _scopedCache.GetOrSetUserIdAsync(
                personId,
                async () => await _martialBaseUsersRepository.GetUserIdForPersonAsync(personId));

        /// <inheritdoc />
        public async Task<Guid?> GetPersonIdFromHttpRequestAsync(HttpRequest request)
        {
            (Guid azureId, string invitationCode) = _azureUserHelper.GetAzureIdAndInvitationCodeFromHttpRequest(request);

            return await _azureUserHelper.GetPersonIdForAzureUserAsync(azureId, invitationCode);
        }

        public async Task<List<string>> GetUserRolesForPersonAsync(Guid personId)
        {
            Guid userId = await GetUserIdForPersonAsync(personId);

            return (await _scopedCache.GetOrSetUserRolesAsync(
                    personId,
                    async () =>
                    {
                        return (await _martialBaseUserRolesRepository
                                .GetRolesForUserAsync(userId))
                            .Select(mur => mur.Name)
                            .ToList();
                    }))
                .ToList();
        }

        /// <inheritdoc />
        public async Task ValidateUserHasMemberAccessToSchoolAsync(
            Guid requestingUserPersonId,
            Guid schoolId)
        {
            if (!await _schoolsRepository.ExistsAsync(schoolId))
            {
                throw new EntityIdNotFoundException("School", schoolId);
            }

            var userRoles = await GetUserRolesForPersonAsync(requestingUserPersonId);

            if (userRoles.Contains(UserRoles.Thanos))
            {
                return;
            }

            if (!userRoles.Contains(UserRoles.SchoolMember) &&
                !userRoles.Contains(UserRoles.SchoolInstructor) &&
                !userRoles.Contains(UserRoles.SchoolHeadInstructor) &&
                !userRoles.Contains(UserRoles.SchoolSecretary))
            {
                throw new ErrorResponseCodeException(ErrorResponseCode.InsufficientUserRole, HttpStatusCode.Forbidden);
            }

            if (!await _schoolsRepository.SchoolHasStudentAsync(schoolId, requestingUserPersonId))
            {
                throw new ErrorResponseCodeException(ErrorResponseCode.NotSchoolStudent, HttpStatusCode.Forbidden);
            }
        }

        /// <inheritdoc />
        public async Task ValidateUserHasAdminAccessToSchoolAsync(
            Guid requestingUserPersonId,
            Guid schoolId)
        {
            if (!await _schoolsRepository.ExistsAsync(schoolId))
            {
                throw new EntityIdNotFoundException("School", schoolId);
            }

            var userRoles = await GetUserRolesForPersonAsync(requestingUserPersonId);

            if (userRoles.Contains(UserRoles.Thanos))
            {
                return;
            }

            if (!userRoles.Contains(UserRoles.SchoolSecretary))
            {
                throw new ErrorResponseCodeException(ErrorResponseCode.InsufficientUserRole, HttpStatusCode.Forbidden);
            }

            if (!await _schoolsRepository.SchoolHasSecretaryAsync(schoolId, requestingUserPersonId))
            {
                throw new ErrorResponseCodeException(ErrorResponseCode.NotSchoolSecretary, HttpStatusCode.Forbidden);
            }
        }

        /// <inheritdoc />
        public async Task ValidateUserHasMemberAccessToOrganisationAsync(
            Guid requestingUserPersonId,
            Guid organisationId)
        {
            if (!await _organisationsRepository.ExistsAsync(organisationId))
            {
                throw new EntityIdNotFoundException("Organisation", organisationId);
            }

            var userRoles = await GetUserRolesForPersonAsync(requestingUserPersonId);

            if (userRoles.Contains(UserRoles.Thanos))
            {
                return;
            }

            if (!userRoles.Contains(UserRoles.OrganisationMember) &&
                !userRoles.Contains(UserRoles.OrganisationAdmin))
            {
                throw new ErrorResponseCodeException(ErrorResponseCode.InsufficientUserRole, HttpStatusCode.Forbidden);
            }

            if (!await _organisationsRepository.PersonCanAccessOrganisationAsync(
                organisationId, requestingUserPersonId))
            {
                throw new ErrorResponseCodeException(ErrorResponseCode.NoOrganisationAccess, HttpStatusCode.Forbidden);
            }
        }

        /// <inheritdoc />
        public async Task ValidateUserHasAdminAccessToOrganisationAsync(
            Guid requestingUserPersonId,
            Guid organisationId)
        {
            if (!await _organisationsRepository.ExistsAsync(organisationId))
            {
                throw new EntityIdNotFoundException("Organisation", organisationId);
            }

            var userRoles = await GetUserRolesForPersonAsync(requestingUserPersonId);

            if (userRoles.Contains(UserRoles.Thanos))
            {
                return;
            }

            if (!userRoles.Contains(UserRoles.OrganisationAdmin))
            {
                throw new ErrorResponseCodeException(ErrorResponseCode.InsufficientUserRole, HttpStatusCode.Forbidden);
            }

            if (!await _organisationsRepository.PersonIsOrganisationAdminAsync(organisationId, requestingUserPersonId))
            {
                throw new ErrorResponseCodeException(ErrorResponseCode.NotOrganisationAdmin, HttpStatusCode.Forbidden);
            }
        }

        /// <inheritdoc />
        public async Task ValidateUserHasAccessToPersonAsync(
            Guid requestingUserPersonId,
            Guid personId)
        {
            if (!await _peopleRepository.ExistsAsync(personId))
            {
                throw new EntityIdNotFoundException("Person", personId);
            }

            var userRoles = await GetUserRolesForPersonAsync(requestingUserPersonId);

            if (userRoles.Contains(UserRoles.Thanos))
            {
                return;
            }

            bool requestingUserAuthorized = requestingUserPersonId == personId;

            if (requestingUserAuthorized)
            {
                return;
            }

            if (!userRoles.Contains(UserRoles.SchoolSecretary) &&
                !userRoles.Contains(UserRoles.OrganisationAdmin))
            {
                throw new ErrorResponseCodeException(ErrorResponseCode.InsufficientUserRole, HttpStatusCode.Forbidden);
            }

            List<StudentSchoolDTO> studentSchools =
                await _schoolsRepository.GetSchoolsForPersonAsync(personId);

            foreach (StudentSchoolDTO studentSchool in studentSchools)
            {
                if (await _schoolsRepository.SchoolHasSecretaryAsync(
                    studentSchool.School.Id,
                    requestingUserPersonId))
                {
                    return;
                }
            }

            List<PersonOrganisationDTO> personOrganisations =
                await _organisationsRepository.GetOrganisationsForPersonAsync(personId);

            foreach (PersonOrganisationDTO personOrganisation in personOrganisations)
            {
                if (await _organisationsRepository.PersonIsOrganisationAdminAsync(
                    new Guid(personOrganisation.Organisation.Id), requestingUserPersonId))
                {
                    return;
                }
            }

            throw new ErrorResponseCodeException(ErrorResponseCode.NoAccessToPerson, HttpStatusCode.Forbidden);
        }

        /// <inheritdoc />
        public async Task<List<OrganisationDTO>> FilterOrganisationsByMemberAccess(
            IEnumerable<OrganisationDTO> organisations,
            Guid requestingUserPersonId)
        {
            var allowedOrganisations = new List<OrganisationDTO>();

            var userRoles = await GetUserRolesForPersonAsync(requestingUserPersonId);

            if (userRoles.Contains(UserRoles.Thanos))
            {
                return organisations.ToList();
            }

            foreach (OrganisationDTO organisation in organisations)
            {
                if (await TryCheckUserHasMemberAccessToOrganisationAsync(
                        requestingUserPersonId,
                        new Guid(organisation.Id)))
                {
                    allowedOrganisations.Add(organisation);
                }
            }

            return allowedOrganisations;
        }

        private async Task<bool> TryCheckUserHasMemberAccessToOrganisationAsync(Guid requestingUserPersonId, Guid organisationId)
        {
            try
            {
                await ValidateUserHasMemberAccessToOrganisationAsync(
                    requestingUserPersonId,
                    organisationId);

                return true;
            }
            catch (ErrorResponseCodeException)
            {
                return false;
            }
        }
    }
}
