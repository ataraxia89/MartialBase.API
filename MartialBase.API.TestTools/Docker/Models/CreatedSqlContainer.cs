// <copyright file="CreatedSqlContainer.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using DotNet.Testcontainers.Networks;

using Testcontainers.MsSql;

namespace MartialBase.API.TestTools.Docker.Models
{
    public class CreatedSqlContainer
    {
        public CreatedSqlContainer(
            MsSqlContainer sqlContainer,
            string connectionString,
            INetwork network,
            string networkAlias)
        {
            SqlContainer = sqlContainer;
            ConnectionString = connectionString;
            Network = network;
            NetworkAlias = networkAlias;
        }

        public MsSqlContainer SqlContainer { get; }

        public string ConnectionString { get; }

        public INetwork Network { get; }

        public string NetworkAlias { get; }
    }
}
