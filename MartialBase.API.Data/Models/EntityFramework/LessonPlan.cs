// <copyright file="LessonPlan.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartialBase.API.Data.Models.EntityFramework
{
    public class LessonPlan
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid ArtId { get; set; }

        [Required]
        [ForeignKey("ArtId")]
        public Art Art { get; set; }

        [Required]
        [MaxLength(20)]
        public string Name { get; set; }

        [Required]
        public Guid CreatedById { get; set; }

        [Required]
        [ForeignKey("CreatedById")]
        public Person CreatedBy { get; set; }

        public SystemNote Notes { get; set; }

        public ICollection<Lesson> Lessons { get; set; }

        public ICollection<LessonPlanLine> LessonPlanLines { get; set; }
    }
}
