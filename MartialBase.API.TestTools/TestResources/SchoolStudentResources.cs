// <copyright file="SchoolStudentResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data;
using MartialBase.API.Data.Models.EntityFramework;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace MartialBase.API.TestTools.TestResources
{
    internal static class SchoolStudentResources
    {
        internal static void AssertSchoolDoesNotHaveStudents(Guid schoolId, List<Guid> studentIds, string dbIdentifier)
        {
            foreach (Guid studentId in studentIds)
            {
                Assert.False(CheckSchoolHasStudent(schoolId, studentId, dbIdentifier).Exists);
            }
        }

        internal static (bool Exists, bool IsInstructor, bool IsSecretary) CheckSchoolHasStudent(Guid schoolId, Guid studentId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                SchoolStudent schoolStudent = dbContext.SchoolStudents.FirstOrDefault(ss =>
                    ss.SchoolId == schoolId && ss.StudentId == studentId);

                return (
                    schoolStudent != null,
                    schoolStudent?.IsInstructor ?? false,
                    schoolStudent?.IsSecretary ?? false);
            }
        }

        internal static SchoolStudent GetSchoolStudent(Guid schoolId, Guid studentId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.SchoolStudents
                    .Include(ss => ss.InsuranceDocument)
                    .ThenInclude(d => d.DocumentType)
                    .ThenInclude(dt => dt.Organisation)
                    .ThenInclude(o => o.Address)
                    .Include(ss => ss.LicenceDocument)
                    .ThenInclude(d => d.DocumentType)
                    .ThenInclude(dt => dt.Organisation)
                    .ThenInclude(o => o.Address)
                    .FirstOrDefault(ss => ss.SchoolId == schoolId && ss.StudentId == studentId);
            }
        }

        internal static SchoolStudent GetStudentSchool(Guid studentId, Guid schoolId, string dbIdentifier) => GetSchoolStudent(schoolId, studentId, dbIdentifier);

        internal static void EnsureSchoolHasStudents(Guid schoolId, List<Guid> studentIds, string dbIdentifier)
        {
            foreach (Guid studentId in studentIds)
            {
                EnsureSchoolHasStudent(schoolId, studentId, dbIdentifier);
            }
        }

        internal static void EnsureSchoolHasStudent(Guid schoolId, Guid studentId, string dbIdentifier, bool isInstructor = false, bool isSecretary = false)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                SchoolStudent schoolStudent = dbContext.SchoolStudents
                    .FirstOrDefault(ss =>
                        ss.SchoolId == schoolId &&
                        ss.StudentId == studentId);

                if (schoolStudent == null)
                {
                    dbContext.SchoolStudents.Add(new SchoolStudent
                    {
                        Id = Guid.NewGuid(),
                        SchoolId = schoolId,
                        StudentId = studentId,
                        IsInstructor = isInstructor,
                        IsSecretary = isSecretary
                    });

                    Assert.True(dbContext.SaveChanges() > 0);
                }
                else if (schoolStudent.IsInstructor != isInstructor || schoolStudent.IsSecretary != isSecretary)
                {
                    schoolStudent.IsInstructor = isInstructor;
                    schoolStudent.IsSecretary = isSecretary;

                    Assert.True(dbContext.SaveChanges() > 0);
                }
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.SchoolStudents.Any(ss =>
                    ss.SchoolId == schoolId &&
                    ss.StudentId == studentId &&
                    ss.IsInstructor == isInstructor &&
                    ss.IsSecretary == isSecretary));
            }
        }

        internal static void EnsureSchoolDoesNotHaveStudent(Guid schoolId, Guid studentId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                SchoolStudent schoolStudent = dbContext.SchoolStudents
                    .FirstOrDefault(ss =>
                        ss.SchoolId == schoolId &&
                        ss.StudentId == studentId);

                if (schoolStudent != null)
                {
                    dbContext.SchoolStudents.Remove(schoolStudent);

                    Assert.True(dbContext.SaveChanges() > 0);
                }
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.False(dbContext.SchoolStudents.Any(ss =>
                    ss.SchoolId == schoolId &&
                    ss.StudentId == studentId));
            }
        }

        internal static void EnsureSchoolsHaveStudent(List<Guid> schoolIds, Guid studentId, string dbIdentifier, bool isInstructor = false, bool isSecretary = false)
        {
            foreach (Guid schoolId in schoolIds)
            {
                EnsureSchoolHasStudent(schoolId, studentId, dbIdentifier, isInstructor, isSecretary);
            }
        }

        internal static void EnsureSchoolsDoNotHaveStudent(List<Guid> schoolIds, Guid studentId, string dbIdentifier)
        {
            foreach (Guid schoolId in schoolIds)
            {
                EnsureSchoolDoesNotHaveStudent(schoolId, studentId, dbIdentifier);
            }
        }

        internal static void EnsureStudentHasLicenceDocument(Guid schoolId, Guid studentId, Guid documentId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                SchoolStudent schoolStudent = dbContext.SchoolStudents.First(ss =>
                    ss.SchoolId == schoolId &&
                    ss.StudentId == studentId);

                if (schoolStudent.LicenceDocumentId != documentId)
                {
                    schoolStudent.LicenceDocumentId = documentId;

                    Assert.True(dbContext.SaveChanges() > 0);
                }

                if (!dbContext.PersonDocuments.Any(pd =>
                    pd.PersonId == studentId &&
                    pd.DocumentId == documentId))
                {
                    var personDocument = new PersonDocument
                    {
                        Id = Guid.NewGuid(),
                        PersonId = studentId,
                        DocumentId = documentId,
                        IsActive = true
                    };

                    dbContext.PersonDocuments.Add(personDocument);

                    Assert.True(dbContext.SaveChanges() > 0);
                }
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.SchoolStudents.Any(ss =>
                    ss.SchoolId == schoolId &&
                    ss.StudentId == studentId &&
                    ss.LicenceDocumentId == documentId));

                Assert.True(dbContext.PersonDocuments.Any(pd =>
                    pd.PersonId == studentId &&
                    pd.DocumentId == documentId));
            }
        }

        internal static void EnsureStudentHasInsuranceDocument(Guid schoolId, Guid studentId, Guid documentId, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                SchoolStudent schoolStudent = dbContext.SchoolStudents.First(ss =>
                    ss.SchoolId == schoolId &&
                    ss.StudentId == studentId);

                if (schoolStudent.InsuranceDocumentId != documentId)
                {
                    schoolStudent.InsuranceDocumentId = documentId;

                    Assert.True(dbContext.SaveChanges() > 0);
                }

                if (!dbContext.PersonDocuments.Any(pd =>
                    pd.PersonId == studentId &&
                    pd.DocumentId == documentId))
                {
                    var personDocument = new PersonDocument
                    {
                        Id = Guid.NewGuid(),
                        PersonId = studentId,
                        DocumentId = documentId,
                        IsActive = true
                    };

                    dbContext.PersonDocuments.Add(personDocument);

                    Assert.True(dbContext.SaveChanges() > 0);
                }
            }

            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                Assert.True(dbContext.SchoolStudents.Any(ss =>
                    ss.SchoolId == schoolId &&
                    ss.StudentId == studentId &&
                    ss.InsuranceDocumentId == documentId));

                Assert.True(dbContext.PersonDocuments.Any(pd =>
                    pd.PersonId == studentId &&
                    pd.DocumentId == documentId));
            }
        }
    }
}