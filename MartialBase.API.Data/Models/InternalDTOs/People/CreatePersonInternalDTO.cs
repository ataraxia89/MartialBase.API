// <copyright file="CreatePersonInternalDTO.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

using MartialBase.API.Models.DTOs.People;

namespace MartialBase.API.Data.Models.InternalDTOs.People
{
    public class CreatePersonInternalDTO : CreatePersonDTO
    {
        public new DateTime? DateOfBirth { get; set; }
    }
}