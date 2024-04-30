// <copyright file="GetArtsTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.ControllerUnitTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;

using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Controllers;
using MartialBase.API.ControllerUnitTests.TestControllerInstances;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Arts;
using MartialBase.API.TestTools.TestResources;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.ControllerUnitTests.ArtsControllerTests
{
    public class GetArtsTests
    {
        private IArtsRepository _artsRepository;
        private IMartialBaseUserHelper _martialBaseUserHelper;
        private IAzureUserHelper _azureUserHelper;
        private ArtsControllerInstance _artsControllerInstance;
        private MartialBaseUser _testUser;

        private ArtsController ArtsController => _artsControllerInstance.Instance;

        [SetUp]
        public void Setup()
        {
            _artsRepository = Substitute.For<IArtsRepository>();
            _martialBaseUserHelper = Substitute.For<IMartialBaseUserHelper>();
            _azureUserHelper = Substitute.For<IAzureUserHelper>();

            _testUser =
                DataGenerator.MartialBaseUsers.GenerateMartialBaseUserObject();

            _artsControllerInstance =
                new ArtsControllerInstance(
                    _artsRepository,
                    _martialBaseUserHelper,
                    _azureUserHelper,
                    "Live",
                    _testUser);
        }

        [Test]
        public async Task CanGetArts()
        {
            // Arrange
            var testArts = new List<ArtDTO>();

            for (int i = 0; i < 100; i++)
            {
                testArts.Add(DataGenerator.Arts.GenerateArtDTO());
            }

            _artsRepository
                .GetAllAsync()
                .Returns(testArts);

            // Act
            IActionResult result = await ArtsController.GetArtsAsync();
            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<List<ArtDTO>>());

            var retrievedArts = (List<ArtDTO>)objectResult.Value;

            ArtResources.AssertEqual(testArts, retrievedArts);
        }
    }
}
