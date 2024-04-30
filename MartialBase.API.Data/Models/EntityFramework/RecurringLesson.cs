// <copyright file="RecurringLesson.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartialBase.API.Data.Models.EntityFramework
{
    public class RecurringLesson
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid AddressId { get; set; }

        [Required]
        [ForeignKey("AddressId")]
        public Address Address { get; set; }

        [Required]
        public DayOfWeek LessonDayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }
    }
}
