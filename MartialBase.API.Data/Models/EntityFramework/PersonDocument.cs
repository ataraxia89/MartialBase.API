// <copyright file="PersonDocument.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartialBase.API.Data.Models.EntityFramework
{
    public class PersonDocument
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid PersonId { get; set; }

        [Required]
        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        [Required]
        public Guid DocumentId { get; set; }

        [Required]
        [ForeignKey("DocumentId")]
        public Document Document { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}
