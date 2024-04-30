// <copyright file="UserRoles.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;

using MartialBase.API.Data.Models.EntityFramework;

namespace MartialBase.API.Data.Collections
{
    /// <summary>
    /// A collection of string constants representing user roles within the application.
    /// </summary>
    public static class UserRoles
    {
        /// <summary>
        /// A role representing a super user.
        /// </summary>
        public const string Thanos = "Thanos";

        /// <summary>
        /// The base user role assigned to all users.
        /// </summary>
        public const string User = API.Models.Collections.UserRole.User;

        /// <summary>
        /// A role representing a user that is a system administrator, who is responsible for managing
        /// application users.
        /// </summary>
        /// <remarks>System administrators should not have access to personal data.</remarks>
        public const string SystemAdmin = API.Models.Collections.UserRole.SystemAdmin;

        /// <summary>
        /// A role representing a user that is a member/student of a <see cref="School"/>.
        /// </summary>
        public const string SchoolMember = API.Models.Collections.UserRole.SchoolMember;

        /// <summary>
        /// A role representing a user that is an instructor at a <see cref="School"/>.
        /// </summary>
        public const string SchoolInstructor = API.Models.Collections.UserRole.SchoolInstructor;

        /// <summary>
        /// A role representing a user that is the head instructor of a <see cref="School"/>.
        /// </summary>
        public const string SchoolHeadInstructor = API.Models.Collections.UserRole.SchoolHeadInstructor;

        /// <summary>
        /// A role representing a user that is a secretary at a <see cref="School"/>.
        /// </summary>
        public const string SchoolSecretary = API.Models.Collections.UserRole.SchoolSecretary;

        /// <summary>
        /// A role representing a user that is a member of an <see cref="Organisation"/>.
        /// </summary>
        public const string OrganisationMember = API.Models.Collections.UserRole.OrganisationMember;

        /// <summary>
        /// A role representing a user that is an admin of an <see cref="Organisation"/>.
        /// </summary>
        public const string OrganisationAdmin = API.Models.Collections.UserRole.OrganisationAdmin;

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}">IEnumerable</see> of user role constants.
        /// </summary>
        public static IEnumerable<UserRole> GetRoles => new[]
                {
                    new UserRole { Name = Thanos },
                    new UserRole { Name = User },
                    new UserRole { Name = SystemAdmin },
                    new UserRole { Name = SchoolMember },
                    new UserRole { Name = SchoolInstructor },
                    new UserRole { Name = SchoolHeadInstructor },
                    new UserRole { Name = SchoolSecretary },
                    new UserRole { Name = OrganisationMember },
                    new UserRole { Name = OrganisationAdmin }
                };
    }
}