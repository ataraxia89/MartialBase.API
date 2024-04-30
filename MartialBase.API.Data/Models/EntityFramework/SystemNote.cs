// <copyright file="SystemNote.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MartialBase.API.Data.Models.EntityFramework
{
    public class SystemNote
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Details { get; set; }

        public Guid? EventId { get; set; }

        [ForeignKey("EventId")]
        public Event Event { get; set; }

        public Guid? LessonId { get; set; }

        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; }

        public Guid? LessonPlanId { get; set; }

        [ForeignKey("LessonPlanId")]
        public LessonPlan LessonPlan { get; set; }

        public Guid? OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        public Guid? OrderLineDeliveryId { get; set; }

        [ForeignKey("OrderLineDeliveryId")]
        public OrderLineDelivery OrderLineDelivery { get; set; }

        public Guid? OrderLineId { get; set; }

        [ForeignKey("OrderLineId")]
        public OrderLine OrderLine { get; set; }

        public Guid? PaymentId { get; set; }

        [ForeignKey("PaymentId")]
        public Payment Payment { get; set; }

        public Guid? ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
