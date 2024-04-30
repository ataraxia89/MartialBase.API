// <copyright file="GetTests.cs" company="Martialtech®">
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
    public class GetTests : BaseTestClass
    {
        [Test]
        public async Task CanGetPersonWithAddress()
        {
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, true);

            PersonDTO retrievedPerson = await PeopleRepository.GetAsync(testPerson.Id);

            PersonResources.AssertEqual(testPerson, retrievedPerson);
        }

        [Test]
        public async Task CanGetPersonWithoutAddress()
        {
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            PersonDTO retrievedPerson = await PeopleRepository.GetAsync(testPerson.Id);

            PersonResources.AssertEqual(testPerson, retrievedPerson);
        }
    }
}
