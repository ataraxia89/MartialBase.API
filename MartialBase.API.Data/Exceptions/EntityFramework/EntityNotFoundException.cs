// <copyright file="EntityNotFoundException.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

namespace MartialBase.API.Data.Exceptions.EntityFramework
{
    public class EntityNotFoundException : EntityFrameworkException
    {
        public EntityNotFoundException(string message)
            : base(message)
        {
        }
    }
}