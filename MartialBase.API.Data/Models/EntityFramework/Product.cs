// <copyright file="Product.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MartialBase.API.Data.Models.EntityFramework
{
    public class Product
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid ArtId { get; set; }

        [Required]
        [ForeignKey("ArtId")]
        public Art Art { get; set; }

        [Required]
        public Guid ProductCategoryId { get; set; }

        [Required]
        [ForeignKey("ProductCategoryId")]
        public ProductCategory ProductCategory { get; set; }

        [MaxLength(20)]
        public string ProductRef { get; set; }

        [Required]
        [MaxLength(40)]
        public string Description { get; set; }

        /// <summary>
        /// <para>Entity Framework cannot handle having both a single ProductPrice object
        /// and a collection of ProductPrice objects (price history), because both will
        /// use ProductPriceId so relationships cannot be properly defined.</para>
        /// <para>However, the ProductPrice object has an IsCurrentPrice flag, which can
        /// be used to return the ProductPrice item in the collection that represents the
        /// current price. If no item in the collection is marked as IsCurrentPrice, this
        /// will return null.</para>
        /// </summary>
        [NotMapped]
        public ProductPrice CurrentPrice => ProductPrices.FirstOrDefault(pp => pp.IsCurrentPrice);

        [Required]
        public bool IsService { get; set; } // TODO: Are more product types needed apart from goods and services? Tax/import/courier charges?

        public SystemNote Notes { get; set; }

        public ICollection<OrderLine> OrderLinesPlaced { get; set; }

        public ICollection<ProductPrice> ProductPrices { get; set; }
    }
}
