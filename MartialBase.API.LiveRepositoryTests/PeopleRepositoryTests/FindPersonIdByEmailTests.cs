// <copyright file="FindPersonIdByEmailTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.PeopleRepositoryTests
{
    public class FindPersonIdByEmailTests : BaseTestClass
    {
        [Test]
        public async Task CanGetPersonIdFromEmail()
        {
            Person testPerson = PersonResources.CreateTestPerson(DbIdentifier, false);

            Guid? retrievedPersonId = await PeopleRepository.FindPersonIdByEmailAsync(testPerson.Email);

            Assert.IsNotNull(retrievedPersonId);
            Assert.AreEqual(testPerson.Id, (Guid)retrievedPersonId);
        }
    }
}
