// <copyright file="MartialBaseUser.cs" company="Martialtech®">
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
    public class MartialBaseUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid PersonId { get; set; }

        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        public Guid? AzureId { get; set; }

        public string InvitationCode { get; set; }

        public ICollection<MartialBaseUserRole> UserRoles { get; set; }
    }
}
