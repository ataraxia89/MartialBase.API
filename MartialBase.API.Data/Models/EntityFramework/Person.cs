// <copyright file="Person.cs" company="Martialtech®">
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
    public class Person
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [MaxLength(15)]
        public string Title { get; set; }

        [Required]
        [MaxLength(15)]
        public string FirstName { get; set; }

        [MaxLength(35)]
        public string MiddleName { get; set; }

        [Required]
        [MaxLength(25)]
        public string LastName { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        public DateTime? DoB { get; set; }

        public Guid? AddressId { get; set; }

        [ForeignKey("AddressId")]
        public Address Address { get; set; }

        [MaxLength(30)]
        public string MobileNo { get; set; }

        [MaxLength(45)]
        public string Email { get; set; }

        public ICollection<SchoolStudent> StudentSchools { get; set; }

        public ICollection<BankAccount> OwnedBankAccounts { get; set; }

        public ICollection<Lesson> InstructedLessons { get; set; }

        public ICollection<LessonPlan> CreatedLessonPlans { get; set; }

        public ICollection<Order> OrdersPlaced { get; set; }

        /// <summary>
        /// Collection of schools for which this person is the head instructor.
        /// </summary>
        public ICollection<School> Schools { get; set; }

        [InverseProperty("CreatedBy")]
        public ICollection<Task> TasksCreated { get; set; }

        [InverseProperty("AssignedTo")]
        public ICollection<Task> TasksAssignedTo { get; set; }

        public ICollection<OrganisationPerson> Organisations { get; set; }
    }
}
