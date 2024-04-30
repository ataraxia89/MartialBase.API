// <copyright file="CreateTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.People;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.PeopleRepositoryTests
{
    public class CreateTests : BaseTestClass
    {
        [Test]
        public async Task CreatePersonWithNoParametersThrowsNotImplementedException() => Assert.That(
            async () =>
                await PeopleRepository.CreateAsync(new CreatePersonInternalDTO()),
            Throws.Exception.TypeOf<NotImplementedException>());

        [Test]
        public async Task CreatePersonWithNoOrganisationThrowsArgumentNullException() => Assert.That(
            async () =>
                await PeopleRepository.CreateAsync(new CreatePersonInternalDTO(), null, null),
            Throws.Exception.TypeOf<ArgumentNullException>());

        [Test]
        public async Task CanCreatePersonWithOrganisationAndNoSchool()
        {
            CreatePersonInternalDTO createPersonInternalDTO = DataGenerator.People.GenerateCreatePersonInternalDTO();
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            PersonDTO createdPerson = await PeopleRepository.CreateAsync(
                createPersonInternalDTO, testOrganisation.Id, null);

            Assert.IsTrue(await PeopleRepository.SaveChangesAsync());

            PersonResources.AssertEqual(createPersonInternalDTO, createdPerson);
            PersonResources.AssertExists(createdPerson, DbIdentifier);

            (bool personExists, _) = OrganisationPersonResources.CheckOrganisationHasPerson(
                testOrganisation.Id,
                createdPerson.Id,
                DbIdentifier);

            Assert.IsTrue(personExists);
        }

        [Test]
        public async Task CanCreatePersonWithSchoolAndNoOrganisation()
        {
            CreatePersonInternalDTO createPersonInternalDTO =
                DataGenerator.People.GenerateCreatePersonInternalDTO();
            School testSchool = SchoolResources.CreateTestSchool(DbIdentifier);

            PersonDTO createdPerson = await PeopleRepository.CreateAsync(
                createPersonInternalDTO, null, testSchool.Id);

            Assert.IsTrue(await PeopleRepository.SaveChangesAsync());

            PersonResources.AssertEqual(createPersonInternalDTO, createdPerson);
            PersonResources.AssertExists(createdPerson, DbIdentifier);

            (bool studentExists, bool isInstructor, bool isSecretary) = SchoolStudentResources.CheckSchoolHasStudent(testSchool.Id, createdPerson.Id, DbIdentifier);

            Assert.IsTrue(studentExists);
            Assert.IsFalse(isInstructor);
            Assert.IsFalse(isSecretary);

            (bool personExists, bool personIsAdmin) = OrganisationPersonResources.CheckOrganisationHasPerson(
                testSchool.Organisation.Id,
                createdPerson.Id,
                DbIdentifier);

            Assert.IsTrue(personExists);
            Assert.IsFalse(personIsAdmin);
        }

        [Test]
        public async Task CanCreatePersonWithOnlyRequiredDetails()
        {
            // Either an organisation or school must be provided when creating any Person
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            CreatePersonInternalDTO createPersonDTO = DataGenerator.People.GenerateBasicCreatePersonInternalDTO();

            PersonDTO createdPerson = await PeopleRepository.CreateAsync(createPersonDTO, testOrganisation.Id, null);

            Assert.IsTrue(await PeopleRepository.SaveChangesAsync());

            PersonResources.AssertEqual(createPersonDTO, createdPerson);
            PersonResources.AssertExists(createdPerson, DbIdentifier);
        }

        [Test]
        public async Task CanCreatePersonWithNoAddress()
        {
            // Either an organisation or school must be provided when creating any Person
            Organisation testOrganisation = OrganisationResources.CreateTestOrganisation(DbIdentifier);

            CreatePersonInternalDTO createPersonInternalDTO =
                DataGenerator.People.GenerateCreatePersonInternalDTO(false);

            Assert.IsNull(createPersonInternalDTO.Address);

            PersonDTO createdPerson = await PeopleRepository.CreateAsync(
                createPersonInternalDTO, testOrganisation.Id, null);

            Assert.IsTrue(await PeopleRepository.SaveChangesAsync());

            PersonResources.AssertEqual(createPersonInternalDTO, createdPerson);
            PersonResources.AssertExists(createdPerson, DbIdentifier);
        }
    }
}
