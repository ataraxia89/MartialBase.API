// <copyright file="DocumentsRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace MartialBase.API.Data.Repositories
{
    /// <summary>
    /// A repository class to manage access to <see cref="Document">Documents</see> held on the database.
    /// </summary>
    public class DocumentsRepository : IDocumentsRepository
    {
        private readonly MartialBaseDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentsRepository"/> class.
        /// </summary>
        /// <param name="context">The <see cref="MartialBaseDbContext"/> in use.</param>
        public DocumentsRepository(MartialBaseDbContext context) => _context = context;

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(Guid id) => await _context.Documents.AnyAsync(d => d.Id == id);
    }
}
