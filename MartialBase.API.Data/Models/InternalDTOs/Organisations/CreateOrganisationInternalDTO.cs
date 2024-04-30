// <copyright file="CreateOrganisationInternalDTO.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

using MartialBase.API.Models.DTOs.Organisations;

namespace MartialBase.API.Data.Models.InternalDTOs.Organisations
{
    public class CreateOrganisationInternalDTO : CreateOrganisationDTO
    {
        public new Guid? ParentId { get; set; }
    }
}