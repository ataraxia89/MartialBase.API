// <copyright file="MartialBaseDbContext.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Linq;

using MartialBase.API.Data.Models.EntityFramework;

using Microsoft.EntityFrameworkCore;

namespace MartialBase.API.Data
{
    public class MartialBaseDbContext : DbContext
    {
        public MartialBaseDbContext(DbContextOptions<MartialBaseDbContext> options)
        : base(options)
        {
        }

        public DbSet<Address> Addresses { get; set; }

        public DbSet<Art> Arts { get; set; }

        public DbSet<ArtGrade> ArtGrades { get; set; }

        public DbSet<BankAccount> BankAccounts { get; set; }

        public DbSet<Document> Documents { get; set; }

        public DbSet<DocumentType> DocumentTypes { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<EventType> EventTypes { get; set; }

        public DbSet<Lesson> Lessons { get; set; }

        public DbSet<LessonPlan> LessonPlans { get; set; }

        public DbSet<LessonPlanLine> LessonPlanLines { get; set; }

        public DbSet<MartialBaseUser> MartialBaseUsers { get; set; }

        public DbSet<MartialBaseUserRole> MartialBaseUserRoles { get; set; }

        public DbSet<MedicalCategory> MedicalCategories { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderLineDelivery> OrderLineDeliveries { get; set; }

        public DbSet<OrderLine> OrderLines { get; set; }

        public DbSet<Organisation> Organisations { get; set; }

        public DbSet<OrganisationPerson> OrganisationPeople { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductCategory> ProductCategories { get; set; }

        public DbSet<ProductPrice> ProductPrices { get; set; }

        public DbSet<RecurringLesson> RecurringLessons { get; set; }

        public DbSet<School> Schools { get; set; }

        public DbSet<SchoolStudent> SchoolStudents { get; set; }

        public DbSet<SchoolAddress> SchoolAddresses { get; set; }

        public DbSet<Person> People { get; set; }

        public DbSet<PersonDocument> PersonDocuments { get; set; }

        public DbSet<SystemNote> SystemNotes { get; set; }

        public DbSet<Task> Tasks { get; set; }

        public DbSet<TaskItem> TaskItems { get; set; }

        public DbSet<UserRole> UserRoles { get; set; }

        public bool IsDisposed { get; private set; }

        public override void Dispose()
        {
            IsDisposed = true;

            base.Dispose();
        }

        internal void Initialize(bool isSqlite)
        {
            if (isSqlite)
            {
                Database.EnsureCreated();
            }
            else
            {
                Database.Migrate();
            }

            AddArts();
            AddUserRoles();
        }

        protected static void CreateTables(ModelBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Entity<Address>().ToTable(nameof(Addresses));
            builder.Entity<Art>().ToTable(nameof(Arts));
            builder.Entity<ArtGrade>().ToTable(nameof(ArtGrades));
            builder.Entity<BankAccount>().ToTable(nameof(BankAccounts));
            builder.Entity<Document>().ToTable(nameof(Documents));
            builder.Entity<DocumentType>().ToTable(nameof(DocumentTypes));
            builder.Entity<Event>().ToTable(nameof(Events));
            builder.Entity<EventType>().ToTable(nameof(EventTypes));
            builder.Entity<Lesson>().ToTable(nameof(Lessons));
            builder.Entity<LessonPlan>().ToTable(nameof(LessonPlans));
            builder.Entity<LessonPlanLine>().ToTable(nameof(LessonPlanLines));
            builder.Entity<MartialBaseUser>().ToTable(nameof(MartialBaseUsers));
            builder.Entity<MartialBaseUserRole>().ToTable(nameof(MartialBaseUserRoles));
            builder.Entity<MedicalCategory>().ToTable(nameof(MedicalCategories));
            builder.Entity<Order>().ToTable(nameof(Orders));
            builder.Entity<OrderLineDelivery>().Property(olq => olq.QuantityDelivered).HasColumnType("decimal(6, 2)");
            builder.Entity<OrderLineDelivery>().ToTable(nameof(OrderLineDeliveries));
            builder.Entity<OrderLine>().Property(ol => ol.Quantity).HasColumnType("decimal(6, 2)");
            builder.Entity<OrderLine>().Property(ol => ol.TotalPrice).HasColumnType("decimal(6, 2)");
            builder.Entity<OrderLine>().ToTable(nameof(OrderLines));
            builder.Entity<Organisation>().ToTable(nameof(Organisations));
            builder.Entity<OrganisationPerson>().ToTable(nameof(OrganisationPeople));
            builder.Entity<OrganisationPerson>().Property(op => op.IsOrganisationAdmin).HasDefaultValue(false);
            builder.Entity<Payment>().Property(p => p.PaymentAmount).HasColumnType("decimal(6, 2)");
            builder.Entity<Payment>().ToTable(nameof(Payments));
            builder.Entity<Product>().ToTable(nameof(Products));
            builder.Entity<ProductCategory>().ToTable(nameof(ProductCategories));
            builder.Entity<ProductPrice>().Property(pp => pp.InstructorPrice).HasColumnType("decimal(6, 2)");
            builder.Entity<ProductPrice>().Property(pp => pp.StudentPrice).HasColumnType("decimal(6, 2)");
            builder.Entity<ProductPrice>().ToTable(nameof(ProductPrices));
            builder.Entity<RecurringLesson>().ToTable(nameof(RecurringLessons));
            builder.Entity<School>().ToTable(nameof(Schools));
            builder.Entity<SchoolStudent>().ToTable(nameof(SchoolStudents));
            builder.Entity<SchoolStudent>().Property(ss => ss.IsInstructor).HasDefaultValue(false);
            builder.Entity<SchoolStudent>().Property(ss => ss.IsSecretary).HasDefaultValue(false);
            builder.Entity<SchoolAddress>().ToTable(nameof(SchoolAddresses));
            builder.Entity<Person>().ToTable(nameof(People));
            builder.Entity<PersonDocument>().ToTable(nameof(PersonDocuments));
            builder.Entity<SystemNote>().ToTable(nameof(SystemNotes));
            builder.Entity<Task>().ToTable(nameof(Tasks));
            builder.Entity<TaskItem>().ToTable(nameof(TaskItems));
            builder.Entity<UserRole>().ToTable(nameof(UserRoles));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            CreateTables(modelBuilder);
        }

        private void AddArts()
        {
            if (Arts.Any())
            {
                return;
            }

            foreach (Art art in Collections.Arts.GetArts)
            {
                Arts.Add(art);
            }

            SaveChanges();
        }

        private void AddUserRoles()
        {
            if (UserRoles.Any())
            {
                return;
            }

            foreach (UserRole userRole in Collections.UserRoles.GetRoles)
            {
                UserRoles.Add(userRole);
            }

            SaveChanges();
        }
    }
}
