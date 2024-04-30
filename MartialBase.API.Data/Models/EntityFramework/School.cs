// <copyright file="School.cs" company="Martialtech®">
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
    public class School
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
        [MaxLength(60)]
        public string Name { get; set; }

        /// <summary>
        /// <para>This property is nullable due to cascade delete rules.</para>
        /// <para>If a person is deleted this field will be set as null in the database,
        /// rather than deleting the school.</para>
        /// </summary>
        public Guid? HeadInstructorId { get; set; }

        /// <summary>
        /// <para>This property is nullable due to cascade delete rules.</para>
        /// <para>If a person is deleted this field will be set as null in the database,
        /// rather than deleting the school.</para>
        /// </summary>
        [ForeignKey("HeadInstructorId")]
        public Person HeadInstructor { get; set; }

        public Guid DefaultAddressId { get; set; }

        [ForeignKey("DefaultAddressId")]
        public Address DefaultAddress { get; set; }

        [MaxLength(30)]
        public string PhoneNo { get; set; }

        [MaxLength(50)]
        public string Email { get; set; }

        [MaxLength(70)]
        public string Website { get; set; }

        public Guid? BankAccountId { get; set; }

        [ForeignKey("BankAccountId")]
        public BankAccount BankAccount { get; set; }

        public ICollection<SchoolStudent> Students { get; set; }

        public ICollection<Event> Events { get; set; }

        public ICollection<Lesson> Lessons { get; set; }

        public ICollection<Order> OrdersPlaced { get; set; }

        /// <summary>
        /// One school can have many addresses and one address can have many schools.
        /// </summary>
        public ICollection<SchoolAddress> SchoolAddresses { get; set; }
    }
}
