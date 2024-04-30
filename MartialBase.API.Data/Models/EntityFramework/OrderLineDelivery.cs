// <copyright file="OrderLineDelivery.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartialBase.API.Data.Models.EntityFramework
{
    public class OrderLineDelivery
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public DateTime DeliveryDateTime { get; set; }

        [Required]
        public Guid OrderLineId { get; set; }

        [Required]
        [ForeignKey("OrderLineId")]
        public OrderLine OrderLine { get; set; }

        [Required]
        public decimal QuantityDelivered { get; set; }

        public SystemNote Notes { get; set; }
    }
}
