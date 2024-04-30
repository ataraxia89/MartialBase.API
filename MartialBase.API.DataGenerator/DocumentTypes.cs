// <copyright file="DocumentTypeResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.DocumentTypes;
using MartialBase.API.Models.DTOs.DocumentTypes;
using MartialBase.API.Tools;

namespace MartialBase.API.DataGenerator
{
    public static class DocumentTypes
    {
        public static DocumentType GenerateDocumentTypeObject(bool realisticData = false)
        {
            Organisation organisation = Organisations.GenerateOrganisationObject(realisticData: realisticData);

            return new DocumentType
            {
                Id = Guid.NewGuid(),
                OrganisationId = organisation.Id,
                Organisation = organisation,
                ReferenceNo = RandomData.GetRandomString(50),
                Description = RandomData.GetRandomString(50),
                DefaultExpiryDays = RandomData.GetRandomNumber(),
                URL = RandomData.GetRandomString(250)
            };
        }

        public static List<DocumentTypeDTO> GenerateDocumentTypeDTOs(int numberToGenerate)
        {
            var documentTypes = new List<DocumentTypeDTO>();

            for (int i = 0; i < numberToGenerate; i++)
            {
                documentTypes.Add(GenerateDocumentTypeDTO());
            }

            return documentTypes;
        }

        public static DocumentTypeDTO GenerateDocumentTypeDTO() => new ()
        {
            Id = Guid.NewGuid().ToString(),
            OrganisationId = Guid.NewGuid().ToString(),
            Organisation = RandomData.GetRandomString(8),
            ReferenceNo = RandomData.GetRandomString(50),
            Name = RandomData.GetRandomString(50),
            DocumentExpiryDays = RandomData.GetRandomNumber(0, 365),
            URL = RandomData.GetRandomString(250)
        };

        public static CreateDocumentTypeInternalDTO GenerateCreateDocumentTypeInternalDTOObject(Guid organisationId) => new ()
        {
            OrganisationId = organisationId,
            ReferenceNo = RandomData.GetRandomString(50),
            Name = RandomData.GetRandomString(50),
            DefaultExpiryDays = RandomData.GetRandomNumber(),
            URL = RandomData.GetRandomString(250)
        };

        public static UpdateDocumentTypeDTO GenerateUpdateDocumentTypeDTOObject() => new ()
        {
            ReferenceNo = RandomData.GetRandomString(50),
            Name = RandomData.GetRandomString(50),
            DefaultExpiryDays = RandomData.GetRandomNumber(),
            URL = RandomData.GetRandomString(250)
        };
    }
}
