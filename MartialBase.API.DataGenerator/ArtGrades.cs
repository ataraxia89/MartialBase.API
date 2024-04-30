// <copyright file="ArtGradeResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.ArtGrades;
using MartialBase.API.Models.DTOs.ArtGrades;
using MartialBase.API.Tools;

namespace MartialBase.API.DataGenerator
{
    public static class ArtGrades
    {
        public static ArtGrade GenerateArtGradeObject(Art? art = null, Organisation? organisation = null, bool realisticData = false)
        {
            art ??= Arts.GenerateArtObject();

            organisation ??= Organisations.GenerateOrganisationObject(realisticData: realisticData);

            return new ArtGrade
            {
                Id = Guid.NewGuid(),
                ArtId = art.Id,
                Art = art,
                OrganisationId = organisation.Id,
                Organisation = organisation,
                GradeLevel = RandomData.GetRandomNumber(),
                Description = RandomData.GetRandomString(20)
            };
        }

        public static List<ArtGradeDTO> GenerateArtGradeDTOs(int numberToGenerate)
        {
            var artGrades = new List<ArtGradeDTO>();

            for (int i = 0; i < numberToGenerate; i++)
            {
                artGrades.Add(GenerateArtGradeDTO());
            }

            return artGrades;
        }

        public static ArtGradeDTO GenerateArtGradeDTO() => new ()
        {
            Id = Guid.NewGuid().ToString(),
            ArtId = Guid.NewGuid().ToString(),
            Art = RandomData.GetRandomString(45),
            OrganisationId = Guid.NewGuid().ToString(),
            Organisation = RandomData.GetRandomString(8),
            GradeLevel = RandomData.GetRandomNumber(),
            Description = RandomData.GetRandomString(20)
        };

        public static CreateArtGradeDTO GenerateCreateArtGradeDTOObject(Guid artId, Guid organisationId) => new ()
        {
            ArtId = artId.ToString(),
            OrganisationId = organisationId.ToString(),
            GradeLevel = RandomData.GetRandomNumber(),
            Description = RandomData.GetRandomString(20)
        };

        public static CreateArtGradeInternalDTO GenerateCreateArtGradeInternalDTOObject(Guid artId, Guid organisationId) => new ()
        {
            ArtId = artId,
            OrganisationId = organisationId,
            GradeLevel = RandomData.GetRandomNumber(),
            Description = RandomData.GetRandomString(20)
        };

        public static UpdateArtGradeDTO GenerateUpdateArtGradeDTOObject() => new ()
        {
            GradeLevel = RandomData.GetRandomNumber(),
            Description = RandomData.GetRandomString(20)
        };
    }
}
