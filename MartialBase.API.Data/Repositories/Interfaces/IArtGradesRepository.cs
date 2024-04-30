// <copyright file="IArtGradesRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.ArtGrades;
using MartialBase.API.Models.DTOs.ArtGrades;

namespace MartialBase.API.Data.Repositories.Interfaces
{
    /// <summary>
    /// Interface for the <see cref="ArtGrade"/> repository.
    /// </summary>
    public interface IArtGradesRepository : IRepository<ArtGradeDTO, CreateArtGradeInternalDTO, UpdateArtGradeDTO>
    {
        Task<List<ArtGradeDTO>> GetAllAsync(Guid artId, Guid organisationId);

        Task<Guid> GetArtGradeOrganisationIdAsync(Guid artGradeId);
    }
}
