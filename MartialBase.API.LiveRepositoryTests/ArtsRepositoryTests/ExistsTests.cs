// <copyright file="ExistsTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.ArtsRepositoryTests
{
    public class ExistsTests : BaseTestClass
    {
        [Test]
        public async Task CanCheckArtExists()
        {
            Art art = ArtResources.CreateTestArt(DbIdentifier);

            Assert.IsTrue(await ArtsRepository.ExistsAsync(art.Id));
        }
    }
}
