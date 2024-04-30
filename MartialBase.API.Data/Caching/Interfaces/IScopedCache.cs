// <copyright file="IScopedCache.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MartialBase.API.Data.Caching.Interfaces
{
    public interface IScopedCache
    {
        Task<Guid?> GetOrSetPersonIdForAzureUserAsync(Guid azureUserId, Func<Task<Guid?>> addFunction);

        Task<Guid> GetOrSetUserIdAsync(Guid personId, Func<Task<Guid>> addFunction);

        Task<IEnumerable<string>> GetOrSetUserRolesAsync(Guid personId, Func<Task<IEnumerable<string>>> addFunction);
    }
}
