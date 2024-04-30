// <copyright file="DeleteTests.cs" company="Martialtech®">
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
    public class DeleteTests : BaseTestClass
    {
        [Test]
        public async Task CanDeletePersonWithAddress()
        {
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, true);

            await PeopleRepository.DeleteAsync(testPerson.Id);

            Assert.IsTrue(await PeopleRepository.SaveChangesAsync());

            Assert.IsFalse(PersonResources.CheckExists(testPerson.Id, DbIdentifier));
            Assert.IsFalse(AddressResources.CheckExists(testPerson.Address.Id, DbIdentifier));
        }

        [Test]
        public async Task CanDeletePersonWithoutAddress()
        {
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            await PeopleRepository.DeleteAsync(testPerson.Id);

            Assert.IsTrue(await PeopleRepository.SaveChangesAsync());

            Assert.IsFalse(PersonResources.CheckExists(testPerson.Id, DbIdentifier));
        }
    }
}
