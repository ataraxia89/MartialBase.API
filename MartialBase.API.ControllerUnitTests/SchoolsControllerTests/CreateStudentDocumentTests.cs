// <copyright file="CreateStudentDocumentTests.cs" company="Martialtech®">
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
using MartialBase.API.Data.Models.InternalDTOs.Documents;
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
    /// <see cref="SchoolsController.CreateStudentDocumentAsync">CreateStudentDocument</see> method in the
    /// <see cref="Controllers.SchoolsController"/>.
    /// </summary>
    public class CreateStudentDocumentTests
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
        private Guid _testDocumentTypeId;
        private CreateDocumentDTO _testCreateDocumentDTO;
        private CreateDocumentInternalDTO _testCreateDocumentInternalDTO;
        private DocumentDTO _testDocumentDTO;

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
        /// <see cref="Controllers.SchoolsController.CreateStudentDocumentAsync">CreateStudentDocument</see> endpoint
        /// returns an <see cref="StatusCodes.Status500InternalServerError">InternalServerError</see> response
        /// containing a list of model state errors when provided with an invalid
        /// <see cref="CreateDocumentDTO"/> object.
        /// </summary>
        [Test]
        public async Task CreateStudentDocumentWithInvalidDTOReturnsInternalServerErrorWithDetails()
        {
            // Arrange
            var testErrors = new Dictionary<string, string>();
            var expectedModelStateErrors = new Dictionary<string, string[]>();

            for (int i = 0; i < 10; i++)
            {
                expectedModelStateErrors.Add(Guid.NewGuid().ToString(), new[] { Guid.NewGuid().ToString() });
            }

            foreach (KeyValuePair<string, string[]> expectedModelStateError in expectedModelStateErrors)
            {
                testErrors.Add(expectedModelStateError.Key, expectedModelStateError.Value[0]);
            }

            foreach (KeyValuePair<string, string> testError in testErrors)
            {
                SchoolsController.ModelState.AddModelError(testError.Key, testError.Value);
            }

            // Act
            IActionResult result =
                await SchoolsController.CreateStudentDocumentAsync(
                    _testSchoolId.ToString(), _testPersonId.ToString(), _testCreateDocumentDTO);

            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<Dictionary<string, string[]>>());

            DictionaryResources.AssertEqual(expectedModelStateErrors, (Dictionary<string, string[]>)objectResult.Value);
        }

        /// <summary>
        /// Verifies that the
        /// <see cref="Controllers.SchoolsController.CreateStudentDocumentAsync">CreateStudentDocument</see> method
        /// returns a <see cref="StatusCodes.Status403Forbidden">Forbidden</see> response containing an
        /// <see cref="ErrorResponseCode"/> returned from the <see cref="IMartialBaseUserHelper"/> when checking
        /// for school access.
        /// </summary>
        /// <param name="errorResponseCode">The <see cref="ErrorResponseCode"/> to be returned in the response.</param>
        [TestCase(ErrorResponseCode.InsufficientUserRole)]
        [TestCase(ErrorResponseCode.AzureUserNotRegistered)]
        [TestCase(ErrorResponseCode.NotSchoolSecretary)]
        public async Task CreateStudentDocumentReturnsForbiddenWithErrorResponseCode(ErrorResponseCode errorResponseCode)
        {
            // Arrange
            ErrorResponseCodeException caughtException = null;

            _schoolsControllerInstance.SchoolAdminAccessReturnsForbidden(_testUser.PersonId, _testSchoolId, errorResponseCode);

            // Act
            try
            {
                await SchoolsController.CreateStudentDocumentAsync(
                    _testSchoolId.ToString(), _testPersonId.ToString(), _testCreateDocumentDTO);
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
        /// <see cref="Controllers.SchoolsController.CreateStudentDocumentAsync">CreateStudentDocument</see> method
        /// returns a <see cref="StatusCodes.Status404NotFound">NotFound</see> response when provided with an
        /// invalid <see cref="School"/> ID.
        /// </summary>
        [Test]
        public async Task CreateStudentDocumentFromANonExistentSchoolReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _schoolsControllerInstance.SchoolAdminAccessThrowsNotFoundException(_testUser.PersonId, _testSchoolId);

            // Act
            try
            {
                await SchoolsController.CreateStudentDocumentAsync(
                    _testSchoolId.ToString(),
                    _testPersonId.ToString(),
                    _testCreateDocumentDTO);
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
        /// <see cref="Controllers.SchoolsController.CreateStudentDocumentAsync">CreateStudentDocument</see> method
        /// returns a <see cref="StatusCodes.Status404NotFound">NotFound</see> response when provided with a
        /// <see cref="CreateDocumentDTO"/> object containing an invalid <see cref="DocumentType"/> ID.
        /// </summary>
        [Test]
        public async Task CreateStudentDocumentWithInvalidDocumentTypeReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;
            var invalidDocumentTypeId = Guid.NewGuid();

            _documentTypesRepository
                .ExistsAsync(invalidDocumentTypeId)
                .Returns(false);

            _testCreateDocumentDTO.DocumentTypeId = invalidDocumentTypeId.ToString();

            // Act
            try
            {
                await SchoolsController.CreateStudentDocumentAsync(
                    _testSchoolId.ToString(),
                    _testPersonId.ToString(),
                    _testCreateDocumentDTO);
            }
            catch (EntityIdNotFoundException ex)
            {
                caughtException = ex;
            }

            // Assert
            Assert.NotNull(caughtException);
            Assert.AreEqual($"Document type ID '{invalidDocumentTypeId}' not found.", caughtException.Message);
        }

        /// <summary>
        /// Verifies that the
        /// <see cref="Controllers.SchoolsController.CreateStudentDocumentAsync">CreateStudentDocument</see> method
        /// returns a <see cref="StatusCodes.Status404NotFound">NotFound</see> response when provided with an
        /// invalid <see cref="Person"/> ID.
        /// </summary>
        [Test]
        public async Task CreateStudentDocumentForANonExistentPersonReturnsNotFound()
        {
            // Arrange
            EntityIdNotFoundException caughtException = null;

            _peopleRepository
                .ExistsAsync(_testPersonId)
                .Returns(false);

            // Act
            try
            {
                await SchoolsController.CreateStudentDocumentAsync(
                    _testSchoolId.ToString(),
                    _testPersonId.ToString(),
                    _testCreateDocumentDTO);
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
        /// <see cref="Controllers.SchoolsController.CreateStudentDocumentAsync">CreateStudentDocument</see> method
        /// returns a <see cref="StatusCodes.Status404NotFound">NotFound</see> response when provided with a
        /// <see cref="Person"/> ID that doesn't belong to the specified <see cref="School"/>.
        /// </summary>
        [Test]
        public async Task CreateStudentDocumentReturnsNotFoundWhenStudentDoesNotBelongToSchool()
        {
            // Arrange
            EntityNotFoundException caughtException = null;

            _schoolsRepository
                .SchoolHasStudentAsync(_testSchoolId, _testPersonId)
                .Returns(false);

            // Act
            try
            {
                await SchoolsController.CreateStudentDocumentAsync(
                    _testSchoolId.ToString(),
                    _testPersonId.ToString(),
                    _testCreateDocumentDTO);
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
        /// <see cref="Controllers.SchoolsController.CreateStudentDocumentAsync">CreateStudentDocument</see> method
        /// successfully creates a <see cref="Document"/> and returns a representative <see cref="DocumentDTO"/>
        /// to a school secretary.
        /// </summary>
        [Test]
        public async Task SchoolSecretaryCanGetStudentDocument()
        {
            // Arrange
            _peopleRepository
                .CreatePersonDocumentAsync(_testPersonId, _testCreateDocumentInternalDTO)
                .Returns(_testDocumentDTO);

            _peopleRepository
                .SaveChangesAsync()
                .Returns(true);

            // Act
            IActionResult result =
                await SchoolsController.CreateStudentDocumentAsync(
                    _testSchoolId.ToString(), _testPersonId.ToString(), _testCreateDocumentDTO);

            var objectResult = result as ObjectResult;

            // Assert
            Assert.NotNull(objectResult);
            Assert.AreEqual(StatusCodes.Status201Created, objectResult.StatusCode);
            Assert.That(objectResult.Value, Is.TypeOf<DocumentDTO>());

            var retrievedDocument = (DocumentDTO)objectResult.Value;

            DocumentResources.AssertEqual(_testCreateDocumentDTO, retrievedDocument);
        }

        private void PrepareTestResources()
        {
            _testSchoolId = Guid.NewGuid();
            _testPersonId = Guid.NewGuid();
            _testDocumentTypeId = Guid.NewGuid();

            _testCreateDocumentDTO = DataGenerator.Documents.GenerateCreateDocumentDTO(_testDocumentTypeId);
            _testCreateDocumentInternalDTO =
                DataGenerator.Documents.GenerateCreateDocumentInternalDTO(_testCreateDocumentDTO);
            _testDocumentDTO = DataGenerator.Documents.GenerateDocumentDTO(_testCreateDocumentInternalDTO);

            _schoolsRepository
                .ExistsAsync(_testSchoolId)
                .Returns(true);

            _peopleRepository
                .ExistsAsync(_testPersonId)
                .Returns(true);

            _schoolsRepository
                .SchoolHasStudentAsync(_testSchoolId, _testPersonId)
                .Returns(true);

            _documentTypesRepository
                .ExistsAsync(_testDocumentTypeId)
                .Returns(true);

            _peopleRepository
                .CreatePersonDocumentAsync(_testPersonId, _testCreateDocumentInternalDTO)
                .ReturnsForAnyArgs(_testDocumentDTO);
        }
    }
}
