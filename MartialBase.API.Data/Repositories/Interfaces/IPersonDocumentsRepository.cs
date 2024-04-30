// <copyright file="IPersonDocumentsRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.Documents;
using MartialBase.API.Models.DTOs.Documents;

namespace MartialBase.API.Data.Repositories.Interfaces
{
    /// <summary>
    /// An interface to manage access to <see cref="Document">Documents</see> relating to <see cref="Person">People</see> held on the database.
    /// </summary>
    public interface IPersonDocumentsRepository
    {
        /// <summary>
        /// Creates a <see cref="Document"/> and assigns it to a <see cref="Person"/>.
        /// </summary>
        /// <param name="personId">The ID of the <see cref="Person"/> to assign the <see cref="Document"/> to.</param>
        /// <param name="createDTO">The <see cref="CreateDocumentInternalDTO"/> object used to create the <see cref="Document"/>.</param>
        /// <returns>A <see cref="DocumentDTO"/> object representing the newly-created <see cref="Document"/>.</returns>
        Task<DocumentDTO> CreatePersonDocumentAsync(Guid personId, CreateDocumentInternalDTO createDTO);

        /// <summary>
        /// Gets a <see cref="DocumentDTO">Document</see> relating to a <see cref="Person"/>.
        /// </summary>
        /// <param name="personId">The ID of the <see cref="Person"/> to retrieve the <see cref="DocumentDTO">Document</see> for.</param>
        /// <param name="documentId">The ID of the <see cref="DocumentDTO">Document</see> to be retrieved.</param>
        /// <returns>A <see cref="DocumentDTO"/> object relating to the requested <see cref="Person"/> ID.</returns>
        Task<DocumentDTO> GetPersonDocumentAsync(Guid personId, Guid documentId);

        /// <summary>
        /// Gets a list of all <see cref="DocumentDTO">Documents</see> relating to a provided <see cref="Person"/> ID.
        /// </summary>
        /// <param name="personId">The ID of the <see cref="Person"/> to retrieve <see cref="DocumentDTO">Documents</see> for.</param>
        /// <param name="includeInactive">Indicates whether to include inactive documents or not.</param>
        /// <returns>A list of all <see cref="DocumentDTO"/> objects relating to the requested <see cref="Person"/> ID.</returns>
        Task<List<DocumentDTO>> GetPersonDocumentsAsync(Guid personId, bool includeInactive);
    }
}
