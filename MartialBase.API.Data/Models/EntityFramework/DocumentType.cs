// <copyright file="DocumentType.cs" company="Martialtech®">
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
    public class DocumentType
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid OrganisationId { get; set; }

        [Required]
        [ForeignKey("OrganisationId")]
        public Organisation Organisation { get; set; }

        [MaxLength(50)]
        public string ReferenceNo { get; set; }

        [Required]
        [MaxLength(50)]
        public string Description { get; set; }

        public int? DefaultExpiryDays { get; set; }

        [MaxLength(250)]
        public string URL { get; set; }

        public ICollection<Document> Documents { get; set; }
    }
}
