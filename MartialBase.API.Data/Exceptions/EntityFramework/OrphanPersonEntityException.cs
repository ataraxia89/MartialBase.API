// <copyright file="OrphanPersonEntityException.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

namespace MartialBase.API.Data.Exceptions.EntityFramework
{
    public class OrphanPersonEntityException : EntityFrameworkException
    {
        public OrphanPersonEntityException()
            : base("Person belongs to just one organisation, please add the person to another " +
                                              "organisation before removing them from this one or delete the person completely.")
        {
        }
    }
}