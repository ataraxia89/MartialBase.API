// <copyright file="GetPersonNameFromIdTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.PeopleRepositoryTests
{
    public class GetPersonNameFromIdTests : BaseTestClass
    {
        [Test]
        public async Task CanGetPersonNameFromId()
        {
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            Assert.AreEqual(
                $"{testPerson.FirstName} {testPerson.LastName}",
                await PeopleRepository.GetPersonNameFromIdAsync(testPerson.Id));
        }
    }
}
