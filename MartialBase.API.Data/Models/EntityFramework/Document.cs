// <copyright file="Document.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartialBase.API.Data.Models.EntityFramework
{
    public class Document
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid DocumentTypeId { get; set; }

        [Required]
        [ForeignKey("DocumentTypeId")]
        public DocumentType DocumentType { get; set; }

        [Required]
        public DateTime FiledDate { get; set; }

        public DateTime? DocumentDate { get; set; }

        [MaxLength(50)]
        public string DocumentRef { get; set; }

        [MaxLength(250)]
        public string DocumentURL { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }
}
