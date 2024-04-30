// <copyright file="MartialBaseControllerBase.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Net;

using MartialBase.API.Data.Exceptions;
using MartialBase.API.Models.Enums;

using Microsoft.AspNetCore.Mvc;

namespace MartialBase.API.Controllers
{
    /// <summary>
    /// Base class to be used by all MartialBase controllers.
    /// </summary>
    public class MartialBaseControllerBase : Controller
    {
#pragma warning disable SA1401 // Fields should be private
        /// <summary>
        /// The current host environment.
        /// </summary>
        internal IWebHostEnvironment HostEnvironment;
#pragma warning restore SA1401 // Fields should be private

        private Guid _requestingPersonId = Guid.Empty;
        private string[] _requestingUserRoles = Array.Empty<string>();

        public Guid RequestingPersonId
        {
            get
            {
                if (_requestingPersonId == Guid.Empty)
                {
                    _requestingPersonId = new Guid(Request.Headers["RequestingPersonId"].ToString());
                }

                return _requestingPersonId;
            }
        }

        public string[] RequestingUserRoles
        {
            get
            {
                if (_requestingUserRoles != Array.Empty<string>())
                {
                    return _requestingUserRoles;
                }

                if (Request.Headers.All(rh => rh.Key != "RequestingUserRoles"))
                {
                    throw new ErrorResponseCodeException(
                        ErrorResponseCode.InsufficientUserRole,
                        HttpStatusCode.Unauthorized);
                }

                _requestingUserRoles = Request.Headers["RequestingUserRoles"];

                return _requestingUserRoles;
            }
        }
    }
}
