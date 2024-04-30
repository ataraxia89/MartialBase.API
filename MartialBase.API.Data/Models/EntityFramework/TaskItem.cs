// <copyright file="TaskItem.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartialBase.API.Data.Models.EntityFramework
{
    public class TaskItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid TaskId { get; set; }

        [Required]
        [ForeignKey("TaskId")]
        public Task Task { get; set; }

        public Guid? AssignedToId { get; set; }

        /// <summary>
        /// This can represent currently assigned to or completed by.
        /// </summary>
        [ForeignKey("AssignedToId")]
        public Person AssignedTo { get; set; }

        /// <summary>
        /// 1st item, 2nd item, etc.
        /// </summary>
        [Required]
        public short ItemOrder { get; set; }

        public DateTime CompletionDateTime { get; set; }

        [Required]
        [MaxLength(100)]
        public string Description { get; set; }

        [Required]
        public bool IsCompletionTaskItem { get; set; }
    }
}
