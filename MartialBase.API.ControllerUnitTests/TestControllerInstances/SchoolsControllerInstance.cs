// <copyright file="SchoolsControllerInstance.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.ControllerUnitTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Net;

using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Controllers;
using MartialBase.API.Data.Exceptions;
using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.Enums;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace MartialBase.API.ControllerUnitTests.TestControllerInstances
{
    /// <summary>
    /// A class used in controller unit tests allowing access to the <see cref="SchoolsController"/>.
    /// </summary>
    internal class SchoolsControllerInstance : ControllerInstanceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchoolsControllerInstance"/> class.
        /// </summary>
        /// <param name="addressesRepository">The <see cref="IAddressesRepository"/> instance to be injected into the <see cref="SchoolsController"/> instance.</param>
        /// <param name="artsRepository">The <see cref="IArtsRepository"/> instance to be injected into the <see cref="SchoolsController"/> instance.</param>
        /// <param name="countriesRepository">The <see cref="ICountriesRepository"/> instance to be injected into the <see cref="SchoolsController"/> instance.</param>
        /// <param name="documentsRepository">The <see cref="IDocumentsRepository"/> instance to be injected into the <see cref="SchoolsController"/> instance.</param>
        /// <param name="documentTypesRepository">The <see cref="IDocumentTypesRepository"/> instance to be injected into the <see cref="SchoolsController"/> instance.</param>
        /// <param name="martialBaseUserHelper">The <see cref="IMartialBaseUserHelper"/> instance to be injected into the <see cref="SchoolsController"/> instance.</param>
        /// <param name="organisationsRepository">The <see cref="IOrganisationsRepository"/> instance to be injected into the <see cref="SchoolsController"/> instance.</param>
        /// <param name="peopleRepository">The <see cref="IPeopleRepository"/> instance to be injected into the <see cref="SchoolsController"/> instance.</param>
        /// <param name="schoolsRepository">The <see cref="ISchoolsRepository"/> instance to be injected into the <see cref="SchoolsController"/> instance.</param>
        /// <param name="azureUserHelper">The <see cref="IAzureUserHelper"/> instance to be injected into the <see cref="SchoolsController"/> instance.</param>
        /// <param name="environmentName">The name of the environment to be used by the <see cref="SchoolsController"/>.</param>
        /// <param name="martialBaseUser">The <see cref="MartialBaseUser"/> to be used when sending requests to the <see cref="SchoolsController"/> instance.</param>
        internal SchoolsControllerInstance(
            IAddressesRepository addressesRepository,
            IArtsRepository artsRepository,
            ICountriesRepository countriesRepository,
            IDocumentsRepository documentsRepository,
            IDocumentTypesRepository documentTypesRepository,
            IMartialBaseUserHelper martialBaseUserHelper,
            IOrganisationsRepository organisationsRepository,
            IPeopleRepository peopleRepository,
            ISchoolsRepository schoolsRepository,
            IAzureUserHelper azureUserHelper,
            string environmentName,
            MartialBaseUser martialBaseUser)
        : base(environmentName, martialBaseUser, martialBaseUserHelper, azureUserHelper)
        {
            Instance = new SchoolsController(
                addressesRepository,
                artsRepository,
                countriesRepository,
                documentsRepository,
                documentTypesRepository,
                martialBaseUserHelper,
                organisationsRepository,
                peopleRepository,
                schoolsRepository,
                Environment);

            RegisterControllerInstance(Instance);
        }

        /// <summary>
        /// The current instance of the <see cref="SchoolsController"/>, allowing access to controller methods.
        /// </summary>
        internal SchoolsController Instance { get; }

        internal void SchoolAdminAccessThrowsNotFoundException(Guid requestingPersonId, Guid invalidSchoolId)
        {
            MartialBaseUserHelper
                .ValidateUserHasAdminAccessToSchoolAsync(requestingPersonId, invalidSchoolId)
                .Throws(new EntityIdNotFoundException("School", invalidSchoolId));
        }

        internal void SchoolAdminAccessReturnsForbidden(
            Guid requestingPersonId, Guid testSchoolId, ErrorResponseCode errorResponseCode)
        {
            MartialBaseUserHelper
                .ValidateUserHasAdminAccessToSchoolAsync(requestingPersonId, testSchoolId)
                .Throws(new ErrorResponseCodeException(errorResponseCode, HttpStatusCode.Forbidden));
        }

        internal void PersonAccessThrowsNotFoundException(Guid requestingPersonId, Guid invalidPersonId)
        {
            MartialBaseUserHelper
                .ValidateUserHasAccessToPersonAsync(requestingPersonId, invalidPersonId)
                .Throws(new EntityIdNotFoundException("Person", invalidPersonId));
        }

        internal void PersonAccessReturnsForbidden(
            Guid requestingPersonId, Guid testPersonId, ErrorResponseCode errorResponseCode)
        {
            MartialBaseUserHelper
                .ValidateUserHasAccessToPersonAsync(requestingPersonId, testPersonId)
                .Throws(new ErrorResponseCodeException(errorResponseCode, HttpStatusCode.Forbidden));
        }
    }
}
