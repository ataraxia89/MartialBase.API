// <copyright file="ControllerInstanceBase.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.ControllerUnitTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;

using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Controllers;
using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Exceptions;
using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.Enums;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

using Xunit;

namespace MartialBase.API.ControllerUnitTests.TestControllerInstances
{
    public class ControllerInstanceBase
    {
        private readonly MartialBaseUser _testUser;
        private readonly Guid _azureId;
        private List<string> _authorizedRoles;
        private MartialBaseControllerBase _controller;

        public ControllerInstanceBase(
            string environmentName,
            MartialBaseUser testUser,
            IMartialBaseUserHelper martialBaseUserHelper,
            IAzureUserHelper azureUserHelper)
        {
            EnvironmentName = environmentName;

            Environment = Substitute.For<IWebHostEnvironment>();

            Environment
                .EnvironmentName
                .Returns(EnvironmentName);

            _testUser = testUser;
            _azureId = Guid.NewGuid();

            MartialBaseUserHelper = martialBaseUserHelper;
            AzureUserHelper = azureUserHelper;

            MartialBaseUserHelper
                .VerifyUserHasAnyAcceptedScope(Arg.Any<HttpContext>(), "query")
                .Returns(true);

            AzureUserHelper
                .GetAzureIdAndInvitationCodeFromHttpRequest(Arg.Any<HttpRequest>())
                .Returns((_azureId, testUser.InvitationCode));

            AzureUserHelper
                .GetPersonIdForAzureUserAsync(_azureId, testUser.InvitationCode)
                .Returns(_testUser.PersonId);

            MartialBaseUserHelper
                .GetPersonIdFromHttpRequestAsync(Arg.Any<HttpRequest>())
                .Returns(_testUser.PersonId);
        }

        internal string EnvironmentName { get; }

        internal IWebHostEnvironment Environment { get; }

        internal IMartialBaseUserHelper MartialBaseUserHelper { get; }

        internal IAzureUserHelper AzureUserHelper { get; }

        internal List<string> TestUserRoles => _authorizedRoles;

        internal Guid RequestUserAzureId => _azureId;

        internal void RegisterControllerInstance(MartialBaseControllerBase controller)
        {
            _controller = controller;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            SetRequestingPersonId(_testUser.PersonId);
            SetTestUserRole(UserRoles.User);
        }

        internal void RemoveTestUserPerson()
        {
            AzureUserHelper
                .GetPersonIdForAzureUserAsync(_azureId, _testUser.InvitationCode)
                .Returns((Guid?)null);
        }

        internal void SetTestUserRole(string authorizedRole) => SetTestUserRole(new List<string> { authorizedRole });

        internal void SetTestUserRole(List<string> authorizedRoles)
        {
            _authorizedRoles = authorizedRoles;

            Assert.NotNull(_testUser.AzureId);

            foreach (string authorizedRole in _authorizedRoles)
            {
                AzureUserHelper
                    .AzureUserHasRequiredRoleAsync((Guid)_testUser.AzureId, authorizedRole)
                    .Returns(true);
            }

            MartialBaseUserHelper
                .GetUserRolesForPersonAsync(_testUser.PersonId)
                .Returns(_authorizedRoles);

            SetRequestingUserRoles(authorizedRoles);
        }

        internal void OrganisationAdminAccessThrowsNotFoundException(Guid invalidOrganisationId)
        {
            MartialBaseUserHelper
                .ValidateUserHasAdminAccessToOrganisationAsync(_testUser.PersonId, invalidOrganisationId)
                .Throws(new EntityIdNotFoundException("Organisation", invalidOrganisationId));
        }

        internal void OrganisationAdminAccessReturnsForbidden(
            Guid testOrganisationId, ErrorResponseCode errorResponseCode)
        {
            MartialBaseUserHelper
                .ValidateUserHasAdminAccessToOrganisationAsync(_testUser.PersonId, testOrganisationId)
                .Throws(new ErrorResponseCodeException(errorResponseCode, HttpStatusCode.Forbidden));
        }

        internal void OrganisationAdminAccessThrowsUnauthorizedAccessException(Guid testOrganisationId)
        {
            MartialBaseUserHelper
                .ValidateUserHasAdminAccessToOrganisationAsync(_testUser.PersonId, testOrganisationId)
                .Throws(new UnauthorizedAccessException("Auth token does not contain a valid user ID."));
        }

        internal void OrganisationMemberAccessThrowsNotFoundException(Guid invalidOrganisationId)
        {
            MartialBaseUserHelper
                .ValidateUserHasMemberAccessToOrganisationAsync(_testUser.PersonId, invalidOrganisationId)
                .Throws(new EntityIdNotFoundException("Organisation", invalidOrganisationId));
        }

        internal void OrganisationMemberAccessReturnsForbidden(
            Guid testOrganisationId, ErrorResponseCode errorResponseCode)
        {
            MartialBaseUserHelper
                .ValidateUserHasMemberAccessToOrganisationAsync(_testUser.PersonId, testOrganisationId)
                .Throws(new ErrorResponseCodeException(errorResponseCode, HttpStatusCode.Forbidden));
        }

        internal void OrganisationMemberAccessThrowsUnauthorizedAccessException(Guid testOrganisationId)
        {
            MartialBaseUserHelper
                .ValidateUserHasMemberAccessToOrganisationAsync(_testUser.PersonId, testOrganisationId)
                .Throws(new UnauthorizedAccessException("Auth token does not contain a valid user ID."));
        }

        private void SetRequestingPersonId(Guid personId)
        {
            _controller.ControllerContext.HttpContext.Request.Headers["RequestingPersonId"] =
                new StringValues(personId.ToString());
        }

        private void SetRequestingUserRoles(List<string> authorizedRoles)
        {
            _controller.ControllerContext.HttpContext.Request.Headers["RequestingUserRoles"] =
                new StringValues(authorizedRoles.ToArray());
        }
    }
}
