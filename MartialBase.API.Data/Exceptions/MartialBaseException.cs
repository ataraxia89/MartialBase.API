// <copyright file="MartialBaseException.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

namespace MartialBase.API.Data.Exceptions
{
    public class MartialBaseException : Exception
    {
        public MartialBaseException(string message)
            : base(message)
        {
        }

        public MartialBaseException()
        {
        }
    }
}
