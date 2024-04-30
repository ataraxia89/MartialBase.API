// <copyright file="ScopedCache.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using MartialBase.API.Data.Caching.Interfaces;

namespace MartialBase.API.Data.Caching
{
    public class ScopedCache : IScopedCache
    {
        private readonly Dictionary<string, string> _cache;

        public ScopedCache()
        {
            _cache = new Dictionary<string, string>();
        }

        public async Task<Guid?> GetOrSetPersonIdForAzureUserAsync(Guid azureUserId, Func<Task<Guid?>> addFunction) =>
            await GetOrSetAsync($"PersonIdForAzureUser-{azureUserId}", addFunction);

        public async Task<Guid> GetOrSetUserIdAsync(Guid personId, Func<Task<Guid>> addFunction) =>
            await GetOrSetAsync($"UserId-{personId}", addFunction);

        public async Task<IEnumerable<string>> GetOrSetUserRolesAsync(Guid personId, Func<Task<IEnumerable<string>>> addFunction) =>
            await GetOrSetAsync($"PersonRoles-{personId}", addFunction);

        private async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> addFunction)
        {
            if (addFunction == null)
            {
                throw new ArgumentNullException(nameof(addFunction));
            }

            if (_cache.TryGetValue(key, out string value))
            {
                return JsonSerializer.Deserialize<T>(value);
            }

            var result = await Task.Run(addFunction);

            if (result != null)
            {
                _cache.Add(key, JsonSerializer.Serialize(result));
            }

            return result;
        }
    }
}
