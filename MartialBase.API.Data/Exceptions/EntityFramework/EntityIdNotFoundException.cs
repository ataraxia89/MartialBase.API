// <copyright file="EntityIdNotFoundException.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

namespace MartialBase.API.Data.Exceptions.EntityFramework
{
    public class EntityIdNotFoundException : EntityNotFoundException
    {
        public EntityIdNotFoundException(string entityName, Guid entityId)
            : this(entityName, entityId.ToString())
        {
        }

        public EntityIdNotFoundException(string entityName, string entityId)
            : base($"{entityName} ID '{entityId}' not found.")
        {
        }
    }
}