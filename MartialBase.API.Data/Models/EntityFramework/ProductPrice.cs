// <copyright file="ProductPrice.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartialBase.API.Data.Models.EntityFramework
{
    public class ProductPrice
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public DateTime PriceDate { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Required]
        public decimal InstructorPrice { get; set; }

        [Required]
        public decimal StudentPrice { get; set; }

        [Required]
        public bool IsCurrentPrice { get; set; }
    }
}
