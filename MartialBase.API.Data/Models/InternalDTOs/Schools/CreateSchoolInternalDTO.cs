// <copyright file="CreateSchoolInternalDTO.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

using MartialBase.API.Models.DTOs.Schools;

namespace MartialBase.API.Data.Models.InternalDTOs.Schools
{
    public class CreateSchoolInternalDTO : CreateSchoolDTO
    {
        public new Guid ArtId { get; set; }

        public new Guid OrganisationId { get; set; }

        public new Guid? HeadInstructorId { get; set; }
    }
}