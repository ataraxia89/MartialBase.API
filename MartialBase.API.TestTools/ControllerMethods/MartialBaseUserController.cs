// <copyright file="MartialBaseUserController.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using MartialBase.API.Models.DTOs.MartialBaseUsers;
using MartialBase.API.Models.DTOs.UserRoles;
using MartialBase.API.TestTools.Http;
using MartialBase.API.TestTools.Models;

namespace MartialBase.API.TestTools.ControllerMethods
{
    public class MartialBaseUserController
    {
        public static async Task<string> GetInvitationCodeAsync(HttpClient client, string userId) =>
            await HttpClientMethods<string>.GetAsync(client, $"admin/users/{userId}/invitationcode");

        public static async Task<HttpResponseModel> GetInvitationCodeResponseAsync(HttpClient client, string userId) =>
            await HttpClientMethods.GetResponseAsync(
                client,
                $"admin/users/{userId}/invitationcode");

        public static async Task<List<MartialBaseUserDTO>> GetUsersAsync(HttpClient client) =>
            await HttpClientMethods<List<MartialBaseUserDTO>>.GetAsync(client, "admin/users");

        public static async Task<HttpResponseModel> GetUsersResponseAsync(HttpClient client) =>
            await HttpClientMethods.GetResponseAsync(
                client,
                "admin/users");

        public static async Task<MartialBaseUserDTO> GetUserAsync(HttpClient client, string userId, string includePerson) =>
            await HttpClientMethods<MartialBaseUserDTO>.GetAsync(
                client,
                $"admin/users/{userId}{(includePerson != null ? $"?includePerson={includePerson}" : string.Empty)}");

        public static async Task<HttpResponseModel> GetUserResponseAsync(
            HttpClient client,
            string userId,
            string includePerson) => await HttpClientMethods.GetResponseAsync(
            client,
            $"admin/users/{userId}{(includePerson != null ? $"?includePerson={includePerson}" : string.Empty)}");

        public static async Task<List<UserRoleDTO>> GetRolesAsync(HttpClient client) =>
            await HttpClientMethods<List<UserRoleDTO>>.GetAsync(client, "admin/roles");

        public static async Task<HttpResponseModel> GetRolesResponseAsync(HttpClient client) =>
            await HttpClientMethods.GetResponseAsync(
                client,
                "admin/roles");

        public static async Task<List<UserRoleDTO>> GetRolesForUserAsync(HttpClient client, string userId) =>
            await HttpClientMethods<List<UserRoleDTO>>.GetAsync(client, $"admin/users/{userId}/roles");

        public static async Task<HttpResponseModel> GetRolesForUserResponseAsync(HttpClient client, string userId) =>
            await HttpClientMethods.GetResponseAsync(
                client,
                $"admin/users/{userId}/roles");

#if !RELEASE
        public static async Task<MartialBaseUserDTO> CreateUserAsync(
            HttpClient client,
            CreateMartialBaseUserDTO createMartialBaseUserDTO) =>
            await HttpClientMethods<MartialBaseUserDTO>.PostAsync(client, "admin/users", createMartialBaseUserDTO);

        public static async Task<HttpResponseModel> GetCreateUserResponseAsync(
            HttpClient client,
            CreateMartialBaseUserDTO createMartialBaseUserDTO) => await HttpClientMethods.PostResponseAsync(
            client,
            "admin/users",
            createMartialBaseUserDTO);
#endif

        public static async Task<bool> AddRoleToUserAsync(HttpClient client, string userId, string roleId) =>
            await HttpClientMethods<bool>.PostAsync(
                client,
                $"admin/users/{userId}/roles{(roleId != null ? $"?roleId={roleId}" : string.Empty)}",
                null);

        public static async Task<HttpResponseModel> AddRoleToUserResponseAsync(
            HttpClient client,
            string userId,
            string roleId) => await HttpClientMethods.PostResponseAsync(
            client,
            $"admin/users/{userId}/roles{(roleId != null ? $"?roleId={roleId}" : string.Empty)}",
            null);

        public static async Task<bool> RemoveRoleFromUserAsync(HttpClient client, string userId, string roleId) =>
            await HttpClientMethods.DeleteAsync(client, $"admin/users/{userId}/roles/{roleId}");

        public static async Task<HttpResponseModel> RemoveRoleFromUserResponseAsync(
            HttpClient client,
            string userId,
            string roleId) => await HttpClientMethods.DeleteResponseAsync(
            client,
            $"admin/users/{userId}/roles/{roleId}");

        public static async Task<bool> SetUserRolesAsync(HttpClient client, string userId, IEnumerable<Guid> userRoleIds) =>
            await HttpClientMethods<bool>.PutAsync(
                client,
                $"admin/users/{userId}/roles",
                userRoleIds);

        public static async Task<HttpResponseModel> SetUserRolesResponseAsync(
            HttpClient client,
            string userId,
            IEnumerable<Guid> userRoleIds) => await HttpClientMethods.PutResponseAsync(
            client,
            $"admin/users/{userId}/roles",
            userRoleIds);
    }
}
