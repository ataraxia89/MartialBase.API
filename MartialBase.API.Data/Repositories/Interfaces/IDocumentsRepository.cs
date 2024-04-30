// <copyright file="IDocumentsRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

using MartialBase.API.Data.Models.EntityFramework;

namespace MartialBase.API.Data.Repositories.Interfaces
{
    /// <summary>
    /// An interface to manage access to <see cref="Document">Documents</see> held on the database.
    /// </summary>
    public interface IDocumentsRepository
    {
        /// <summary>
        /// Checks whether a <see cref="Document"/> exists matching a provided ID.
        /// </summary>
        /// <param name="id">The ID of the <see cref="Document"/> to check.</param>
        /// <returns>True if a <see cref="Document"/> is found matching the provided ID.</returns>
        Task<bool> ExistsAsync(Guid id);
    }
}
