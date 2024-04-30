// <copyright file="RandomData.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Tools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Security.Cryptography;
using System.Text;

namespace MartialBase.API.Tools
{
    /// <summary>
    /// Common methods used to generate random data, typically used for unit testing.
    /// </summary>
    public static class RandomData
    {
        private const int DefaultDateRangeInYears = 200;

        /// <summary>
        /// Generates a random <see cref="string"/>.
        /// </summary>
        /// <param name="length">The desired length of the string to be generated.</param>
        /// <returns>A random <see cref="string"/> to the specified length.</returns>
        public static string GetRandomString(int length)
        {
            return GetRandomString(length, true, true, true, string.Empty);
        }

        /// <summary>
        /// Generates a random <see cref="string"/>.
        /// </summary>
        /// <param name="length">The desired length of the string to be generated.</param>
        /// <param name="includeUpperCase">Whether uppercase characters should be included.</param>
        /// <param name="includeLowerCase">Whether lowercase characters should be included.</param>
        /// <returns>A random <see cref="string"/> to the specified length and included characters.</returns>
        public static string GetRandomString(int length, bool includeUpperCase, bool includeLowerCase)
        {
            return GetRandomString(length, includeUpperCase, includeLowerCase, true, string.Empty);
        }

        /// <summary>
        /// Generates a random <see cref="string"/>.
        /// </summary>
        /// <param name="length">The desired length of the string to be generated.</param>
        /// <param name="includeUpperCase">Whether uppercase characters should be included.</param>
        /// <param name="includeLowerCase">Whether lowercase characters should be included.</param>
        /// <param name="includeNumeric">Whether numeric characters should be included.</param>
        /// <param name="allowedSpecialChars">Which special characters are allowed in the generated string.</param>
        /// <returns>A random <see cref="string"/> to the specified length and included characters.</returns>
        public static string GetRandomString(
            int length,
            bool includeUpperCase,
            bool includeLowerCase,
            bool includeNumeric,
            string allowedSpecialChars)
        {
            if (!includeUpperCase &&
                !includeLowerCase &&
                !includeNumeric &&
                string.IsNullOrEmpty(allowedSpecialChars))
            {
                throw new ArgumentException("No allowed characters for random string.");
            }

            string allowedChars = string.Empty;

            if (includeUpperCase)
            {
                allowedChars += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            }

            if (includeLowerCase)
            {
                allowedChars += "abcdefghijklmnopqrstuvwxyz";
            }

            if (includeNumeric)
            {
                allowedChars += "0123456789";
            }

            if (!string.IsNullOrEmpty(allowedSpecialChars))
            {
                allowedChars += allowedSpecialChars;
            }

            var res = new StringBuilder();

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(allowedChars[(int)(num % (uint)allowedChars.Length)]);
                }
            }

            return res.ToString();
        }

        /// <summary>
        /// Generates a random <see cref="int"/>.
        /// </summary>
        /// <returns>A random <see cref="int"/> equal to or between 0 and <see cref="int.MaxValue"/>.</returns>
        public static int GetRandomNumber() => GetRandomNumber(0, int.MaxValue);

        /// <summary>
        /// Generates a random <see cref="int"/> based on the provided <paramref name="minValue"/> and <paramref name="maxValue"/> parameters.
        /// </summary>
        /// <param name="minValue">The minimum required value of the random <see cref="int"/>.</param>
        /// <param name="maxValue">The maximum required value of the random <see cref="int"/>.</param>
        /// <returns>A random <see cref="int"/> equal to or between the provided <paramref name="minValue"/> and <paramref name="maxValue"/> parameters.</returns>
        public static int GetRandomNumber(int minValue, int maxValue)
        {
            if (maxValue < minValue)
            {
                throw new ArgumentException("Maximum value must be greater than minimum value.");
            }

            if (minValue < 0)
            {
                throw new ArgumentException("Minimum value must be greater than 0.");
            }

            var rng = RandomNumberGenerator.Create();

            byte[] data = new byte[sizeof(int)];
            rng.GetBytes(data, minValue, sizeof(int));

            return BitConverter.ToInt32(data, 0) & maxValue;
        }

        /// <summary>
        /// Generates a random <see cref="DateTime"/>.
        /// </summary>
        /// <returns>A random <see cref="DateTime"/>.</returns>
        public static DateTime GetRandomDate()
        {
            DateTime minDate = new DateTime(1900, 1, 1);

            return GetRandomDate(minDate, minDate.AddYears(DefaultDateRangeInYears));
        }

        /// <summary>
        /// Generates a random <see cref="DateTime"/> based on the optional <paramref name="minDate"/> and <paramref name="maxDate"/> parameters.
        /// </summary>
        /// <param name="minDate">The minimum required value of the random <see cref="DateTime"/>.</param>
        /// <param name="maxDate">The maximum required value of the random <see cref="DateTime"/>.</param>
        /// <returns>A random <see cref="DateTime"/> equal to or between the optional <paramref name="minDate"/> and <paramref name="maxDate"/> parameters (if applicable).</returns>
        public static DateTime GetRandomDate(DateTime minDate, DateTime maxDate)
        {
            if (maxDate < minDate)
            {
                throw new ArgumentException("Maximum date must be greater than minimum date.");
            }

            int dateRangeInMinutes = (int)Math.Floor((maxDate - minDate).TotalMinutes);

            return minDate.AddMinutes(GetRandomNumber(0, dateRangeInMinutes));
        }
    }
}
