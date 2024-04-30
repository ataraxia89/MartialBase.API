// <copyright file="MartialBaseUserControllerInstance.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.ControllerUnitTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Controllers;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;

using NSubstitute;

namespace MartialBase.API.ControllerUnitTests.TestControllerInstances
{
    internal class MartialBaseUserControllerInstance : ControllerInstanceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MartialBaseUserControllerInstance"/> class.
        /// </summary>
        /// <param name="azureUserHelper">The <see cref="IAzureUserHelper"/> instance to be injected into the <see cref="MartialBaseUserController"/> instance.</param>
        /// <param name="martialBaseUserHelper">The <see cref="IMartialBaseUserHelper"/> instance to be injected into the <see cref="MartialBaseUserController"/> instance.</param>
        /// <param name="martialBaseUsersRepository">The <see cref="IMartialBaseUsersRepository"/> instance to be injected into the <see cref="MartialBaseUserController"/> instance.</param>
        /// <param name="martialBaseUserRolesRepository">The <see cref="IMartialBaseUserRolesRepository"/> instance to be injected into the <see cref="MartialBaseUserController"/> instance.</param>
        /// <param name="userRolesRepository">The <see cref="IUserRolesRepository"/> instance to be injected into the <see cref="MartialBaseUserController"/> instance.</param>
        /// <param name="peopleRepository">The <see cref="IPeopleRepository"/> instance to be injected into the <see cref="MartialBaseUserController"/> instance.</param>
        /// <param name="environmentName">The name of the environment to be used by the <see cref="MartialBaseUserController"/>.</param>
        /// <param name="martialBaseUser">The <see cref="MartialBaseUser"/> to be used when sending requests to the <see cref="MartialBaseUserController"/> instance.</param>
        internal MartialBaseUserControllerInstance(
            IAzureUserHelper azureUserHelper,
            IMartialBaseUserHelper martialBaseUserHelper,
            IMartialBaseUsersRepository martialBaseUsersRepository,
            IMartialBaseUserRolesRepository martialBaseUserRolesRepository,
            IUserRolesRepository userRolesRepository,
            IPeopleRepository peopleRepository,
            string environmentName,
            MartialBaseUser martialBaseUser)
            : base(environmentName, martialBaseUser, martialBaseUserHelper, azureUserHelper)
        {
            Instance = new MartialBaseUserController(
                azureUserHelper,
                null,
                martialBaseUserRolesRepository,
                userRolesRepository,
                peopleRepository,
                Environment);

            RegisterControllerInstance(Instance);

            martialBaseUsersRepository
                .ExistsAsync(martialBaseUser.Id)
                .Returns(true);
        }

        /// <summary>
        /// The current instance of the <see cref="MartialBaseUserController"/>, allowing access to controller methods.
        /// </summary>
        internal MartialBaseUserController Instance { get; }
    }
}
