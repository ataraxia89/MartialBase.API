// <copyright file="CreateDocumentTypeInternalDTO.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

using MartialBase.API.Models.DTOs.DocumentTypes;

namespace MartialBase.API.Data.Models.InternalDTOs.DocumentTypes
{
    public class CreateDocumentTypeInternalDTO : CreateDocumentTypeDTO
    {
        public new Guid OrganisationId { get; set; }
    }
}