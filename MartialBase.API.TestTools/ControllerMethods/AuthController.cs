// <copyright file="AuthController.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Net.Http;
using System.Threading.Tasks;

using MartialBase.API.TestTools.Http;
using MartialBase.API.TestTools.Models;

namespace MartialBase.API.TestTools.ControllerMethods
{
    public static class AuthController
    {
        public static async Task BlockUserAccessAsync(HttpClient client, string userId) =>
            await HttpClientMethods.DeleteAsync(
                client,
                $"login{(userId != null ? $"?userId={userId}" : string.Empty)}");

        public static async Task<HttpResponseModel> BlockUserAccessResponseAsync(HttpClient client, string userId) =>
            await HttpClientMethods.DeleteResponseAsync(
                client,
                $"login{(userId != null ? $"?userId={userId}" : string.Empty)}");
    }
}
