// <copyright file="GetAllTests.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Arts;
using MartialBase.API.TestTools.TestResources;

using NUnit.Framework;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.LiveRepositoryTests.ArtsRepositoryTests
{
    public class GetAllTests : BaseTestClass
    {
        [Test]
        public async Task CanGetAllArts()
        {
            List<Art> defaultArts = ArtResources.GetAllArts(DbIdentifier);

            List<ArtDTO> retrievedArts = await ArtsRepository.GetAllAsync();

            Assert.NotZero(defaultArts.Count);
            ArtResources.AssertEqual(defaultArts, retrievedArts);
        }
    }
}
