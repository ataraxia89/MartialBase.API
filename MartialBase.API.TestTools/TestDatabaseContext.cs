// <copyright file="TestDatabaseContext.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data;

using Microsoft.Extensions.Configuration;

namespace MartialBase.API.TestTools
{
    public class TestDatabaseContext
    {
        public TestDatabaseContext(IConfiguration configuration, MartialBaseDbContext context)
        {
            Configuration = configuration;
            Context = context;
        }

        public IConfiguration Configuration { get; }

        public MartialBaseDbContext Context { get; }
    }
}
