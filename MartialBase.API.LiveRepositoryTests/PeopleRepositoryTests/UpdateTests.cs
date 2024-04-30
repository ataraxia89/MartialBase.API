// <copyright file="UpdateTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.PeopleRepositoryTests
{
    public class UpdateTests : BaseTestClass
    {
        [Test]
        public async Task CanUpdatePersonWithAddress()
        {
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, true);
            UpdatePersonDTO updatePersonDTO = DataGenerator.People.GenerateUpdatePersonDTO(true);

            PersonResources.AssertNotEqual(testPerson, updatePersonDTO);

            PersonDTO updatedPerson = await PeopleRepository.UpdateAsync(testPerson.Id, updatePersonDTO);

            Assert.IsTrue(await PeopleRepository.SaveChangesAsync());

            PersonResources.AssertEqual(updatePersonDTO, updatedPerson);
            PersonResources.AssertExists(updatedPerson, DbIdentifier);
        }

        [Test]
        public async Task CanUpdatePersonWithoutAddress()
        {
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);
            UpdatePersonDTO updatePersonDTO = DataGenerator.People.GenerateUpdatePersonDTO(false);

            PersonResources.AssertNotEqual(testPerson, updatePersonDTO);

            PersonDTO updatedPerson = await PeopleRepository.UpdateAsync(testPerson.Id, updatePersonDTO);

            Assert.IsTrue(await PeopleRepository.SaveChangesAsync());

            PersonResources.AssertEqual(updatePersonDTO, updatedPerson);
            PersonResources.AssertExists(updatedPerson, DbIdentifier);
        }

        [Test]
        public async Task CanUpdatePersonAndAddAddress()
        {
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);
            UpdatePersonDTO updatePersonDTO = DataGenerator.People.GenerateUpdatePersonDTO(true);

            PersonResources.AssertNotEqual(testPerson, updatePersonDTO);

            PersonDTO updatedPerson = await PeopleRepository.UpdateAsync(testPerson.Id, updatePersonDTO);

            Assert.IsTrue(await PeopleRepository.SaveChangesAsync());

            PersonResources.AssertEqual(updatePersonDTO, updatedPerson);
            PersonResources.AssertExists(updatedPerson, DbIdentifier);
        }

        [Test]
        public async Task CanUpdatePersonAndRemoveAddress()
        {
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, true);
            UpdatePersonDTO updatePersonDTO = DataGenerator.People.GenerateUpdatePersonDTO(false);

            PersonResources.AssertNotEqual(testPerson, updatePersonDTO);

            PersonDTO updatedPerson = await PeopleRepository.UpdateAsync(testPerson.Id, updatePersonDTO);

            Assert.IsTrue(await PeopleRepository.SaveChangesAsync());

            PersonResources.AssertEqual(updatePersonDTO, updatedPerson, false);
            PersonResources.AssertExists(updatedPerson, DbIdentifier);
            Assert.False(AddressResources.CheckExists(testPerson.Address.Id, DbIdentifier));
        }
    }
}
