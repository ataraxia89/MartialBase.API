// <copyright file="Utilities.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.IO;
using System.Reflection;

using Microsoft.Extensions.Configuration;

namespace MartialBase.API.Data
{
    public static class Utilities
    {
        public static IConfiguration GetConfiguration(string appSettingsFileName)
        {
            // Get the MartialBase.API.Data assembly containing the required JSON file
            var dataAssembly = Assembly.LoadFrom(
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "MartialBase.API.Data.dll"));

            Stream jsonStream =
                dataAssembly.GetManifestResourceStream($"MartialBase.API.Data.{appSettingsFileName}");

            // Set configuration using above app settings file
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonStream(jsonStream)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .Build();

            return configuration;
        }
    }
}
