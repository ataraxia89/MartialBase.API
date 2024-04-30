// <copyright file="ArtResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.Arts;
using MartialBase.API.Tools;

namespace MartialBase.API.DataGenerator
{
    public static class Arts
    {
        public static Art GenerateArtObject() => new ()
        {
            Id = Guid.NewGuid(),
            Name = RandomData.GetRandomString(45)
        };

        public static ArtDTO GenerateArtDTO() => new ()
        {
            Id = Guid.NewGuid().ToString(),
            Name = RandomData.GetRandomString(45)
        };
    }
}
