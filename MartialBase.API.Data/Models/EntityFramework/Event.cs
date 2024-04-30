// <copyright file="Event.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartialBase.API.Data.Models.EntityFramework
{
    public class Event
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid EventTypeId { get; set; }

        [Required]
        [ForeignKey("EventTypeId")]
        public EventType EventType { get; set; }

        /// <summary>
        /// <para>This property is nullable due to cascade delete rules.</para>
        /// <para>If a school is deleted this field will be set as null in the database,
        /// rather than deleting the event.</para>
        /// </summary>
        public Guid? HostingSchoolId { get; set; }

        /// <summary>
        /// <para>This property is nullable due to cascade delete rules.</para>
        /// <para>If a school is deleted this field will be set as null in the database,
        /// rather than deleting the event.</para>
        /// </summary>
        [ForeignKey("HostingSchoolId")]
        public School HostingSchool { get; set; }

        [Required]
        public Guid AddressId { get; set; }

        [Required]
        [ForeignKey("AddressId")]
        public Address Address { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime FinishDateTime { get; set; }

        [Required]
        [MaxLength(20)]
        public string Name { get; set; }

        public SystemNote Notes { get; set; }
    }
}
