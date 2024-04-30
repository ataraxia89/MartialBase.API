// <copyright file="Task.cs" company="Martialtech®">
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
    public class Task
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// <para>This property is nullable due to cascade delete rules.</para>
        /// <para>If a person is deleted this field will be set as null in the database,
        /// rather than deleting the task.</para>
        /// </summary>
        [Required]
        public Guid CreatedById { get; set; }

        /// <summary>
        /// <para>This property is nullable due to cascade delete rules.</para>
        /// <para>If a person is deleted this field will be set as null in the database,
        /// rather than deleting the task.</para>
        /// </summary>
        [Required]
        [ForeignKey("CreatedById")]
        public Person CreatedBy { get; set; }

        public Guid? AssignedToId { get; set; }

        [ForeignKey("AssignedToId")]
        public Person AssignedTo { get; set; }

        [Required]
        [MaxLength(250)]
        public string Description { get; set; }

        public ICollection<TaskItem> TaskItems { get; set; }
    }
}
