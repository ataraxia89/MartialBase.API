// <copyright file="OrphanSchoolEntityException.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

namespace MartialBase.API.Data.Exceptions.EntityFramework
{
    public class OrphanSchoolEntityException : EntityFrameworkException
    {
        public OrphanSchoolEntityException()
            : base("School belongs to organisation, please move the school to another " +
                                                    "organisation or delete the school completely.")
        {
        }
    }
}