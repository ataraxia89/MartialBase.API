// <copyright file="BankAccount.cs" company="Martialtech®">
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
    public class BankAccount
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public int SortCode { get; set; }

        [Required]
        public int AccountNo { get; set; }

        [Required]
        [MaxLength(50)]
        public string AccountName { get; set; }

        /// <summary>
        /// For internal MartialBase® use only.
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string AccountDescription { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressLine3 { get; set; }

        public string AddressTown { get; set; }

        public string AddressCounty { get; set; }

        public string AddressCountry { get; set; }

        public string AddressPostCode { get; set; }

        [Required]
        public Guid AccountOwnerId { get; set; }

        [Required]
        [ForeignKey("AccountOwnerId")]
        public Person AccountOwner { get; set; }

        [Required]
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// At the moment a single bank account can only belong to one organisation,
        /// however because organisations are tiered (parent-child), bank accounts can
        /// be retrieved for all parents and children of any given organisation. This
        /// should be sufficient for multi-tier organisations as a single organisation
        /// should not need multiple bank accounts.
        /// </summary>
        [Required]
        [ForeignKey("OrganisationId")]
        public Organisation Organisation { get; set; }

        public ICollection<Payment> Payments { get; set; }

        /// <summary>
        /// A school can only have one bank account, but one bank account can serve
        /// many schools.
        /// </summary>
        public ICollection<School> Schools { get; set; }
    }
}
