// <copyright file="SchoolStudent.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartialBase.API.Data.Models.EntityFramework
{
    public class SchoolStudent
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// <para>This property is nullable due to cascade delete rules.</para>
        /// <para>If a school is deleted this field will be set as null in the database,
        /// rather than deleting the art student.</para>
        /// </summary>
        public Guid? SchoolId { get; set; }

        /// <summary>
        /// <para>This property is nullable due to cascade delete rules.</para>
        /// <para>If a school is deleted this field will be set as null in the database,
        /// rather than deleting the art student.</para>
        /// </summary>
        [ForeignKey("SchoolId")]
        public School School { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        [ForeignKey("StudentId")]
        public Person Student { get; set; }

        public Guid? InsuranceDocumentId { get; set; }

        [ForeignKey("InsuranceDocumentId")]
        public Document InsuranceDocument { get; set; }

        public Guid? LicenceDocumentId { get; set; }

        [ForeignKey("LicenceDocumentId")]
        public Document LicenceDocument { get; set; }

        public DateTime? InactiveDate { get; set; }

        [Required]
        public bool IsInstructor { get; set; }

        [Required]
        public bool IsSecretary { get; set; }
    }
}
