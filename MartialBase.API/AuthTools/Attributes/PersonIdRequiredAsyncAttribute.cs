// <copyright file="PersonIdRequiredAsyncAttribute.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;

namespace MartialBase.API.AuthTools.Attributes
{
    public class PersonIdRequiredAsyncAttribute : TypeFilterAttribute
    {
        public PersonIdRequiredAsyncAttribute()
            : base(typeof(PersonIdRequiredAsyncFilter))
        {
        }
    }
}
