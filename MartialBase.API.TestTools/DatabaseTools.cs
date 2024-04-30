// <copyright file="DatabaseTools.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using MartialBase.API.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MartialBase.API.TestTools
{
    public static class DatabaseTools
    {
        private static readonly Dictionary<string, (TestDatabaseContext TestDatabaseContext, bool IsSqlite)> Contexts = new ();

        public static MartialBaseDbContext GetMartialBaseDbContext(string dbIdentifier, bool ensureSegregated = true)
        {
            if (!Contexts.ContainsKey(dbIdentifier))
            {
                throw new InvalidOperationException($"Database identifier '{dbIdentifier}' not registered.");
            }

            if (!ensureSegregated && !Contexts[dbIdentifier].TestDatabaseContext.Context.IsDisposed)
            {
                return Contexts[dbIdentifier].TestDatabaseContext.Context;
            }

            return BuildMartialBaseDbContextObject(Contexts[dbIdentifier].TestDatabaseContext.Configuration, Contexts[dbIdentifier].IsSqlite);
        }

        public static TestDatabaseContext RegisterMartialBaseDbContext(string dbIdentifier, IConfiguration configuration)
        {
            var connString = configuration.GetConnectionString("SQLConnection");
            var isSqlite = connString.Contains(
                "Data Source=",
                StringComparison.InvariantCultureIgnoreCase);

            if (Contexts.ContainsKey(dbIdentifier))
            {
                return Contexts[dbIdentifier].TestDatabaseContext;
            }

            if (isSqlite)
            {
                configuration["ConnectionStrings:SQLConnection"] = $"Data Source={dbIdentifier}.db;Cache=Shared";
            }

            MartialBaseDbContext context = BuildMartialBaseDbContextObject(configuration, isSqlite);

            context.Initialize(isSqlite);

            var testDatabaseContext = new TestDatabaseContext(configuration, context);

            Contexts.Add(dbIdentifier, (testDatabaseContext, isSqlite));

            return testDatabaseContext;
        }

        public static TestDatabaseContext RegisterMartialBaseDbContext(string dbIdentifier, IConfiguration configuration, string connectionString)
        {
            var isSqlite = connectionString.Contains(
                "Data Source=",
                StringComparison.InvariantCultureIgnoreCase);

            if (Contexts.ContainsKey(dbIdentifier))
            {
                return Contexts[dbIdentifier].TestDatabaseContext;
            }

            configuration["ConnectionStrings:SQLConnection"] = connectionString;

            MartialBaseDbContext context = BuildMartialBaseDbContextObject(configuration, isSqlite);

            context.Initialize(isSqlite);

            var testDatabaseContext = new TestDatabaseContext(configuration, context);

            Contexts.Add(dbIdentifier, (testDatabaseContext, isSqlite));

            return testDatabaseContext;
        }

        public static bool DeleteDatabase(string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = GetMartialBaseDbContext(dbIdentifier, false))
            {
                dbContext.Database.EnsureDeleted();
            }

            Contexts.Remove(dbIdentifier);

            return true;
        }

        private static MartialBaseDbContext BuildMartialBaseDbContextObject(IConfiguration configuration, bool isSqlite)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MartialBaseDbContext>();

            if (isSqlite)
            {
                optionsBuilder.UseSqlite(configuration.GetConnectionString("SQLConnection"));
            }
            else
            {
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("SQLConnection"));
            }

            return new MartialBaseDbContext(optionsBuilder.Options);
        }
    }
}
