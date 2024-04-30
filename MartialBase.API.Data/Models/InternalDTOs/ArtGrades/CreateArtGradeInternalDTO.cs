// <copyright file="CreateArtGradeInternalDTO.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

using MartialBase.API.Models.DTOs.ArtGrades;

namespace MartialBase.API.Data.Models.InternalDTOs.ArtGrades
{
    public class CreateArtGradeInternalDTO : CreateArtGradeDTO
    {
        public new Guid ArtId { get; set; }

        public new Guid OrganisationId { get; set; }
    }
}
