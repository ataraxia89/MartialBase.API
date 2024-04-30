// <copyright file="ArtGrade.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartialBase.API.Data.Models.EntityFramework
{
    public class ArtGrade
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid ArtId { get; set; }

        [Required]
        [ForeignKey("ArtId")]
        public Art Art { get; set; }

        [Required]
        public Guid OrganisationId { get; set; }

        [Required]
        [ForeignKey("OrganisationId")]
        public Organisation Organisation { get; set; }

        [Required]
        public int GradeLevel { get; set; }

        [Required]
        [MaxLength(20)]
        public string Description { get; set; }
    }
}
