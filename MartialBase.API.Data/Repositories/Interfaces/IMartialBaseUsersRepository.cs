// <copyright file="IMartialBaseUsersRepository.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.MartialBaseUsers;

using Task = System.Threading.Tasks.Task;

namespace MartialBase.API.Data.Repositories.Interfaces
{
    /// <summary>
    /// Interface for the <see cref="MartialBaseUser"/> repository.
    /// </summary>
    /// <remarks>
    /// There is no facility to re-assign an Azure user ID for security reasons, this is a rare occurrence and
    /// therefore should be done manually by MartialBase admins. There is also no facility to create new invitation
    /// codes for existing users, again for security reasons. The only scenario where this would need to be used is
    /// if the subject user needed to change the account they sign in with. Opening up the ability to issue new
    /// invitation codes is the same security risk as re-assigning Azure user IDs.
    /// </remarks>
    public interface IMartialBaseUsersRepository : IRepository<MartialBaseUserDTO, CreateMartialBaseUserDTO, UpdateMartialBaseUserDTO>
    {
        /// <summary>
        /// Retrieves the ID of the <see cref="MartialBaseUser"/> record associated with the provided
        /// <see cref="Person"/> ID.
        /// </summary>
        /// <param name="personId">The ID of the <see cref="Person"/>.</param>
        /// <returns>
        /// The ID of the <see cref="MartialBaseUser"/> record associated with the provided <see cref="Person"/> ID.
        /// </returns>
        Task<Guid> GetUserIdForPersonAsync(Guid personId);

        /// <summary>
        /// Retrieves the ID of the <see cref="Person"/> record associated with the provided
        /// <see cref="MartialBaseUser"/> user ID.
        /// </summary>
        /// <param name="userId">The user ID of the <see cref="MartialBaseUser"/>.</param>
        /// <returns>
        /// The ID of the <see cref="Person"/> record associated with the provided <see cref="MartialBaseUser"/>
        /// user ID.
        /// </returns>
        Task<Guid?> GetPersonIdForUserAsync(Guid userId);

        /// <summary>
        /// Generates an invitation code for a MartialBase user to associate their details with an Azure user.
        /// </summary>
        /// <param name="userId">The user ID of the <see cref="MartialBaseUser"/> to generate an invitation code for.</param>
        /// <returns>An invitation code string.</returns>
        Task<string> GenerateInvitationCodeAsync(Guid userId);

        /// <summary>
        /// Retrieves the ID of the <see cref="Person"/> record associated with the provided Azure user ID.
        /// </summary>
        /// <param name="azureUserId">The Azure ID relating to the relevant <see cref="MartialBaseUser"/>.</param>
        /// <param name="invitationCode">The invitation code of the Azure user.</param>
        /// <returns>The ID of the <see cref="Person"/> record associated with the provided Azure user ID.</returns>
        Task<Guid?> GetPersonIdForAzureUserAsync(Guid azureUserId, string invitationCode);

        /// <summary>
        /// Retrieves the Azure ID of the <see cref="MartialBaseUser"/> record associated with the provided ID.
        /// </summary>
        /// <param name="martialBaseUserId">The ID of the <see cref="MartialBaseUser"/> to retrieve the Azure ID for.</param>
        /// <returns>The Azure ID of the <see cref="MartialBaseUser"/> record associated with the provided ID.</returns>
        Task<Guid?> GetAzureIdForUserAsync(Guid martialBaseUserId);

        /// <summary>
        /// Removes both the Azure ID and the invitation code from the specified <see cref="MartialBaseUser"/>.
        /// </summary>
        /// <param name="userId">The ID of the <see cref="MartialBaseUser"/> to be disassociated.</param>
        Task DisassociateAzureUserAsync(Guid userId);
    }
}
