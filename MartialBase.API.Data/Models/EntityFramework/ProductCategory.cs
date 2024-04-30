// <copyright file="ProductCategory.cs" company="Martialtech®">
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
    public class ProductCategory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public ProductCategory Parent { get; set; }

        [Required]
        [MaxLength(60)]
        public string Description { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
