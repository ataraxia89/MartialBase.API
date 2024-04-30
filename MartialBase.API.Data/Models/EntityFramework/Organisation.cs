// <copyright file="Organisation.cs" company="Martialtech®">
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
    public class Organisation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(8)]
        public string Initials { get; set; }

        [Required]
        [MaxLength(60)]
        public string Name { get; set; }

        public Guid? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public Organisation Parent { get; set; }

        /// <summary>
        /// At the moment an organisation can only have one bank account, however because
        /// organisations are tiered (parent-child), bank accounts can be retrieved for
        /// all parents and children of any given organisation. This should be sufficient
        /// for multi-tier organisations as a single organisation should not need
        /// multiple bank accounts.
        /// </summary>
        public BankAccount BankAccount { get; set; }

        public Guid? AddressId { get; set; }

        [ForeignKey("AddressId")]
        public Address Address { get; set; }

        public bool IsPublic { get; set; }

        public ICollection<ArtGrade> ArtGrades { get; set; }

        public ICollection<DocumentType> DocumentTypes { get; set; }

        public ICollection<School> Schools { get; set; }

        public ICollection<OrganisationPerson> People { get; set; }
    }
}
