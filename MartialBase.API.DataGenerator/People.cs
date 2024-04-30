// <copyright file="PersonResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.People;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Tools;

namespace MartialBase.API.DataGenerator
{
    public static class People
    {
        public static Person GeneratePersonObject(bool includeAddress = true, bool realisticData = false)
        {
            Address? address = includeAddress ? Addresses.GenerateAddressObject(realisticData) : null;
            string fullName =
                $"{RandomData.GetRandomString(15)} " +
                $"{RandomData.GetRandomString(15)} " +
                $"{RandomData.GetRandomString(35)} " +
                $"{RandomData.GetRandomString(25)}";

            var person = new Person
            {
                Id = Guid.NewGuid(),
                Title = realisticData ? FakeData.Name.Prefix() : RandomData.GetRandomString(15),
                DoB = RandomData.GetRandomDate(DateTime.Now.AddYears(-50), DateTime.Now.AddYears(-10)),
                AddressId = address?.Id,
                Address = address,
                MobileNo = realisticData ? FakeData.Phone.Number() : RandomData.GetRandomString(30),
                Email = realisticData ? FakeData.Internet.Email() : RandomData.GetRandomString(45)
            };

            if (realisticData)
            {
                int rnd = RandomData.GetRandomNumber(0, 1);

                fullName = rnd == 0
                    ? FakeData.Name.FullNameMale(FakeData.NameFormats.StandardWithMiddleWithPrefix)
                    : FakeData.Name.FullNameFemale(FakeData.NameFormats.StandardWithMiddleWithPrefix);

                SetPersonName(person, fullName, true);
            }
            else
            {
                SetPersonName(person, fullName, true);
            }

            return person;
        }

        public static PersonDTO GeneratePersonDTO(bool realisticData = false) =>
            ModelMapper.GetPersonDTO(GeneratePersonObject(realisticData));

        public static CreatePersonDTO GenerateCreatePersonDTOObject(
            Person? person = null,
            bool includeAddress = true,
            bool realisticData = false) => ModelMapper.GetCreatePersonDTO(
            person ?? GeneratePersonObject(realisticData, includeAddress));

        public static CreatePersonDTO GenerateCreatePersonDTO(bool includeAddress = true, bool realisticData = false) =>
            ModelMapper.GetCreatePersonDTO(GeneratePersonObject(realisticData, includeAddress));

        public static CreatePersonInternalDTO GenerateCreatePersonInternalDTO(bool includeAddress = true, bool realisticData = false) =>
            ModelMapper.GetCreatePersonInternalDTO(GeneratePersonObject(includeAddress, realisticData));

        public static CreatePersonInternalDTO GenerateBasicCreatePersonInternalDTO(bool realisticData = false) => new ()
        {
            FirstName = realisticData ? FakeData.Name.First() : RandomData.GetRandomString(15),
            LastName = realisticData ? FakeData.Name.Last() : RandomData.GetRandomString(25)
        };

        public static UpdatePersonDTO GenerateUpdatePersonDTO(bool includeAddress, bool realisticData = false) =>
            ModelMapper.GetUpdatePersonDTO(GeneratePersonObject(includeAddress, realisticData));

        public static void SetPersonName(Person person, string fullName, bool includesTitle = false)
        {
            if (person == null)
            {
                throw new ArgumentNullException(nameof(person));
            }

            if (fullName == null)
            {
                throw new ArgumentNullException(nameof(fullName));
            }

            string[] personNames = fullName.Split(' ');

            if (includesTitle)
            {
                person.Title = personNames[0];

                personNames = fullName.Replace(person.Title, string.Empty).Trim().Split(' ');
            }

            switch (personNames.Length)
            {
                case 1:
                    person.FirstName = personNames[0];
                    person.MiddleName = null;
                    person.LastName = null;
                    break;
                case 2:
                    person.FirstName = personNames[0];
                    person.MiddleName = null;
                    person.LastName = personNames[1];
                    break;
                default:
                    person.FirstName = personNames[0];
                    person.MiddleName = string.Join(' ', personNames.Skip(1).Take(personNames.Length - 2));
                    person.LastName = personNames[^1];
                    break;
            }
        }

        public static void SetPersonName(CreatePersonDTO person, string fullName, bool includesTitle = false)
        {
            if (person == null)
            {
                throw new ArgumentNullException(nameof(person));
            }

            if (fullName == null)
            {
                throw new ArgumentNullException(nameof(fullName));
            }

            string[] personNames = fullName.Split(' ');

            if (includesTitle)
            {
                person.Title = personNames[0];

                personNames = fullName.Replace(person.Title, string.Empty).Trim().Split(' ');
            }

            switch (personNames.Length)
            {
                case 1:
                    person.FirstName = personNames[0];
                    person.MiddleName = null;
                    person.LastName = null;
                    break;
                case 2:
                    person.FirstName = personNames[0];
                    person.MiddleName = null;
                    person.LastName = personNames[1];
                    break;
                default:
                    person.FirstName = personNames[0];
                    person.MiddleName = string.Join(' ', personNames.Skip(1).Take(personNames.Length - 2));
                    person.LastName = personNames[^1];
                    break;
            }
        }
    }
}
