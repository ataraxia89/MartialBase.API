// <copyright file="OrderLine.cs" company="Martialtech®">
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
    public class OrderLine
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid OrderId { get; set; }

        [Required]
        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        /// <summary>
        /// <para>This property is nullable due to cascade delete rules.</para>
        /// <para>If a product is deleted this field will be set as null in the database,
        /// rather than deleting the order line.</para>
        /// </summary>
        public Guid? ProductId { get; set; }

        /// <summary>
        /// <para>This property is nullable due to cascade delete rules.</para>
        /// <para>If a product is deleted this field will be set as null in the database,
        /// rather than deleting the order line.</para>
        /// </summary>
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        public SystemNote Notes { get; set; }

        /// <summary>
        /// Total price for current order line.
        /// </summary>
        public decimal TotalPrice { get; set; }

        public ICollection<OrderLineDelivery> OrderLineDeliveries { get; set; }
    }
}
