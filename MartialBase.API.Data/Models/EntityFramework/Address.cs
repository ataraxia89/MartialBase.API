// <copyright file="Address.cs" company="Martialtech®">
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
    public class Address
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Line1 { get; set; }

        [MaxLength(30)]
        public string Line2 { get; set; }

        [MaxLength(30)]
        public string Line3 { get; set; }

        [Required]
        [MaxLength(80)]
        public string Town { get; set; }

        [Required]
        [MaxLength(30)]
        public string County { get; set; }

        [Required]
        [MaxLength(8)]
        public string PostCode { get; set; }

        [Required]
        [MaxLength(3)]
        public string CountryCode { get; set; }

        [MaxLength(30)]
        public string LandlinePhone { get; set; }

        public ICollection<Event> Events { get; set; }

        public ICollection<Lesson> Lessons { get; set; }

        public ICollection<RecurringLesson> RecurringLessons { get; set; }

        /// <summary>
        /// One school can have many addresses and one address can have many schools.
        /// </summary>
        public ICollection<SchoolAddress> Schools { get; set; }

        public ICollection<Person> People { get; set; }
    }
}
