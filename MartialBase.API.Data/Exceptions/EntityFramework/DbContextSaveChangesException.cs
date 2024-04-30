// <copyright file="DbContextSaveChangesException.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

namespace MartialBase.API.Data.Exceptions.EntityFramework
{
    public class DbContextSaveChangesException : EntityFrameworkException
    {
        public DbContextSaveChangesException()
            : base("Failed to save changes to database context.")
        {
        }
    }
}