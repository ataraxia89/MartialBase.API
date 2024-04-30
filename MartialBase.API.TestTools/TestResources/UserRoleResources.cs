// <copyright file="UserRoleResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using MartialBase.API.Data;
using MartialBase.API.Data.Collections;
using MartialBase.API.Data.Models.EntityFramework;
using MartialBase.API.Models.DTOs.UserRoles;

using Xunit;

namespace MartialBase.API.TestTools.TestResources
{
    internal class UserRoleResources
    {
        public static IEnumerable<object[]> NonSystemAdminRoleNames
        {
            get
            {
                var roleNames = new List<object[]>();

                foreach (string roleName in UserRoles.GetRoles.Select(ur => ur.Name))
                {
                    if (roleName != UserRoles.Thanos &&
                        roleName != UserRoles.SystemAdmin)
                    {
                        roleNames.Add(new object[] { roleName });
                    }
                }

                return roleNames.ToArray();
            }
        }

        public static IEnumerable<object[]> SystemAdminRoleNames
        {
            get
            {
                var roleNames = new List<object[]>
                                    {
                                        new object[] { UserRoles.Thanos },
                                        new object[] { UserRoles.SystemAdmin }
                                    };

                return roleNames.ToArray();
            }
        }

        public static IEnumerable<object[]> AllUserRoleNames
        {
            get
            {
                var roleNames = new List<object[]>();

                foreach (string roleName in UserRoles.GetRoles.Select(ur => ur.Name))
                {
                    roleNames.Add(new object[] { roleName });
                }

                return roleNames.ToArray();
            }
        }

        public static IEnumerable<object[]> OrganisationUserRoleNames
        {
            get
            {
                var roleNames = new List<object[]>();

                foreach (string roleName in UserRoles.GetRoles.Select(ur => ur.Name))
                {
                    if (roleName == UserRoles.OrganisationMember ||
                        roleName == UserRoles.OrganisationAdmin)
                    {
                        roleNames.Add(new object[] { roleName });
                    }
                }

                return roleNames.ToArray();
            }
        }

        public static IEnumerable<object[]> NonOrganisationUserRoleNames
        {
            get
            {
                var roleNames = new List<object[]>();

                foreach (string roleName in UserRoles.GetRoles.Select(ur => ur.Name))
                {
                    if (roleName != UserRoles.Thanos &&
                        roleName != UserRoles.OrganisationMember &&
                        roleName != UserRoles.OrganisationAdmin)
                    {
                        roleNames.Add(new object[] { roleName });
                    }
                }

                return roleNames.ToArray();
            }
        }

        public static IEnumerable<object[]> NonOrganisationAdminRoleNames
        {
            get
            {
                var roleNames = new List<object[]>();

                foreach (string roleName in UserRoles.GetRoles.Select(ur => ur.Name))
                {
                    if (roleName != UserRoles.Thanos &&
                        roleName != UserRoles.OrganisationAdmin)
                    {
                        roleNames.Add(new object[] { roleName });
                    }
                }

                return roleNames.ToArray();
            }
        }

        public static IEnumerable<object[]> SchoolMemberRoleNames
        {
            get
            {
                var roleNames = new List<object[]>();

                foreach (string roleName in UserRoles.GetRoles.Select(ur => ur.Name))
                {
                    if (roleName == UserRoles.SchoolMember ||
                        roleName == UserRoles.SchoolHeadInstructor ||
                        roleName == UserRoles.SchoolSecretary)
                    {
                        roleNames.Add(new object[] { roleName });
                    }
                }

                return roleNames.ToArray();
            }
        }

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of <see cref="object"/> arrays representing all role names
        /// which don't relate to <see cref="School"/> secretaries, to be used as test member data.
        /// </summary>
        public static IEnumerable<object[]> NonSchoolSecretaryRoleNames
        {
            get
            {
                var roleNames = new List<object[]>();

                foreach (string roleName in UserRoles.GetRoles.Select(ur => ur.Name))
                {
                    if (roleName != UserRoles.SchoolSecretary &&
                        roleName != UserRoles.Thanos)
                    {
                        roleNames.Add(new object[] { roleName });
                    }
                }

                return roleNames.ToArray();
            }
        }

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of <see cref="object"/> arrays representing all role names
        /// which don't relate to <see cref="School"/> secretaries or <see cref="Organisation"/> admins, to be
        /// used as test member data.
        /// </summary>
        public static IEnumerable<object[]> NonPersonAdminRoleNames
        {
            get
            {
                var roleNames = new List<object[]>();

                foreach (string roleName in UserRoles.GetRoles.Select(ur => ur.Name))
                {
                    if (roleName != UserRoles.SchoolSecretary &&
                        roleName != UserRoles.OrganisationAdmin &&
                        roleName != UserRoles.Thanos)
                    {
                        roleNames.Add(new object[] { roleName });
                    }
                }

                return roleNames.ToArray();
            }
        }

        internal static Guid GetUserRoleId(string roleName, string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.UserRoles.First(ur => ur.Name == roleName).Id;
            }
        }

        internal static IEnumerable<UserRole> GetUserRoles(string dbIdentifier)
        {
            using (MartialBaseDbContext dbContext = DatabaseTools.GetMartialBaseDbContext(dbIdentifier))
            {
                return dbContext.UserRoles.ToList();
            }
        }

        internal static void AssertEqual(List<UserRole> expected, List<UserRole> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (UserRole expectedRole in expected)
            {
                UserRole actualRole = actual.FirstOrDefault(ur => ur.Id == expectedRole.Id);

                AssertEqual(expectedRole, actualRole);
            }
        }

        internal static void AssertEqual(List<UserRole> expected, List<UserRoleDTO> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (UserRole expectedRole in expected)
            {
                UserRoleDTO actualRole = actual.FirstOrDefault(ur => ur.Id == expectedRole.Id.ToString());

                AssertEqual(expectedRole, actualRole);
            }
        }

        internal static void AssertEqual(UserRole expected, UserRole actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Name, actual.Name);
        }

        internal static void AssertEqual(UserRole expected, UserRoleDTO actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Id, new Guid(actual.Id));
            Assert.Equal(expected.Name, actual.Name);
        }
    }
}
