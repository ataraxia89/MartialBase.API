// <copyright file="FindTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.PeopleRepositoryTests
{
    public class FindTests : BaseTestClass
    {
        private const string TestPersonEmail = "test.user@martial.tech";
        private const string TestPersonFirstName = "Jan";
        private const string TestPersonMiddleName = "Michael";
        private const string TestPersonLastName = "Vincent";

        [Test]
        public async Task FindThrowsArgumentNullExceptionWhenNoParametersAreProvided() => Assert.That(async () => await PeopleRepository.FindAsync(), Throws.ArgumentException);

        [TestCase(TestPersonEmail, null, null, null)]
        [TestCase(null, TestPersonFirstName, null, null)]
        [TestCase(null, null, TestPersonMiddleName, null)]
        [TestCase(null, null, null, TestPersonLastName)]
        [TestCase(TestPersonEmail, TestPersonFirstName, null, null)]
        [TestCase(TestPersonEmail, null, TestPersonMiddleName, null)]
        [TestCase(TestPersonEmail, null, null, TestPersonLastName)]
        [TestCase(null, TestPersonFirstName, TestPersonMiddleName, null)]
        [TestCase(null, TestPersonFirstName, null, TestPersonLastName)]
        [TestCase(null, null, TestPersonMiddleName, TestPersonLastName)]
        [TestCase(TestPersonEmail, TestPersonFirstName, TestPersonMiddleName, null)]
        [TestCase(TestPersonEmail, TestPersonFirstName, null, TestPersonLastName)]
        [TestCase(TestPersonEmail, null, TestPersonMiddleName, TestPersonLastName)]
        [TestCase(null, TestPersonFirstName, TestPersonMiddleName, TestPersonLastName)]
        [TestCase(TestPersonEmail, TestPersonFirstName, TestPersonMiddleName, TestPersonLastName)]
        public async Task CanFindPeople(string email, string firstName, string middleName, string lastName)
        {
            Person templatePerson = DataGenerator.People.GeneratePersonObject(true);
            templatePerson.Email = TestPersonEmail;
            templatePerson.FirstName = TestPersonFirstName;
            templatePerson.MiddleName = TestPersonMiddleName;
            templatePerson.LastName = TestPersonLastName;

            List<Person> testPeople = PersonResources.CreateTestPeople(templatePerson, 10, DbIdentifier, true);

            // Ensure there are other people on the database too as a control measure.
            PersonResources.CreateTestPeople(10, DbIdentifier, true);

            List<PersonDTO> retrievedPeople =
                await PeopleRepository.FindAsync(email, firstName, middleName, lastName, true);

            PersonResources.AssertEqual(testPeople, retrievedPeople);
        }

        [Test]
        public async Task CanFindPeopleWithNoAddresses()
        {
            Person templatePerson = DataGenerator.People.GeneratePersonObject(true);
            templatePerson.Email = TestPersonEmail;
            templatePerson.FirstName = TestPersonFirstName;
            templatePerson.MiddleName = TestPersonMiddleName;
            templatePerson.LastName = TestPersonLastName;

            List<Person> testPeople = PersonResources.CreateTestPeople(templatePerson, 10, DbIdentifier, true);

            // Ensure there are other people on the database too as a control measure.
            PersonResources.CreateTestPeople(10, DbIdentifier, true);

            foreach (Person testPerson in testPeople)
            {
                testPerson.AddressId = null;
                testPerson.Address = null;
            }

            List<PersonDTO> retrievedPeople =
                await PeopleRepository.FindAsync(
                        TestPersonEmail, TestPersonFirstName, TestPersonMiddleName, TestPersonLastName, false);

            PersonResources.AssertEqual(testPeople, retrievedPeople);
        }

        [Test]
        public async Task FindPeopleWithNoAddressParameterSpecifiedReturnsNoAddresses()
        {
            Person templatePerson = DataGenerator.People.GeneratePersonObject(true);
            templatePerson.Email = TestPersonEmail;
            templatePerson.FirstName = TestPersonFirstName;
            templatePerson.MiddleName = TestPersonMiddleName;
            templatePerson.LastName = TestPersonLastName;

            List<Person> testPeople = PersonResources.CreateTestPeople(templatePerson, 10, DbIdentifier, true);

            // Ensure there are other people on the database too as a control measure.
            PersonResources.CreateTestPeople(10, DbIdentifier, true);

            foreach (Person testPerson in testPeople)
            {
                testPerson.AddressId = null;
                testPerson.Address = null;
            }

            List<PersonDTO> retrievedPeople =
                await PeopleRepository.FindAsync(
                    TestPersonEmail, TestPersonFirstName, TestPersonMiddleName, TestPersonLastName);

            PersonResources.AssertEqual(testPeople, retrievedPeople);
        }

        [Test]
        public async Task FindPeopleWithNoMatchingDataReturnsEmptyList()
        {
            List<PersonDTO> retrievedPeople = await PeopleRepository.FindAsync("test.user@martial.tech");

            Assert.Zero(retrievedPeople.Count);
        }
    }
}
