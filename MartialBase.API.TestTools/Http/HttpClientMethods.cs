// <copyright file="HttpClientMethods.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using MartialBase.API.Models.Enums;
using MartialBase.API.TestTools.Models;

using Newtonsoft.Json;

using Xunit;

namespace MartialBase.API.TestTools.Http
{
#pragma warning disable SA1402 // File may only contain a single type
    public static class HttpClientMethods<T>
#pragma warning restore SA1402 // File may only contain a single type
    {
        public static async Task<T> GetAsync(HttpClient client, string endpoint)
        {
            HttpResponseMessage response = await client.GetAsync(endpoint);

            string responseString = (await response.Content.ReadAsStringAsync()).Trim();

            if (Enum.TryParse(responseString, out ErrorResponseCode errorResponseCode))
            {
                responseString = errorResponseCode.ToString();
            }

            Assert.True(
                response.IsSuccessStatusCode,
                $"HTTP request failed ({response.StatusCode}) with response message '{responseString}'");

            return JsonConvert.DeserializeObject<T>(responseString);
        }

        public static async Task<T> PostAsync(HttpClient client, string endpoint, object model)
        {
            HttpResponseMessage response = await client.PostAsync(
                endpoint,
                model == null
                    ? null
                    : new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));

            string responseString = (await response.Content.ReadAsStringAsync()).Trim();

            if (Enum.TryParse(responseString, out ErrorResponseCode errorResponseCode))
            {
                responseString = errorResponseCode.ToString();
            }

            // Need to use Assert.True instead of Assert.Equal so that the error string will be output
            Assert.True(
                response.StatusCode == HttpStatusCode.Created,
                $"HTTP request failed ({response.StatusCode}) with response message '{responseString}'");

            if (typeof(T) == typeof(bool))
            {
                return JsonConvert.DeserializeObject<T>(response.IsSuccessStatusCode.ToString().ToLower());
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(responseString);
            }
        }

        public static async Task<T> PutAsync(HttpClient client, string endpoint, object model)
        {
            var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PutAsync(endpoint, content);

            string responseString = (await response.Content.ReadAsStringAsync()).Trim();

            if (Enum.TryParse(responseString, out ErrorResponseCode errorResponseCode))
            {
                responseString = errorResponseCode.ToString();
            }

            // Need to use Assert.True instead of Assert.Equal so that the error string will be output
            Assert.True(
                response.StatusCode == HttpStatusCode.OK,
                $"HTTP request failed ({response.StatusCode}) with response message '{responseString}'");

            return JsonConvert.DeserializeObject<T>(responseString);
        }
    }

    public static class HttpClientMethods
    {
        public static async Task<bool> GetAsync(HttpClient client, string endpoint)
        {
            HttpResponseMessage response = await client.GetAsync(endpoint);

            string responseString = (await response.Content.ReadAsStringAsync()).Trim();

            if (Enum.TryParse(responseString, out ErrorResponseCode errorResponseCode))
            {
                responseString = errorResponseCode.ToString();
            }

            Assert.True(
                response.IsSuccessStatusCode,
                $"HTTP request failed ({response.StatusCode}) with response message '{responseString}'");

            return true;
        }

        public static async Task<HttpResponseModel> GetResponseAsync(
            HttpClient client,
            string endpoint) => new (await client.GetAsync(endpoint));

        public static async Task<bool> PostAsync(HttpClient client, string endpoint)
        {
            HttpResponseMessage response = await client.PostAsync(endpoint, null);

            string responseString = (await response.Content.ReadAsStringAsync()).Trim();

            if (Enum.TryParse(responseString, out ErrorResponseCode errorResponseCode))
            {
                responseString = errorResponseCode.ToString();
            }

            // Need to use Assert.True instead of Assert.Equal so that the error string will be output
            Assert.True(
                response.StatusCode == HttpStatusCode.Created,
                $"HTTP request failed ({response.StatusCode}) with response message '{responseString}'");

            return true;
        }

        public static async Task<bool> PostAsync(HttpClient client, string endpoint, object model)
        {
            HttpResponseMessage response = await client.PostAsync(
                                               endpoint,
                                               model == null
                                                   ? null
                                                   : new StringContent(
                                                       JsonConvert.SerializeObject(model),
                                                       Encoding.UTF8,
                                                       "application/json"));

            string responseString = (await response.Content.ReadAsStringAsync()).Trim();

            if (Enum.TryParse(responseString, out ErrorResponseCode errorResponseCode))
            {
                responseString = errorResponseCode.ToString();
            }

            // Need to use Assert.True instead of Assert.Equal so that the error string will be output
            Assert.True(
                response.IsSuccessStatusCode,
                $"HTTP request failed ({response.StatusCode}) with response message '{responseString}'");

            return true;
        }

        public static async Task<HttpResponseModel> PostResponseAsync(
            HttpClient client,
            string endpoint,
            object model) =>
            new (await client.PostAsync(
                    endpoint,
                    model == null
                        ? null
                        : new StringContent(
                            JsonConvert.SerializeObject(model),
                            Encoding.UTF8,
                            "application/json")));

        public static async Task<HttpResponseModel> PostResponseAsync(
            HttpClient client,
            string endpoint) =>
            new (await client.PostAsync(endpoint, null));

        public static async Task<bool> PutAsync(HttpClient client, string endpoint)
        {
            HttpResponseMessage response = await client.PutAsync(endpoint, null);

            string responseString = (await response.Content.ReadAsStringAsync()).Trim();

            if (Enum.TryParse(responseString, out ErrorResponseCode errorResponseCode))
            {
                responseString = errorResponseCode.ToString();
            }

            // Need to use Assert.True instead of Assert.Equal so that the error string will be output
            Assert.True(
                response.StatusCode == HttpStatusCode.OK,
                $"HTTP request failed ({response.StatusCode}) with response message '{responseString}'");

            return true;
        }

        public static async Task<HttpResponseModel> PutResponseAsync(
            HttpClient client,
            string endpoint) => new (await client.PutAsync(endpoint, null));

        public static async Task<HttpResponseModel> PutResponseAsync(
            HttpClient client,
            string endpoint,
            object model) => new (
            await client.PutAsync(
                endpoint,
                new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json")));

        public static async Task<bool> DeleteAsync(HttpClient client, string endpoint)
        {
            HttpResponseMessage response = await client.DeleteAsync(endpoint);

            string responseString = (await response.Content.ReadAsStringAsync()).Trim();

            if (Enum.TryParse(responseString, out ErrorResponseCode errorResponseCode))
            {
                responseString = errorResponseCode.ToString();
            }

            // Need to use Assert.True instead of Assert.Equal so that the error string will be output
            Assert.True(
                response.StatusCode == HttpStatusCode.NoContent,
                $"HTTP request failed ({response.StatusCode}) with response message '{responseString}'");

            return true;
        }

        public static async Task<HttpResponseModel> DeleteResponseAsync(
            HttpClient client,
            string endpoint) => new (await client.DeleteAsync(endpoint));
    }
}
