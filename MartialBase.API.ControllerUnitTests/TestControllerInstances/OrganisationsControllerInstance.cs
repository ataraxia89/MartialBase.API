// <copyright file="OrganisationsControllerInstance.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.ControllerUnitTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Controllers;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;

namespace MartialBase.API.ControllerUnitTests.TestControllerInstances
{
    /// <summary>
    /// A class used in controller unit tests allowing access to the <see cref="OrganisationsController"/>.
    /// </summary>
    internal class OrganisationsControllerInstance : ControllerInstanceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationsControllerInstance"/> class.
        /// </summary>
        /// <param name="countriesRepository">The <see cref="ICountriesRepository"/> instance to be injected into the <see cref="OrganisationsController"/> instance.</param>
        /// <param name="documentTypesRepository">The <see cref="IDocumentTypesRepository"/> instance to be injected into the <see cref="OrganisationsController"/> instance.</param>
        /// <param name="martialBaseUserHelper">The <see cref="IMartialBaseUserHelper"/> instance to be injected into the <see cref="OrganisationsController"/> instance.</param>
        /// <param name="martialBaseUserRolesRepository">The <see cref="IMartialBaseUserRolesRepository"/> instance to be injected into the <see cref="OrganisationsController"/> instance.</param>
        /// <param name="organisationsRepository">The <see cref="IOrganisationsRepository"/> instance to be injected into the <see cref="OrganisationsController"/> instance.</param>
        /// <param name="peopleRepository">The <see cref="IPeopleRepository"/> instance to be injected into the <see cref="OrganisationsController"/> instance.</param>
        /// <param name="azureUserHelper">The <see cref="IAzureUserHelper"/> instance to be injected into the <see cref="OrganisationsController"/> instance.</param>
        /// <param name="environmentName">The name of the environment to be used by the <see cref="OrganisationsController"/>.</param>
        /// <param name="martialBaseUser">The <see cref="MartialBaseUser"/> to be used when sending requests to the <see cref="OrganisationsController"/> instance.</param>
        internal OrganisationsControllerInstance(
            ICountriesRepository countriesRepository,
            IDocumentTypesRepository documentTypesRepository,
            IMartialBaseUserHelper martialBaseUserHelper,
            IMartialBaseUserRolesRepository martialBaseUserRolesRepository,
            IOrganisationsRepository organisationsRepository,
            IPeopleRepository peopleRepository,
            IAzureUserHelper azureUserHelper,
            string environmentName,
            MartialBaseUser martialBaseUser)
            : base(environmentName, martialBaseUser, martialBaseUserHelper, azureUserHelper)
        {
            Instance = new OrganisationsController(
                countriesRepository,
                documentTypesRepository,
                martialBaseUserHelper,
                martialBaseUserRolesRepository,
                organisationsRepository,
                peopleRepository,
                azureUserHelper,
                Environment);

            RegisterControllerInstance(Instance);
        }

        /// <summary>
        /// The current instance of the <see cref="OrganisationsController"/>, allowing access to controller methods.
        /// </summary>
        internal OrganisationsController Instance { get; }
    }
}
