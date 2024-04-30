// <copyright file="ErrorResponseCodeException.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Net;

using MartialBase.API.Models.Enums;

namespace MartialBase.API.Data.Exceptions
{
    public class ErrorResponseCodeException : MartialBaseException
    {
        public ErrorResponseCodeException(ErrorResponseCode errorCode, HttpStatusCode statusCode)
        {
            ErrorResponseCode = errorCode;
            StatusCode = statusCode;
        }

        public ErrorResponseCode ErrorResponseCode { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }
}
