// <copyright file="DocumentResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Data.Models.InternalDTOs.Documents;
using MartialBase.API.Models.DTOs.Documents;
using MartialBase.API.Tools;

namespace MartialBase.API.DataGenerator
{
    public static class Documents
    {
        /// <summary>
        /// Generates a specified number of <see cref="DocumentDTO"/> objects.
        /// </summary>
        /// <remarks>This method does not carry out any operations on the database, it is purely for returning objects.</remarks>
        /// <param name="numberToCreate">The number of <see cref="DocumentDTO"/> objects to be generated.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="DocumentDTO"/> objects.</returns>
        public static List<DocumentDTO> GenerateDocumentDTOs(int numberToCreate)
        {
            var documentDTOs = new List<DocumentDTO>();

            if (numberToCreate <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(numberToCreate),
                    numberToCreate,
                    $"Cannot create {numberToCreate} test documents.");
            }

            for (int i = 0; i < numberToCreate; i++)
            {
                documentDTOs.Add(GenerateDocumentDTO());
            }

            return documentDTOs;
        }

        /// <summary>
        /// Generates a <see cref="DocumentDTO"/> object.
        /// </summary>
        /// <remarks>This method does not carry out any operations on the database, it is purely for returning an object.</remarks>
        /// <param name="createDocumentInternalDTO">The <see cref="CreateDocumentInternalDTO"/> to be used as a base for the <see cref="DocumentDTO"/>.</param>
        /// <returns>A <see cref="DocumentDTO"/> object.</returns>
        public static DocumentDTO GenerateDocumentDTO(CreateDocumentInternalDTO? createDocumentInternalDTO = null)
        {
            if (createDocumentInternalDTO != null)
            {
                return new DocumentDTO(
                    Guid.NewGuid().ToString(),
                    createDocumentInternalDTO.DocumentTypeId.ToString(),
                    RandomData.GetRandomString(50),
                    RandomData.GetRandomString(20),
                    createDocumentInternalDTO.DocumentDate,
                    createDocumentInternalDTO.Reference,
                    createDocumentInternalDTO.URL,
                    createDocumentInternalDTO.ExpiryDate);
            }

            return new DocumentDTO(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                RandomData.GetRandomString(50),
                RandomData.GetRandomString(8),
                RandomData.GetRandomDate(),
                RandomData.GetRandomString(50),
                RandomData.GetRandomString(250),
                RandomData.GetRandomDate());
        }

        public static Document GenerateDocumentObject(bool realisticData = false, DateTime? avoidFiledDate = null, DateTime? avoidDocumentDate = null, DateTime? avoidExpiryDate = null)
        {
            // 18249 days = 1 day short of 50 years
            // 18250 days = 50 years exactly
            // 36500 days = 100 years
            // Filed date must be after the document date, so is set between 50 years ago (minus 1 day) and today
            // Document date must be before the filed date, so is set between 100 years ago and 50 years ago
            // Expiry date must be after both dates, so is set between tomorrow and 100 years from now
            DateTime filedDate = RandomData.GetRandomDate(DateTime.Now.AddDays(-18249), DateTime.Now);
            DateTime documentDate =
                RandomData.GetRandomDate(DateTime.Now.AddDays(-36500), DateTime.Now.AddDays(-18250));
            DateTime expiryDate = RandomData.GetRandomDate(DateTime.Now.AddDays(1), DateTime.Now.AddDays(36500));

            while (filedDate == avoidFiledDate)
            {
                filedDate = RandomData.GetRandomDate(DateTime.Now.AddDays(-18249), DateTime.Now);
            }

            while (documentDate == avoidDocumentDate)
            {
                documentDate = RandomData.GetRandomDate(DateTime.Now.AddDays(-36500), DateTime.Now.AddDays(-18250));
            }

            while (expiryDate == avoidExpiryDate)
            {
                expiryDate = RandomData.GetRandomDate(DateTime.Now, DateTime.Now.AddDays(36500));
            }

            DocumentType documentType = DocumentTypes.GenerateDocumentTypeObject(realisticData);

            return new Document
            {
                Id = Guid.NewGuid(),
                DocumentTypeId = documentType.Id,
                DocumentType = documentType,
                FiledDate = filedDate,
                DocumentDate = documentDate,
                DocumentRef = RandomData.GetRandomString(50),
                DocumentURL = RandomData.GetRandomString(250),
                ExpiryDate = expiryDate
            };
        }

        /// <summary>
        /// Generates a <see cref="CreateDocumentDTO"/> object.
        /// </summary>
        /// <param name="documentTypeId">The ID of the <see cref="DocumentType"/> to be included in the <see cref="CreateDocumentDTO"/> object.</param>
        /// <param name="avoidDocumentDate">An optional date value which will be avoided when setting the <see cref="CreateDocumentDTO.DocumentDate">DocumentDate</see> property.</param>
        /// <param name="avoidExpiryDate">An optional date value which will be avoided when setting the <see cref="CreateDocumentDTO.ExpiryDate">ExpiryDate</see> property.</param>
        /// <returns>A <see cref="CreateDocumentDTO"/> object.</returns>
        public static CreateDocumentDTO GenerateCreateDocumentDTO(Guid documentTypeId, DateTime? avoidDocumentDate = null, DateTime? avoidExpiryDate = null)
        {
            // 18250 days = 50 years
            // 36500 days = 100 years
            // Document date must be before the expiry date, so is set between 100 years ago and today
            // Expiry date must be after the document date, so is set between tomorrow and 100 years from now
            DateTime documentDate =
                RandomData.GetRandomDate(DateTime.Now.AddDays(-36500), DateTime.Now);
            DateTime expiryDate = RandomData.GetRandomDate(DateTime.Now.AddDays(1), DateTime.Now.AddDays(36500));

            while (documentDate == avoidDocumentDate)
            {
                documentDate = RandomData.GetRandomDate(DateTime.Now.AddDays(-36500), DateTime.Now.AddDays(-18250));
            }

            while (expiryDate == avoidExpiryDate)
            {
                expiryDate = RandomData.GetRandomDate(DateTime.Now, DateTime.Now.AddDays(36500));
            }

            return new CreateDocumentDTO
            {
                DocumentTypeId = documentTypeId.ToString(),
                DocumentDate = documentDate,
                Reference = RandomData.GetRandomString(20),
                URL = RandomData.GetRandomString(250),
                ExpiryDate = expiryDate
            };
        }

        /// <summary>
        /// Generates a <see cref="CreateDocumentInternalDTO"/> object based on a provided
        /// <see cref="CreateDocumentDTO"/>.
        /// </summary>
        /// <param name="createDocumentDTO">The <see cref="CreateDocumentDTO"/> to be used as a base for the <see cref="CreateDocumentInternalDTO"/>.</param>
        /// <returns>A <see cref="CreateDocumentInternalDTO"/> object.</returns>
        public static CreateDocumentInternalDTO GenerateCreateDocumentInternalDTO(CreateDocumentDTO createDocumentDTO) => new ()
        {
            DocumentTypeId = new Guid(createDocumentDTO.DocumentTypeId),
            DocumentDate = createDocumentDTO.DocumentDate,
            Reference = createDocumentDTO.Reference,
            URL = createDocumentDTO.URL,
            ExpiryDate = createDocumentDTO.ExpiryDate
        };

        public static CreateDocumentInternalDTO GenerateCreateDocumentInternalDTO(Guid documentTypeId, DateTime? avoidDocumentDate = null, DateTime? avoidExpiryDate = null)
        {
            // 18250 days = 50 years
            // 36500 days = 100 years
            // Document date must be before the expiry date, so is set between 100 years ago and today
            // Expiry date must be after the document date, so is set between tomorrow and 100 years from now
            DateTime documentDate =
                RandomData.GetRandomDate(DateTime.Now.AddDays(-36500), DateTime.Now);
            DateTime expiryDate = RandomData.GetRandomDate(DateTime.Now.AddDays(1), DateTime.Now.AddDays(36500));

            while (documentDate == avoidDocumentDate)
            {
                documentDate = RandomData.GetRandomDate(DateTime.Now.AddDays(-36500), DateTime.Now.AddDays(-18250));
            }

            while (expiryDate == avoidExpiryDate)
            {
                expiryDate = RandomData.GetRandomDate(DateTime.Now, DateTime.Now.AddDays(36500));
            }

            return new CreateDocumentInternalDTO
            {
                DocumentTypeId = documentTypeId,
                DocumentDate = documentDate,
                Reference = RandomData.GetRandomString(20),
                URL = RandomData.GetRandomString(250),
                ExpiryDate = expiryDate
            };
        }
    }
}
