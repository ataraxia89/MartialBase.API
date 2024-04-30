// <copyright file="ModelMapper.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.ArtGrades;
using MartialBase.API.Data.Models.InternalDTOs.Documents;
using MartialBase.API.Data.Models.InternalDTOs.DocumentTypes;
using MartialBase.API.Data.Models.InternalDTOs.Organisations;
using MartialBase.API.Data.Models.InternalDTOs.People;
using MartialBase.API.Data.Models.InternalDTOs.Schools;
using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.Models.DTOs.ArtGrades;
using MartialBase.API.Models.DTOs.Arts;
using MartialBase.API.Models.DTOs.Countries;
using MartialBase.API.Models.DTOs.Documents;
using MartialBase.API.Models.DTOs.DocumentTypes;
using MartialBase.API.Models.DTOs.MartialBaseUsers;
using MartialBase.API.Models.DTOs.Organisations;
using MartialBase.API.Models.DTOs.People;
using MartialBase.API.Models.DTOs.Products;
using MartialBase.API.Models.DTOs.Schools;
using MartialBase.API.Models.DTOs.UserRoles;

namespace MartialBase.API.Data.Models
{
    public static class ModelMapper
    {
        public static Address GetAddress(CreateAddressDTO createAddressDTO) =>
            createAddressDTO == null
                ? null
                : new Address
                {
                    Id = Guid.NewGuid(),
                    Line1 = createAddressDTO.Line1,
                    Line2 = createAddressDTO.Line2,
                    Line3 = createAddressDTO.Line3,
                    Town = createAddressDTO.Town,
                    County = createAddressDTO.County,
                    PostCode = createAddressDTO.PostCode,
                    CountryCode = createAddressDTO.CountryCode,
                    LandlinePhone = createAddressDTO.LandlinePhone
                };

        public static AddressDTO GetAddressDTO(Address address) =>
            address == null
                ? null
                : new AddressDTO(
                    address.Id.ToString(),
                    address.Line1,
                    address.Line2,
                    address.Line3,
                    address.Town,
                    address.County,
                    address.PostCode,
                    address.CountryCode,
                    Countries.GetCountry(address.CountryCode).Name,
                    address.LandlinePhone);

        public static CreateAddressDTO GetCreateAddressDTO(Address address) =>
            address == null
                ? null
                : new CreateAddressDTO
                {
                    Line1 = address.Line1,
                    Line2 = address.Line2,
                    Line3 = address.Line3,
                    Town = address.Town,
                    County = address.County,
                    PostCode = address.PostCode,
                    CountryCode = address.CountryCode,
                    LandlinePhone = address.LandlinePhone
                };

        public static UpdateAddressDTO GetUpdateAddressDTO(Address address) =>
            address == null
                ? null
                : new UpdateAddressDTO
                {
                    Line1 = address.Line1,
                    Line2 = address.Line2,
                    Line3 = address.Line3,
                    Town = address.Town,
                    County = address.County,
                    PostCode = address.PostCode,
                    CountryCode = address.CountryCode,
                    LandlinePhone = address.LandlinePhone
                };

        public static ArtGrade GetArtGrade(CreateArtGradeInternalDTO createArtGradeDTO) =>
            createArtGradeDTO == null
                ? null
                : new ArtGrade
                {
                    ArtId = createArtGradeDTO.ArtId,
                    OrganisationId = createArtGradeDTO.OrganisationId,
                    GradeLevel = createArtGradeDTO.GradeLevel,
                    Description = createArtGradeDTO.Description
                };

        public static ArtGradeDTO GetArtGradeDTO(ArtGrade artGrade) =>
            artGrade == null ? null : new ArtGradeDTO(
                artGrade.Id.ToString(),
                artGrade.Art.Id.ToString(),
                artGrade.Art.Name,
                artGrade.Organisation.Id.ToString(),
                artGrade.Organisation.Initials ?? artGrade.Organisation.Name,
                artGrade.GradeLevel,
                artGrade.Description);

        public static ArtDTO GetArtDTO(Art art) =>
            art == null ? null : new ArtDTO(art.Id.ToString(), art.Name);

        public static CountryDTO GetCountryDTO(Country country) =>
            country == null ? null : new CountryDTO(country.Code, country.Name);

        public static Document GetDocument(CreateDocumentInternalDTO createDocumentDTO) =>
            createDocumentDTO == null
                ? null
                : new Document
                {
                    Id = Guid.NewGuid(),
                    DocumentTypeId = createDocumentDTO.DocumentTypeId,
                    FiledDate = DateTime.UtcNow,
                    DocumentDate = createDocumentDTO.DocumentDate,
                    DocumentRef = createDocumentDTO.Reference,
                    DocumentURL = createDocumentDTO.URL,
                    ExpiryDate = createDocumentDTO.ExpiryDate
                };

        public static DocumentDTO GetDocumentDTO(Document document) =>
            document == null ? null : new DocumentDTO(
                document.Id.ToString(),
                document.DocumentType.Id.ToString(),
                document.DocumentType.Description,
                document.DocumentType.Organisation.Initials ?? document.DocumentType.Organisation.Name,
                document.DocumentDate,
                document.DocumentRef,
                document.DocumentURL,
                document.ExpiryDate);

        public static DocumentType GetDocumentType(CreateDocumentTypeInternalDTO createDocumentTypeDTO) =>
            createDocumentTypeDTO == null
                ? null
                : new DocumentType
                {
                    Id = Guid.NewGuid(),
                    OrganisationId = createDocumentTypeDTO.OrganisationId,
                    ReferenceNo = createDocumentTypeDTO.ReferenceNo,
                    Description = createDocumentTypeDTO.Name,
                    DefaultExpiryDays = createDocumentTypeDTO.DefaultExpiryDays,
                    URL = createDocumentTypeDTO.URL
                };

        public static DocumentTypeDTO GetDocumentTypeDTO(DocumentType documentType) =>
            documentType == null ? null : new DocumentTypeDTO(
                documentType.Id.ToString(),
                documentType.Organisation.Id.ToString(),
                documentType.Organisation.Initials ?? documentType.Organisation.Name,
                documentType.ReferenceNo,
                documentType.Description,
                documentType.DefaultExpiryDays,
                documentType.URL);

        public static MartialBaseUserDTO GetMartialBaseUserDTO(MartialBaseUser user) =>
            user == null ? null : new MartialBaseUserDTO(
                user.Id.ToString(),
                GetPersonDTO(user.Person),
                user.AzureId.ToString(),
                user.InvitationCode);

        public static Organisation GetOrganisation(CreateOrganisationInternalDTO createOrganisationDTO) =>
            createOrganisationDTO == null
                ? null
                : new Organisation
                {
                    Id = Guid.NewGuid(),
                    Initials = createOrganisationDTO.Initials,
                    Name = createOrganisationDTO.Name,
                    ParentId = createOrganisationDTO.ParentId,
                    Address = GetAddress(createOrganisationDTO.Address)
                };

        public static OrganisationDTO GetOrganisationDTO(Organisation organization) =>
            organization == null ? null : new OrganisationDTO(
                organization.Id.ToString(),
                organization.Initials,
                organization.Name,
                organization.Parent?.Id.ToString(),
                organization.Parent?.Initials,
                GetAddressDTO(organization.Address));

        public static CreateOrganisationDTO GetCreateOrganisationDTO(Organisation organisation) =>
            organisation == null
                ? null
                : new CreateOrganisationDTO
                {
                    Initials = organisation.Initials,
                    Name = organisation.Name,
                    ParentId = organisation.ParentId?.ToString(),
                    Address = GetCreateAddressDTO(organisation.Address)
                };

        public static CreateOrganisationInternalDTO GetCreateOrganisationInternalDTO(Organisation organisation) =>
            organisation == null
                ? null
                : new CreateOrganisationInternalDTO
                {
                    Initials = organisation.Initials,
                    Name = organisation.Name,
                    ParentId = organisation.ParentId,
                    Address = GetCreateAddressDTO(organisation.Address)
                };

        public static Person GetPerson(CreatePersonDTO createPersonDTO)
        {
            if (createPersonDTO == null)
            {
                return null;
            }

            var internalDTO = new CreatePersonInternalDTO
            {
                DateOfBirth = Convert.ToDateTime(createPersonDTO.DateOfBirth),
                Address = createPersonDTO.Address,
                Email = createPersonDTO.Email,
                Title = createPersonDTO.Title,
                FirstName = createPersonDTO.FirstName,
                MiddleName = createPersonDTO.MiddleName,
                LastName = createPersonDTO.LastName,
                MobileNo = createPersonDTO.MobileNo
            };

            return GetPerson(internalDTO);
        }

        public static Person GetPerson(CreatePersonInternalDTO createPersonDTO) =>
            createPersonDTO == null
                ? null
                : new Person
                {
                    Id = Guid.NewGuid(),
                    Title = createPersonDTO.Title,
                    FirstName = createPersonDTO.FirstName,
                    MiddleName = createPersonDTO.MiddleName,
                    LastName = createPersonDTO.LastName,
                    DoB = createPersonDTO.DateOfBirth,
                    Address = GetAddress(createPersonDTO.Address),
                    MobileNo = createPersonDTO.MobileNo,
                    Email = createPersonDTO.Email
                };

        public static PersonDTO GetPersonDTO(Person person) =>
            person == null ? null : new PersonDTO(
                person.Id,
                person.Title,
                person.FirstName,
                person.MiddleName,
                person.LastName,
                person.DoB,
                GetAddressDTO(person.Address),
                person.MobileNo,
                person.Email);

        public static CreatePersonDTO GetCreatePersonDTO(Person person) =>
            person == null
                ? null
                : new CreatePersonDTO
                {
                    Title = person.Title,
                    FirstName = person.FirstName,
                    MiddleName = person.MiddleName,
                    LastName = person.LastName,
                    Address = GetCreateAddressDTO(person.Address),
                    MobileNo = person.MobileNo,
                    Email = person.Email,
                    DateOfBirth = person.DoB?.ToString("yyyy-MM-dd")
                };

        public static CreatePersonInternalDTO GetCreatePersonInternalDTO(Person person) =>
            person == null
                ? null
                : new CreatePersonInternalDTO
                {
                    Title = person.Title,
                    FirstName = person.FirstName,
                    MiddleName = person.MiddleName,
                    LastName = person.LastName,
                    Address = GetCreateAddressDTO(person.Address),
                    MobileNo = person.MobileNo,
                    Email = person.Email,
                    DateOfBirth = person.DoB
                };

        public static UpdatePersonDTO GetUpdatePersonDTO(Person person) =>
            person == null
                ? null
                : new UpdatePersonDTO
                {
                    Title = person.Title,
                    FirstName = person.FirstName,
                    MiddleName = person.MiddleName,
                    LastName = person.LastName,
                    Address = GetUpdateAddressDTO(person.Address),
                    MobileNo = person.MobileNo,
                    Email = person.Email
                };

        public static ProductDTO GetProductDTO(Product product) =>
            product == null ? null : new ProductDTO(
                product.Id.ToString(),
                product.ProductCategory.Description,
                product.ProductRef,
                product.Description,
                default,
                product.Notes.Details);

        public static School GetSchool(CreateSchoolInternalDTO createSchoolDTO) =>
            createSchoolDTO == null
                ? null
                : new School
                {
                    Id = Guid.NewGuid(),
                    ArtId = createSchoolDTO.ArtId,
                    OrganisationId = createSchoolDTO.OrganisationId,
                    Name = createSchoolDTO.Name,
                    HeadInstructorId = createSchoolDTO.HeadInstructorId,
                    DefaultAddress = GetAddress(createSchoolDTO.Address),
                    PhoneNo = createSchoolDTO.PhoneNo,
                    Email = createSchoolDTO.EmailAddress,
                    Website = createSchoolDTO.WebsiteURL
                };

        public static SchoolDTO GetSchoolDTO(School school) =>
            school == null ? null : new SchoolDTO(
                school.Id,
                school.Art.Id.ToString(),
                school.Art.Name,
                school.Organisation.Id.ToString(),
                school.Organisation.Initials ?? school.Organisation.Name,
                school.Name,
                school.HeadInstructor.Id.ToString(),
                school.HeadInstructor.FullName,
                GetAddressDTO(school.DefaultAddress),
                school.PhoneNo,
                school.Email,
                school.Website,
                school.SchoolAddresses.Select(sa => GetAddressDTO(sa.Address)).ToList());

        public static CreateSchoolDTO GetCreateSchoolDTO(School school) =>
            school == null
                ? null
                : new CreateSchoolDTO
                {
                    ArtId = school.ArtId.ToString(),
                    OrganisationId = school.OrganisationId.ToString(),
                    Name = school.Name,
                    PhoneNo = school.PhoneNo,
                    EmailAddress = school.Email,
                    WebsiteURL = school.Website,
                    HeadInstructorId = school.HeadInstructorId?.ToString(),
                    Address = GetCreateAddressDTO(school.DefaultAddress),
                    AdditionalTrainingVenues = school.SchoolAddresses
                        .Select(address => GetCreateAddressDTO(address.Address)).ToList()
                };

        public static CreateSchoolInternalDTO GetCreateSchoolInternalDTO(School school) =>
            school == null
                ? null
                : new CreateSchoolInternalDTO
                {
                    ArtId = school.ArtId,
                    OrganisationId = school.OrganisationId,
                    Name = school.Name,
                    PhoneNo = school.PhoneNo,
                    EmailAddress = school.Email,
                    WebsiteURL = school.Website,
                    HeadInstructorId = school.HeadInstructorId,
                    Address = GetCreateAddressDTO(school.DefaultAddress),
                    AdditionalTrainingVenues = school.SchoolAddresses
                        .Where(sa => sa.AddressId != school.DefaultAddressId)
                        .Select(address => GetCreateAddressDTO(address.Address)).ToList()
                };

        public static UpdateSchoolDTO GetUpdateSchoolDTO(School school)
        {
            if (school == null)
            {
                return null;
            }

            var updateAdditionalAddressDTOs = new Dictionary<string, UpdateAddressDTO>();

            foreach (SchoolAddress schoolAddress in school.SchoolAddresses)
            {
                if (schoolAddress.AddressId != school.DefaultAddressId)
                {
                    updateAdditionalAddressDTOs.Add(
                        schoolAddress.Address.Id.ToString(),
                        GetUpdateAddressDTO(schoolAddress.Address));
                }
            }

            return new UpdateSchoolDTO
            {
                Name = school.Name,
                Address = GetUpdateAddressDTO(school.DefaultAddress),
                PhoneNo = school.PhoneNo,
                EmailAddress = school.Email,
                WebsiteURL = school.Website,
                AdditionalTrainingVenues = updateAdditionalAddressDTOs
            };
        }

        public static UserRoleDTO GetUserRoleDTO(UserRole role) =>
            role == null ? null : new UserRoleDTO(role.Id.ToString(), role.Name);
    }
}
