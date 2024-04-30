// <copyright file="Utilities.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

namespace MartialBase.API.Utilities
{
    internal static class Utilities
    {
        internal static string RemoveAccentedCharacters(string inputString)
        {
            string retString = inputString;
            Regex pattern;

            pattern = new Regex("[ÁÂÃÄÅȦȂȀĄĂ]");
            pattern.Replace(retString, "A");
            pattern = new Regex("[àáâãäåăąȁȃȧ]");
            pattern.Replace(retString, "a");

            pattern = new Regex("[ČĊĈĆÇ]");
            pattern.Replace(retString, "C");
            pattern = new Regex("[çćĉċč]");
            pattern.Replace(retString, "c");

            pattern = new Regex("[ĐĎ]");
            pattern.Replace(retString, "D");
            pattern = new Regex("[ďđ]");
            pattern.Replace(retString, "d");

            pattern = new Regex("[ȨȆȄĚĘĖĔĒÈÉÊË]");
            pattern.Replace(retString, "E");
            pattern = new Regex("[èéêëēĕėęěȅȇȩ]");
            pattern.Replace(retString, "e");

            pattern = new Regex("[ĢĠĞĜ]");
            pattern.Replace(retString, "G");
            pattern = new Regex("[ĝğġģ]");
            pattern.Replace(retString, "g");

            pattern = new Regex("[ȞĦĤ]");
            pattern.Replace(retString, "H");
            pattern = new Regex("[ĥħȟ]");
            pattern.Replace(retString, "h");

            pattern = new Regex("[ȊȈĬĪĨÌÍÎÏĮ]");
            pattern.Replace(retString, "I");
            pattern = new Regex("[ìíîïĩīĭȉȋį]");
            pattern.Replace(retString, "i");

            pattern = new Regex("Ĵ");
            pattern.Replace(retString, "J");
            pattern = new Regex("ĵ");
            pattern.Replace(retString, "j");

            pattern = new Regex("Ķ");
            pattern.Replace(retString, "K");
            pattern = new Regex("ķ");
            pattern.Replace(retString, "k");

            pattern = new Regex("[ĽĿŁĻĹ]");
            pattern.Replace(retString, "L");
            pattern = new Regex("[ĺļľŀł]");
            pattern.Replace(retString, "l");

            pattern = new Regex("[ŊŇŅŃÑ]");
            pattern.Replace(retString, "N");
            pattern = new Regex("[ñńņňŉŋ]");
            pattern.Replace(retString, "n");

            pattern = new Regex("[ȎȌŐŎŌÒÓÔÕÖ]");
            pattern.Replace(retString, "O");
            pattern = new Regex("[òóôõöōŏőȍȏ]");
            pattern.Replace(retString, "o");

            pattern = new Regex("[ȒȐŘŖŔ]");
            pattern.Replace(retString, "R");
            pattern = new Regex("[ŕŗřȑȓ]");
            pattern.Replace(retString, "r");

            pattern = new Regex("[ȘŠŞŜŚ]");
            pattern.Replace(retString, "S");
            pattern = new Regex("[śŝşšș]");
            pattern.Replace(retString, "s");

            pattern = new Regex("[ȚŦŤŢ]");
            pattern.Replace(retString, "T");
            pattern = new Regex("[ţťŧț]");
            pattern.Replace(retString, "t");

            pattern = new Regex("[ȖȔŲŰŮŬŪŨÙÚÛÜ]");
            pattern.Replace(retString, "U");
            pattern = new Regex("[ùúûüũūŭůűųȕȗ]");
            pattern.Replace(retString, "u");

            pattern = new Regex("Ŵ");
            pattern.Replace(retString, "W");
            pattern = new Regex("ŵ");
            pattern.Replace(retString, "w");

            pattern = new Regex("[ŶŸ]");
            pattern.Replace(retString, "Y");
            pattern = new Regex("ŷ");
            pattern.Replace(retString, "y");

            pattern = new Regex("[ȤŽŻŹ]");
            pattern.Replace(retString, "Z");
            pattern = new Regex("[źżžȥ]");
            pattern.Replace(retString, "z");

            return retString;
        }

        internal static void AddLog(string pMessage, LogLevel pLogLevel)
        {
            pMessage = $"[App] {pMessage}";

            switch (pLogLevel)
            {
                case LogLevel.Information:
                    Trace.TraceInformation(pMessage);
                    break;

                case LogLevel.Warning:
                    Trace.TraceWarning(pMessage);
                    break;

                case LogLevel.Error:
                    Trace.TraceError(pMessage);
                    break;

                case LogLevel.Critical:
                    Trace.TraceError(pMessage);
                    break;

                case LogLevel.Debug:
                    break;
            }
        }

        internal static string RemoveXMLTags(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var document = new HtmlDocument();
            document.LoadHtml(input);

            var nodes = new Queue<HtmlNode>(document.DocumentNode.SelectNodes("./*|./text()"));
            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                var parentNode = node.ParentNode;

                if (node.Name != "#text")
                {
                    var childNodes = node.SelectNodes("./*|./text()");

                    if (childNodes != null)
                    {
                        foreach (var child in childNodes)
                        {
                            nodes.Enqueue(child);
                            parentNode.InsertBefore(child, node);
                        }
                    }

                    parentNode.RemoveChild(node);
                }
            }

            return document.DocumentNode.InnerHtml;
        }

        internal static string SimplifyObjectReferences(string input)
        {
            if (input == null)
            {
                return null;
            }

            var replaceTerms = new Dictionary<string, string>
            {
                // Auth tools
                { "MartialBase.API.AuthTools.Interfaces.IAzureUserHelper", "IAzureUserHelper" },
                { "MartialBase.API.AuthTools.Interfaces.IMartialBaseUserHelper", "IMartialBaseUserHelper" },

                // Controllers
                { "MartialBase.API.Controllers.APIHealthController", "APIHealthController" },
                { "MartialBase.API.Controllers.ArtGradesController", "ArtGradesController" },
                { "MartialBase.API.Controllers.ArtsController", "ArtsController" },
                { "MartialBase.API.Controllers.AuthController", "AuthController" },
                { "MartialBase.API.Controllers.CountriesController", "CountriesController" },
                { "MartialBase.API.Controllers.DocumentTypesController", "DocumentTypesController" },
                { "MartialBase.API.Controllers.MartialBaseControllerBase", "MartialBaseControllerBase" },
                { "MartialBase.API.Controllers.MartialBaseUserController", "MartialBaseUserController" },
                { "MartialBase.API.Controllers.OrganisationsController", "OrganisationsController" },
                { "MartialBase.API.Controllers.PeopleController", "PeopleController" },
                { "MartialBase.API.Controllers.SchoolsController", "SchoolsController" },

                // User roles
                { "MartialBase.API.Data.Collections.UserRoles.ThanosThanos", "Thanos" },
                { "MartialBase.API.Data.Collections.UserRoles.UserUser", "User" },
                { "MartialBase.API.Data.Collections.UserRoles.SystemAdminSystemAdmin", "SystemAdmin" },
                { "MartialBase.API.Data.Collections.UserRoles.SchoolMemberSchoolMember", "SchoolMember" },
                { "MartialBase.API.Data.Collections.UserRoles.SchoolInstructorSchoolInstructor", "SchoolInstructor" },
                { "MartialBase.API.Data.Collections.UserRoles.SchoolHeadInstructorSchoolHeadInstructor", "SchoolHeadInstructor" },
                { "MartialBase.API.Data.Collections.UserRoles.SchoolSecretarySchoolSecretary", "SchoolSecretary" },
                { "MartialBase.API.Data.Collections.UserRoles.OrganisationMemberOrganisationMember", "OrganisationMember" },
                { "MartialBase.API.Data.Collections.UserRoles.OrganisationAdminOrganisationAdmin", "OrganisationAdmin" },

                // EntityFramework models
                { "MartialBase.API.Data.Models.EntityFramework.Address", "Address" },
                { "MartialBase.API.Data.Models.EntityFramework.Art", "Art" },
                { "MartialBase.API.Data.Models.EntityFramework.ArtGrade", "ArtGrade" },
                { "MartialBase.API.Data.Models.EntityFramework.BankAccount", "BankAccount" },
                { "MartialBase.API.Data.Models.EntityFramework.Country", "Country" },
                { "MartialBase.API.Data.Models.EntityFramework.Document", "Document" },
                { "MartialBase.API.Data.Models.EntityFramework.DocumentType", "DocumentType" },
                { "MartialBase.API.Data.Models.EntityFramework.Event", "Event" },
                { "MartialBase.API.Data.Models.EntityFramework.EventType", "EventType" },
                { "MartialBase.API.Data.Models.EntityFramework.Lesson", "Lesson" },
                { "MartialBase.API.Data.Models.EntityFramework.LessonPlan", "LessonPlan" },
                { "MartialBase.API.Data.Models.EntityFramework.LessonPlanLine", "LessonPlanLine" },
                { "MartialBase.API.Data.Models.EntityFramework.MartialBaseUser", "MartialBaseUser" },
                { "MartialBase.API.Data.Models.EntityFramework.MartialBaseUserRole", "MartialBaseUserRole" },
                { "MartialBase.API.Data.Models.EntityFramework.MedicalCategory", "MedicalCategory" },
                { "MartialBase.API.Data.Models.EntityFramework.Order", "Order" },
                { "MartialBase.API.Data.Models.EntityFramework.OrderLine", "OrderLine" },
                { "MartialBase.API.Data.Models.EntityFramework.OrderLineDelivery", "OrderLineDelivery" },
                { "MartialBase.API.Data.Models.EntityFramework.Organisation", "Organisation" },
                { "MartialBase.API.Data.Models.EntityFramework.OrganisationPerson", "OrganisationPerson" },
                { "MartialBase.API.Data.Models.EntityFramework.Payment", "Payment" },
                { "MartialBase.API.Data.Models.EntityFramework.Person", "Person" },
                { "MartialBase.API.Data.Models.EntityFramework.PersonStudent", "Student" },
                { "MartialBase.API.Data.Models.EntityFramework.PersonDocument", "PersonDocument" },
                { "MartialBase.API.Data.Models.EntityFramework.Product", "Product" },
                { "MartialBase.API.Data.Models.EntityFramework.ProductCategory", "ProductCategory" },
                { "MartialBase.API.Data.Models.EntityFramework.ProductPrice", "ProductPrice" },
                { "MartialBase.API.Data.Models.EntityFramework.RecurringLesson", "RecurringLesson" },
                { "MartialBase.API.Data.Models.EntityFramework.School", "School" },
                { "MartialBase.API.Data.Models.EntityFramework.SchoolAddress", "SchoolAddress" },
                { "MartialBase.API.Data.Models.EntityFramework.SchoolStudent", "SchoolStudent" },
                { "MartialBase.API.Data.Models.EntityFramework.SystemNote", "SystemNote" },
                { "MartialBase.API.Data.Models.EntityFramework.Task", "Task" },
                { "MartialBase.API.Data.Models.EntityFramework.TaskItem", "TaskItem" },
                { "MartialBase.API.Data.Models.EntityFramework.UserRole", "UserRole" },

                // Internal DTOs
                { "MartialBase.API.Data.Models.InternalDTOs.ArtGrades.CreateArtGradeInternalDTO", "CreateArtGradeInternalDTO" },
                { "MartialBase.API.Data.Models.InternalDTOs.Documents.CreateDocumentInternalDTO", "CreateDocumentInternalDTO" },
                { "MartialBase.API.Data.Models.InternalDTOs.DocumentTypes.CreateDocumentTypeInternalDTO", "CreateDocumentTypeInternalDTO" },
                { "MartialBase.API.Data.Models.InternalDTOs.Organisations.CreateOrganisationInternalDTO", "CreateOrganisationInternalDTO" },
                { "MartialBase.API.Data.Models.InternalDTOs.People.CreatePersonInternalDTO", "CreatePersonInternalDTO" },
                { "MartialBase.API.Data.Models.InternalDTOs.Schools.CreateSchoolInternalDTO", "CreateSchoolInternalDTO" },

                // Repositories
                { "MartialBase.API.Data.Repositories.Interfaces.IAddressesRepository", "IAddressesRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.IArtGradesRepository", "IArtGradesRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.IArtsRepository", "IArtsRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.ICountriesRepository", "ICountriesRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.IDocumentsRepository", "IDocumentsRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.IDocumentTypesRepository", "IDocumentTypesRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.IMartialBaseUserRolesRepository", "IMartialBaseUserRolesRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.IMartialBaseUsersRepository", "IMartialBaseUsersRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.IOrganisationsRepository", "IOrganisationsRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.IPeopleRepository", "IPeopleRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.IPersonDocumentsRepository", "IPersonDocumentsRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.IRepository", "IRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.ISchoolsRepository", "ISchoolsRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.IUserRolesRepository", "IUserRolesRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.AddressesRepository", "AddressesRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.ArtGradesRepository", "ArtGradesRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.ArtsRepository", "ArtsRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.CountriesRepository", "CountriesRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.DocumentsRepository", "DocumentsRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.DocumentTypesRepository", "DocumentTypesRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.MartialBaseUserRolesRepository", "MartialBaseUserRolesRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.MartialBaseUsersRepository", "MartialBaseUsersRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.OrganisationsRepository", "OrganisationsRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.PeopleRepository", "PeopleRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.PersonDocumentsRepository", "PersonDocumentsRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.SchoolsRepository", "SchoolsRepository" },
                { "MartialBase.API.Data.Repositories.Interfaces.UserRolesRepository", "UserRolesRepository" },

                // Public DTOs
                { "MartialBase.API.Models.DTOs.Addresses.AddressDTO", "AddressDTO" },
                { "MartialBase.API.Models.DTOs.Addresses.CreateAddressDTO", "CreateAddressDTO" },
                { "MartialBase.API.Models.DTOs.Addresses.UpdateAddressDTO", "UpdateAddressDTO" },
                { "MartialBase.API.Models.DTOs.ArtGrades.ArtGradeDTO", "ArtGradeDTO" },
                { "MartialBase.API.Models.DTOs.ArtGrades.CreateArtGradeDTO", "CreateArtGradeDTO" },
                { "MartialBase.API.Models.DTOs.ArtGrades.UpdateArtGradeDTO", "UpdateArtGradeDTO" },
                { "MartialBase.API.Models.DTOs.Arts.ArtDTO", "ArtDTO" },
                { "MartialBase.API.Models.DTOs.Countries.CountryDTO", "CountryDTO" },
                { "MartialBase.API.Models.DTOs.Documents.CreateDocumentDTO", "CreateDocumentDTO" },
                { "MartialBase.API.Models.DTOs.Documents.DocumentDTO", "DocumentDTO" },
                { "MartialBase.API.Models.DTOs.Documents.UpdateDocumentDTO", "UpdateDocumentDTO" },
                { "MartialBase.API.Models.DTOs.DocumentTypes.CreateDocumentTypeDTO", "CreateDocumentTypeDTO" },
                { "MartialBase.API.Models.DTOs.DocumentTypes.DocumentTypeDTO", "DocumentTypeDTO" },
                { "MartialBase.API.Models.DTOs.DocumentTypes.UpdateDocumentTypeDTO", "UpdateDocumentTypeDTO" },
                { "MartialBase.API.Models.DTOs.MartialBaseUsers.MartialBaseUserDTO", "MartialBaseUserDTO" },
                { "MartialBase.API.Models.DTOs.Organisations.CreateOrganisationDTO", "CreateOrganisationDTO" },
                { "MartialBase.API.Models.DTOs.Organisations.CreatePersonOrganisationDTO", "CreatePersonOrganisationDTO" },
                { "MartialBase.API.Models.DTOs.Organisations.OrganisationDTO", "OrganisationDTO" },
                { "MartialBase.API.Models.DTOs.Organisations.UpdateOrganisationDTO", "UpdateOrganisationDTO" },
                { "MartialBase.API.Models.DTOs.People.CreatedPersonDTO", "CreatedPersonDTO" },
                { "MartialBase.API.Models.DTOs.People.CreatedPersonOrganisationDTO", "CreatedPersonOrganisationDTO" },
                { "MartialBase.API.Models.DTOs.People.CreatePersonDTO", "CreatePersonDTO" },
                { "MartialBase.API.Models.DTOs.People.OrganisationPersonDTO", "OrganisationPersonDTO" },
                { "MartialBase.API.Models.DTOs.People.PersonDTO", "PersonDTO" },
                { "MartialBase.API.Models.DTOs.People.PersonOrganisationDTO", "PersonOrganisationDTO" },
                { "MartialBase.API.Models.DTOs.People.SchoolStudentDTO", "SchoolStudentDTO" },
                { "MartialBase.API.Models.DTOs.People.StudentSchoolDTO", "StudentSchoolDTO" },
                { "MartialBase.API.Models.DTOs.People.UpdatePersonDTO", "UpdatePersonDTO" },
                { "MartialBase.API.Models.DTOs.Products.CreateProductDTO", "CreateProductDTO" },
                { "MartialBase.API.Models.DTOs.Products.ProductDTO", "ProductDTO" },
                { "MartialBase.API.Models.DTOs.Products.UpdateProductDTO", "UpdateProductDTO" },
                { "MartialBase.API.Models.DTOs.Schools.CreateSchoolDTO", "CreateSchoolDTO" },
                { "MartialBase.API.Models.DTOs.Schools.SchoolDTO", "SchoolDTO" },
                { "MartialBase.API.Models.DTOs.Schools.UpdateSchoolDTO", "UpdateSchoolDTO" },
                { "MartialBase.API.Models.DTOs.UserRoles.UserRoleDTO", "UserRoleDTO" },
                { "MartialBase.API.Models.DTOs.Addresses.AddressDTOAddresses", "AddressDTO objects" },
                { "MartialBase.API.Models.DTOs.ArtGrades.ArtGradeDTOArtGrades", "ArtGradeDTO objects" },
                { "MartialBase.API.Models.DTOs.Arts.ArtDTOArts", "ArtDTO objects" },
                { "MartialBase.API.Models.DTOs.Countries.CountryDTOCountries", "CountryDTO objects" },
                { "MartialBase.API.Models.DTOs.Documents.DocumentDTODocuments", "DocumentDTO objects" },
                { "MartialBase.API.Models.DTOs.DocumentTypes.DocumentTypeDTODocumentTypes", "DocumentTypeDTO objects" },
                { "MartialBase.API.Models.DTOs.MartialBaseUsers.MartialBaseUserDTOMartialBaseUsers", "MartialBaseUserDTO objects" },
                { "MartialBase.API.Models.DTOs.Organisations.OrganisationDTOOrganisations", "OrganisationDTO objects" },
                { "MartialBase.API.Models.DTOs.People.PersonDTOPeople", "PersonDTO objects" },
                { "MartialBase.API.Models.DTOs.Products.ProductDTOProducts", "ProductDTO objects" },
                { "MartialBase.API.Models.DTOs.Schools.SchoolDTOSchools", "SchoolDTO objects" },
                { "MartialBase.API.Models.DTOs.UserRoles.UserRoleDTOUserRoles", "UserRoleDTO objects" },

                // Application
                { "MartialBase.API.Startup", "Startup" },
                { "Microsoft.AspNetCore.Builder.IApplicationBuilder", "IApplicationBuilder" },
                { "Microsoft.AspNetCore.Hosting.IWebHostEnvironment", "IWebHostEnvironment" },

                // MVC result objects
                { "Microsoft.AspNetCore.Mvc.BadRequestResult", "BadRequestResult" },
                { "Microsoft.AspNetCore.Mvc.CreatedResult", "CreatedResult" },
                { "Microsoft.AspNetCore.Mvc.IActionResult", "IActionResult" },
                { "Microsoft.AspNetCore.Mvc.NotFoundResult", "NotFoundResult" },
                { "Microsoft.AspNetCore.Mvc.NoContentResult", "NoContentResult" },
                { "Microsoft.AspNetCore.Mvc.OkResult", "OkResult" },
                { "Microsoft.AspNetCore.Mvc.UnauthorizedResult", "UnauthorizedResult" },

                // Services
                { "Microsoft.Extensions.Configuration.IConfiguration", "IConfiguration" },
                { "Microsoft.Extensions.DependencyInjection.IServiceCollection", "IServiceCollection" },

                // System objects
                { "System.Collections.Generic.IList`1", "IList" },
                { "System.Collections.Generic.List`1", "List" },

                // System exceptions
                { "System.ExceptionExceptions", "Exceptions" },
                { "System.Exception", "Exception" },

                // Misc.
                { "Person\">Student", "Person" },
                { "Document\">Documents", "Documents" },
                { "DocumentType\">DocumentTypes", "DocumentTypes" },
                { "Person\">People", "People" },
                { "MartialBase.API.TestTools.Models.HttpResponseModel", "HttpResponseModel" },
            };

            foreach (var kvp in replaceTerms)
            {
                input = input.Replace(kvp.Key, kvp.Value);
            }

            return input;
        }
    }
}
