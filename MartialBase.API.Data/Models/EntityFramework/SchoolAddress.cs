// <copyright file="SchoolAddress.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartialBase.API.Data.Models.EntityFramework
{
    public class SchoolAddress
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// <para>This property is nullable due to cascade delete rules.</para>
        /// <para>If a school is deleted this field will be set as null in the database,
        /// rather than deleting the school address and subsequently address.</para>
        /// </summary>
        public Guid? SchoolId { get; set; }

        /// <summary>
        /// <para>This property is nullable due to cascade delete rules.</para>
        /// <para>If a school is deleted this field will be set as null in the database,
        /// rather than deleting the school address and subsequently address.</para>
        /// </summary>
        [ForeignKey("SchoolId")]
        public School School { get; set; }

        [Required]
        public Guid AddressId { get; set; }

        [Required]
        [ForeignKey("AddressId")]
        public Address Address { get; set; }
    }
}
