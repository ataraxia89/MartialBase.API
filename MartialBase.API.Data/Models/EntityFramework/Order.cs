// <copyright file="Order.cs" company="Martialtech®">
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
    public class Order
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        public Guid SchoolId { get; set; }

        [ForeignKey("SchoolId")]
        public School School { get; set; }

        public Guid? PersonId { get; set; }

        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        [MaxLength(20)]
        public string ReferenceNo { get; set; }

        public SystemNote Notes { get; set; }

        public ICollection<OrderLine> OrderLines { get; set; }
    }
}
