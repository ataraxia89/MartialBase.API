// <copyright file="MartialBaseUserRole.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartialBase.API.Data.Models.EntityFramework
{
    public class MartialBaseUserRole
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public Guid MartialBaseUserId { get; set; }

        [ForeignKey("MartialBaseUserId")]
        public MartialBaseUser MartialBaseUser { get; set; }

        [Required]
        public Guid UserRoleId { get; set; }

        [ForeignKey("UserRoleId")]
        public UserRole UserRole { get; set; }
    }
}
