// <copyright file="GetTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Arts;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.ArtsRepositoryTests
{
    public class GetTests : BaseTestClass
    {
        [Test]
        public async Task CanGetArt()
        {
            Art art = ArtResources.CreateTestArt(DbIdentifier);

            ArtDTO retrievedArt = await ArtsRepository.GetAsync(art.Id);

            ArtResources.AssertEqual(art, retrievedArt);
        }
    }
}
