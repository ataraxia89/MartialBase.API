// <copyright file="MartialBaseUserResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.MartialBaseUsers;
using MartialBase.API.Tools;

namespace MartialBase.API.DataGenerator
{
    public static class MartialBaseUsers
    {
        public static MartialBaseUser GenerateMartialBaseUserObject(bool azureRegistered = true, bool realisticData = false)
        {
            Person person = People.GeneratePersonObject(realisticData: realisticData);

            return new MartialBaseUser
            {
                Id = Guid.NewGuid(),
                AzureId = azureRegistered ? Guid.NewGuid() : null,
                InvitationCode = RandomData.GetRandomString(7, true, false),
                Person = person,
                PersonId = person.Id
            };
        }

        public static CreateMartialBaseUserDTO GenerateCreateMartialBaseUserDTOObject(Person person = null) =>
            new ()
            {
                AzureId = Guid.NewGuid().ToString(),
                InvitationCode = null,
                PersonId = person?.Id.ToString(), // If PersonId is present then Person already exists
                Person = person == null
                    ? People.GenerateCreatePersonDTO(false)
                    : null // Person is not null, therefore exists, therefore this shouldn't return a create model
            };
    }
}
