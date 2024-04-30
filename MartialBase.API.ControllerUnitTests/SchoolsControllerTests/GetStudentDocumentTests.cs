// <copyright file="GetStudentDocumentTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.ControllerUnitTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
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
    /// <see cref="SchoolsController.GetStudentDocumentAsync">GetStudentDocument</see> method in the
    /// <see cref="Controllers.SchoolsController"/>.
    /// </summary>
    public class GetStudentDocumentTests
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
        private Guid _testDocumentId;
        private DocumentDTO _testDocument;

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
        /// <see cref="Controllers.SchoolsController.GetStudentDocumentAsync">GetStudentDocument</see> method
        /// returns a <see cref="StatusCodes.Status403Forbidden">Forbidden</see> response containing an
        /// <see cref="ErrorResponseCode"/> returned from the <see cref="IMartialBaseUserHelper"/> when checking
        /// for school access.
        /// </summary>
        /// <param name="errorResponseCode">The <see cref="ErrorResponseCode"/> to be returned in the response.</param>
        [TestCase(ErrorResponseCode.InsufficientUserRole)]
        [TestCase(ErrorResponseCode.AzureUserNotRegistered)]
        [TestCase(ErrorResponseCode.NotSchoolSecretary)]
        public async Task GetStudentDocumentReturnsForbiddenWithErrorResponseCode(ErrorResponseCode errorResponseCode)
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _schoolsControllerInstance.SchoolAdminAccessReturnsForbidden(_testUser.PersonId, _testSchoolId, errorResponseCode);

            // Act
            try
            {
                await SchoolsController.GetStudentDocumentAsync(
                    _testSchoolId.ToString(),
                    _testPersonId.ToString(),
                    _testDocumentId.ToString());
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
        /// <see cref="Controllers.SchoolsController.GetStudentDocumentAsync">GetStudentDocument</see> method returns
        /// a <see cref="StatusCodes.Status404NotFound">NotFound</see> response when provided with an invalid
        /// <see cref="School"/> ID.
        /// </summary>
        [Test]
        public async Task GetStudentDocumentFromANonExistentSchoolReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _schoolsControllerInstance.SchoolAdminAccessThrowsNotFoundException(_testUser.PersonId, _testSchoolId);

            // Act
            try
            {
                await SchoolsController.GetStudentDocumentAsync(
                    _testSchoolId.ToString(),
                    _testPersonId.ToString(),
                    _testDocumentId.ToString());
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
        /// <see cref="Controllers.SchoolsController.GetStudentDocumentAsync">GetStudentDocument</see> method returns
        /// a <see cref="StatusCodes.Status404NotFound">NotFound</see> response when provided with an invalid
        /// <see cref="Person"/> ID.
        /// </summary>
        [Test]
        public async Task GetStudentDocumentForANonExistentPersonReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;
            var invalidStudentId = Guid.NewGuid();

            _peopleRepository
                .ExistsAsync(invalidStudentId)
                .Returns(false);

            // Act
            try
            {
                await SchoolsController.GetStudentDocumentAsync(
                    _testSchoolId.ToString(),
                    invalidStudentId.ToString(),
                    _testDocumentId.ToString());
            }
            catch (EntityIdNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Person ID '{invalidStudentId}' not found.", caughtException.Message);
        }

        /// <summary>
        /// Verifies that the
        /// <see cref="Controllers.SchoolsController.GetStudentDocumentAsync">GetStudentDocument</see> method returns
        /// a <see cref="StatusCodes.Status404NotFound">NotFound</see> response when provided with an invalid
        /// <see cref="Document"/> ID.
        /// </summary>
        [Test]
        public async Task GetStudentDocumentForANonExistentDocumentReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;
            var invalidDocumentId = Guid.NewGuid();

            _documentsRepository
                .ExistsAsync(invalidDocumentId)
                .Returns(false);

            // Act
            try
            {
                await SchoolsController.GetStudentDocumentAsync(
                    _testSchoolId.ToString(),
                    _testPersonId.ToString(),
                    invalidDocumentId.ToString());
            }
            catch (EntityIdNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Document ID '{invalidDocumentId}' not found.", caughtException.Message);
        }

        /// <summary>
        /// Verifies that the
        /// <see cref="Controllers.SchoolsController.GetStudentDocumentAsync">GetStudentDocument</see> method
        /// returns a <see cref="StatusCodes.Status404NotFound">NotFound</see> response when provided with a
        /// <see cref="Person"/> ID that doesn't belong to the specified <see cref="School"/>.
        /// </summary>
        [Test]
        public async Task GetStudentDocumentReturnsNotFoundWhenStudentDoesNotBelongToSchool()
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
        /// <see cref="Controllers.SchoolsController.GetStudentDocumentAsync">GetStudentDocument</see> method
        /// successfully returns the requested <see cref="DocumentDTO"/> to a school secretary.
        /// </summary>
        [Test]
        public async Task SchoolSecretaryCanGetStudentDocument()
        {
            // Act
            IActionResult result =
                await SchoolsController.GetStudentDocumentAsync(
                    _testSchoolId.ToString(), _testPersonId.ToString(), _testDocumentId.ToString());

            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<DocumentDTO>());

            var retrievedDocument = (DocumentDTO)objectResult.Value;

            DocumentResources.AssertEqual(_testDocument, retrievedDocument);
        }

        private void PrepareTestResources()
        {
            _testDocument = DataGenerator.Documents.GenerateDocumentDTO();

            _testSchoolId = Guid.NewGuid();
            _testPersonId = Guid.NewGuid();
            _testDocumentId = new Guid(_testDocument.Id);

            _schoolsRepository
                .ExistsAsync(_testSchoolId)
                .Returns(true);

            _peopleRepository
                .ExistsAsync(_testPersonId)
                .Returns(true);

            _schoolsRepository
                .SchoolHasStudentAsync(_testSchoolId, _testPersonId)
                .Returns(true);

            _documentsRepository
                .ExistsAsync(_testDocumentId)
                .Returns(true);

            _peopleRepository
                .GetPersonDocumentAsync(_testPersonId, _testDocumentId)
                .Returns(_testDocument);
        }
    }
}
