// <copyright file="Country.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace MartialBase.API.Data.Models.EntityFramework
{
    public class Country
    {
        [MaxLength(3)]
        public string Code { get; set; }

        [MaxLength(36)]
        public string Name { get; set; }

        public long Population { get; set; }
    }
}