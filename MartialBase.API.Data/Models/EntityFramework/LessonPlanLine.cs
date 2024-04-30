// <copyright file="LessonPlanLine.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartialBase.API.Data.Models.EntityFramework
{
    public class LessonPlanLine
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid LessonPlanId { get; set; }

        [Required]
        [ForeignKey("LessonPlanId")]
        public LessonPlan LessonPlan { get; set; }

        [Required]
        [MaxLength(50)]
        public string PlanLineDetails { get; set; }

        public int DurationInMinutes { get; set; }
    }
}
