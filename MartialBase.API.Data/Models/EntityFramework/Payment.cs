// <copyright file="Payment.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartialBase.API.Data.Models.EntityFramework
{
    public class Payment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public Guid BankAccountId { get; set; }

        [Required]
        [ForeignKey("BankAccountId")]
        public BankAccount BankAccount { get; set; }

        [MaxLength(20)]
        public string Reference { get; set; }

        [Required]
        public decimal PaymentAmount { get; set; }

        [Required]
        public bool IsPaymentConfirmed { get; set; }

        public SystemNote Notes { get; set; }
    }
}
