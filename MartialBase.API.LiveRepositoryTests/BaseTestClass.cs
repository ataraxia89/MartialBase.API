// <copyright file="BaseTestClass.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveRepositoryTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;

using MartialBase.API.Data.Repositories;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.TestTools;
using MartialBase.API.TestTools.TestResources;

using Microsoft.Extensions.Configuration;

using NUnit.Framework;

namespace MartialBase.API.LiveRepositoryTests
{
    public class BaseTestClass
    {
        public string DbIdentifier { get; set; }

        public IAddressesRepository AddressesRepository { get; private set; }

        public IArtsRepository ArtsRepository { get; private set; }

        public IArtGradesRepository ArtGradesRepository { get; private set; }

        public ICountriesRepository CountriesRepository { get; private set; }

        public IDocumentsRepository DocumentsRepository { get; private set; }

        public IDocumentTypesRepository DocumentTypesRepository { get; private set; }

        public IMartialBaseUserRolesRepository MartialBaseUserRolesRepository { get; private set; }

        public IMartialBaseUsersRepository MartialBaseUsersRepository { get; private set; }

        public IOrganisationsRepository OrganisationsRepository { get; private set; }

        public IPeopleRepository PeopleRepository { get; private set; }

        public ISchoolsRepository SchoolsRepository { get; private set; }

        public IUserRolesRepository UserRolesRepository { get; private set; }

        [SetUp]
        public void Setup()
        {
            DbIdentifier = Guid.NewGuid().ToString();

            IConfiguration configuration = Data.Utilities.GetConfiguration("appsettings.Debug.json");

            TestDatabaseContext testContext = DatabaseTools.RegisterMartialBaseDbContext(DbIdentifier, configuration);

            AddressesRepository = new AddressesRepository(testContext.Context);
            ArtsRepository = new ArtsRepository(testContext.Context);
            ArtGradesRepository = new ArtGradesRepository(testContext.Context);
            CountriesRepository = new CountriesRepository();
            DocumentsRepository = new DocumentsRepository(testContext.Context);
            DocumentTypesRepository = new DocumentTypesRepository(testContext.Context);
            MartialBaseUserRolesRepository = new MartialBaseUserRolesRepository(testContext.Context);
            MartialBaseUsersRepository = new MartialBaseUsersRepository(testContext.Context);
            OrganisationsRepository = new OrganisationsRepository(testContext.Context);
            PeopleRepository = new PeopleRepository(testContext.Context);
            SchoolsRepository = new SchoolsRepository(testContext.Context);
            UserRolesRepository = new UserRolesRepository(testContext.Context);

            CountryResources.ClearUsedCountries();
        }

        [TearDown]
        public void TearDown() => DatabaseTools.DeleteDatabase(DbIdentifier);
    }
}
