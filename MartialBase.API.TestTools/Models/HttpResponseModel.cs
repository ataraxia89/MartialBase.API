// <copyright file="HttpResponseModel.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using MartialBase.API.Models.Enums;

namespace MartialBase.API.TestTools.Models
{
    public class HttpResponseModel
    {
        public HttpResponseModel(HttpResponseMessage response)
        {
            string responseBody = response.Content.ReadAsStringAsync().Result.Trim();

            if (Enum.TryParse(responseBody, out ErrorResponseCode errorResponseCode))
            {
                responseBody = null;
            }
            else
            {
                errorResponseCode = ErrorResponseCode.None;

                if (string.IsNullOrEmpty(responseBody))
                {
                    responseBody = null;
                }
            }

            StatusCode = response.StatusCode;
            ErrorResponseCode = errorResponseCode;
            ResponseHeaders = response.Headers;
            ResponseBody = responseBody;
        }

        public HttpStatusCode StatusCode { get; set; }

        public ErrorResponseCode ErrorResponseCode { get; set; }

        public HttpResponseHeaders ResponseHeaders { get; set; }

        public string ResponseBody { get; set; }

        public bool IsSuccess => (int)StatusCode >= 200 && (int)StatusCode < 300;
    }
}
