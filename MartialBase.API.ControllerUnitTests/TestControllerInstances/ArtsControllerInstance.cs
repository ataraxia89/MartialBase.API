// <copyright file="ArtsControllerInstance.cs" company="Martialtech®">
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
    /// A class used in controller unit tests allowing access to the <see cref="ArtsController"/>.
    /// </summary>
    internal class ArtsControllerInstance : ControllerInstanceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArtsControllerInstance"/> class.
        /// </summary>
        /// <param name="artsRepository">The <see cref="IArtsRepository"/> instance to be injected into the <see cref="ArtsController"/> instance.</param>
        /// <param name="martialBaseUserHelper">The <see cref="IMartialBaseUserHelper"/> instance to be injected into the <see cref="ArtsController"/> instance.</param>
        /// <param name="azureUserHelper">The <see cref="IAzureUserHelper"/> instance to be injected into the <see cref="ArtsController"/> instance.</param>
        /// <param name="environmentName">The name of the environment to be used by the <see cref="ArtsController"/>.</param>
        /// <param name="martialBaseUser">The <see cref="MartialBaseUser"/> to be used when sending requests to the <see cref="ArtsController"/> instance.</param>
        internal ArtsControllerInstance(
            IArtsRepository artsRepository,
            IMartialBaseUserHelper martialBaseUserHelper,
            IAzureUserHelper azureUserHelper,
            string environmentName,
            MartialBaseUser martialBaseUser)
            : base(environmentName, martialBaseUser, martialBaseUserHelper, azureUserHelper)
        {
            Instance = new ArtsController(artsRepository, Environment);

            RegisterControllerInstance(Instance);
        }

        /// <summary>
        /// The current instance of the <see cref="ArtsController"/>, allowing access to controller methods.
        /// </summary>
        internal ArtsController Instance { get; }
    }
}
