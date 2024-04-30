// <copyright file="Art.cs" company="Martialtech®">
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
    public class Art
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(45)]
        public string Name { get; set; }

        public ICollection<ArtGrade> ArtGrades { get; set; }

        public ICollection<LessonPlan> LessonPlans { get; set; }

        public ICollection<Product> Products { get; set; }

        public ICollection<School> Schools { get; set; }
    }
}
