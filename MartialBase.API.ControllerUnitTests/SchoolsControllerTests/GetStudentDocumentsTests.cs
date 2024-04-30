// <copyright file="GetStudentDocumentsTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.ControllerUnitTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;

using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Controllers;
using MartialBase.API.ControllerUnitTests.TestControllerInstances;
using MartialBase.API.Data.Exceptions;
using MartialBase.API.Data.Exceptions.EntityFramework;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Models.DTOs.Documents;
using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.TestResources;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NSubstitute;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.ControllerUnitTests.SchoolsControllerTests
{
    /// <summary>
    /// Controller unit tests for the
    /// <see cref="SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> method in the
    /// <see cref="Controllers.SchoolsController"/>.
    /// </summary>
    public class GetStudentDocumentsTests
    {
        private IAddressesRepository _addressesRepository;
        private IArtsRepository _artsRepository;
        private ICountriesRepository _countriesRepository;
        private IDocumentsRepository _documentsRepository;
        private IDocumentTypesRepository _documentTypesRepository;
        private IMartialBaseUserHelper _martialBaseUserHelper;
        private IOrganisationsRepository _organisationsRepository;
        private IPeopleRepository _peopleRepository;
        private ISchoolsRepository _schoolsRepository;
        private IAzureUserHelper _azureUserHelper;
        private SchoolsControllerInstance _schoolsControllerInstance;
        private MartialBaseUser _testUser;

        private Guid _testSchoolId;
        private Guid _testPersonId;
        private List<DocumentDTO> _testDocuments;

        private SchoolsController SchoolsController => _schoolsControllerInstance.Instance;

        /// <summary>
        /// Test set up.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _addressesRepository = Substitute.For<IAddressesRepository>();
            _artsRepository = Substitute.For<IArtsRepository>();
            _countriesRepository = Substitute.For<ICountriesRepository>();
            _documentsRepository = Substitute.For<IDocumentsRepository>();
            _documentTypesRepository = Substitute.For<IDocumentTypesRepository>();
            _martialBaseUserHelper = Substitute.For<IMartialBaseUserHelper>();
            _organisationsRepository = Substitute.For<IOrganisationsRepository>();
            _peopleRepository = Substitute.For<IPeopleRepository>();
            _schoolsRepository = Substitute.For<ISchoolsRepository>();
            _azureUserHelper = Substitute.For<IAzureUserHelper>();

            _testUser = DataGenerator.MartialBaseUsers.GenerateMartialBaseUserObject();

            _schoolsControllerInstance = new SchoolsControllerInstance(
                _addressesRepository,
                _artsRepository,
                _countriesRepository,
                _documentsRepository,
                _documentTypesRepository,
                _martialBaseUserHelper,
                _organisationsRepository,
                _peopleRepository,
                _schoolsRepository,
                _azureUserHelper,
                "Live",
                _testUser);

            PrepareTestResources();
        }

        /// <summary>
        /// Verifies that the
        /// <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> returns a
        /// <see cref="StatusCodes.Status400BadRequest">BadRequest</see> response when provided with an invalid
        /// <c>includeInactive</c> boolean value.
        /// </summary>
        [Test]
        public async Task GetStudentDocumentsWithInvalidIncludeActiveParameterReturnsBadRequest()
        {
            // Act
            IActionResult result =
                await SchoolsController.GetStudentDocumentsAsync(
                    _testSchoolId.ToString(), _testPersonId.ToString(), "NotABool");

            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, objectResult.StatusCode);
            Assert.AreEqual("Invalid value provided for includeInactive parameter.", objectResult.Value);
        }

        /// <summary>
        /// Verifies that the
        /// <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> method
        /// returns a <see cref="StatusCodes.Status403Forbidden">Forbidden</see> response containing an
        /// <see cref="ErrorResponseCode"/> returned from the <see cref="IMartialBaseUserHelper"/> when checking
        /// for school access.
        /// </summary>
        /// <param name="errorResponseCode">The <see cref="ErrorResponseCode"/> to be returned in the response.</param>
        [TestCase(ErrorResponseCode.InsufficientUserRole)]
        [TestCase(ErrorResponseCode.AzureUserNotRegistered)]
        [TestCase(ErrorResponseCode.NotSchoolSecretary)]
        public async Task GetStudentDocumentsReturnsForbiddenWithErrorResponseCode(ErrorResponseCode errorResponseCode)
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _schoolsControllerInstance.SchoolAdminAccessReturnsForbidden(_testUser.PersonId, _testSchoolId, errorResponseCode);

            // Act
            try
            {
                await SchoolsController.GetStudentDocumentsAsync(
                    _testSchoolId.ToString(),
                    _testPersonId.ToString(),
                    "true");
            }
            catch (ErrorResponseCodeException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual(errorResponseCode, caughtException.ErrorResponseCode);
            Assert.AreEqual(HttpStatusCode.Forbidden, caughtException.StatusCode);
        }

        /// <summary>
        /// Verifies that the
        /// <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> method
        /// returns a <see cref="StatusCodes.Status404NotFound">NotFound</see> response when provided with an
        /// invalid <see cref="School"/> ID.
        /// </summary>
        [Test]
        public async Task GetStudentDocumentsFromANonExistentSchoolReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _schoolsControllerInstance.SchoolAdminAccessThrowsNotFoundException(_testUser.PersonId, _testSchoolId);

            // Act
            try
            {
                await SchoolsController.GetStudentDocumentsAsync(
                    _testSchoolId.ToString(),
                    _testPersonId.ToString(),
                    "true");
            }
            catch (EntityIdNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"School ID '{_testSchoolId}' not found.", caughtException.Message);
        }

        /// <summary>
        /// Verifies that the
        /// <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> method
        /// returns a <see cref="StatusCodes.Status404NotFound">NotFound</see> response when provided with an
        /// invalid <see cref="Person"/> ID.
        /// </summary>
        [Test]
        public async Task GetStudentDocumentsForANonExistentPersonReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _peopleRepository
                .ExistsAsync(_testPersonId)
                .Returns(false);

            // Act
            try
            {
                await SchoolsController.GetStudentDocumentsAsync(
                    _testSchoolId.ToString(),
                    _testPersonId.ToString(),
                    "true");
            }
            catch (EntityIdNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Person ID '{_testPersonId}' not found.", caughtException.Message);
        }

        /// <summary>
        /// Verifies that the
        /// <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> method
        /// returns a <see cref="StatusCodes.Status404NotFound">NotFound</see> response when provided with a
        /// <see cref="Person"/> ID that doesn't belong to the specified <see cref="School"/>.
        /// </summary>
        [Test]
        public async Task GetStudentDocumentsReturnsNotFoundWhenStudentDoesNotBelongToSchool()
        {
            // Arrange
            EntityNotFoundException caughtException = null;

            _schoolsRepository
                .SchoolHasStudentAsync(_testSchoolId, _testPersonId)
                .Returns(false);

            // Act
            try
            {
                await SchoolsController.GetStudentDocumentsAsync(
                    _testSchoolId.ToString(),
                    _testPersonId.ToString(),
                    "true");
            }
            catch (EntityNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Person '{_testPersonId}' not found in school '{_testSchoolId}'", caughtException.Message);
        }

        /// <summary>
        /// Verifies that the
        /// <see cref="Controllers.SchoolsController.GetStudentDocumentsAsync">GetStudentDocuments</see> method
        /// successfully returns the requested <see cref="List{T}"/> of <see cref="DocumentDTO"/> objects to a
        /// school secretary.
        /// </summary>
        [Test]
        public async Task SchoolSecretaryCanGetStudentDocuments()
        {
            // Arrange
            // Act
            IActionResult result =
                await SchoolsController.GetStudentDocumentsAsync(
                    _testSchoolId.ToString(), _testPersonId.ToString(), "true");

            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<List<DocumentDTO>>());

            var retrievedDocuments = (List<DocumentDTO>)objectResult.Value;

            DocumentResources.AssertEqual(_testDocuments, retrievedDocuments);
        }

        private void PrepareTestResources()
        {
            _testDocuments = DataGenerator.Documents.GenerateDocumentDTOs(10);

            _testSchoolId = Guid.NewGuid();
            _testPersonId = Guid.NewGuid();

            _schoolsRepository
                .ExistsAsync(_testSchoolId)
                .Returns(true);

            _peopleRepository
                .ExistsAsync(_testPersonId)
                .Returns(true);

            _schoolsRepository
                .SchoolHasStudentAsync(_testSchoolId, _testPersonId)
                .Returns(true);

            _peopleRepository
                .GetPersonDocumentsAsync(_testPersonId, true)
                .Returns(_testDocuments);
        }
    }
}
