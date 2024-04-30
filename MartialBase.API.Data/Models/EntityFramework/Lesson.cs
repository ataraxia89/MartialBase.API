// <copyright file="Lesson.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartialBase.API.Data.Models.EntityFramework
{
    public class Lesson
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// <para>This property is nullable due to cascade delete rules.</para>
        /// <para>If a school is deleted this field will be set as null in the database,
        /// rather than deleting the lesson.</para>
        /// </summary>
        public Guid? SchoolId { get; set; }

        /// <summary>
        /// <para>This property is nullable due to cascade delete rules.</para>
        /// <para>If a school is deleted this field will be set as null in the database,
        /// rather than deleting the lesson.</para>
        /// </summary>
        [ForeignKey("SchoolId")]
        public School School { get; set; }

        [Required]
        public Guid AddressId { get; set; }

        [Required]
        [ForeignKey("AddressId")]
        public Address Address { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime FinishDateTime { get; set; }

        /// <summary>
        /// <para>This property is nullable due to cascade delete rules.</para>
        /// <para>If a lesson plan is deleted this field will be set as null in the database,
        /// rather than deleting the lesson.</para>
        /// </summary>
        public Guid? LessonPlanId { get; set; }

        /// <summary>
        /// <para>This property is nullable due to cascade delete rules.</para>
        /// <para>If a lesson plan is deleted this field will be set as null in the database,
        /// rather than deleting the lesson.</para>
        /// </summary>
        [ForeignKey("LessonPlanId")]
        public LessonPlan LessonPlan { get; set; }

        public SystemNote Notes { get; set; }

        /// <summary>
        /// <para>This property is nullable due to cascade delete rules.</para>
        /// <para>If a person is deleted this field will be set as null in the database,
        /// rather than deleting the lesson.</para>
        /// </summary>
        public Guid? LessonInstructorId { get; set; }

        /// <summary>
        /// <para>This property is nullable due to cascade delete rules.</para>
        /// <para>If a person is deleted this field will be set as null in the database,
        /// rather than deleting the lesson.</para>
        /// </summary>
        [ForeignKey("LessonInstructorId")]
        public Person LessonInstructor { get; set; }
    }
}
