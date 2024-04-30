// <copyright file="CreateDocumentInternalDTO.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

using MartialBase.API.Models.DTOs.Documents;

namespace MartialBase.API.Data.Models.InternalDTOs.Documents
{
    public class CreateDocumentInternalDTO : CreateDocumentDTO
    {
        public new Guid DocumentTypeId { get; set; }
    }
}