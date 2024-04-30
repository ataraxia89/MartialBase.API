// <copyright file="DictionaryResources.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.TestTools
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;

using Xunit;

namespace MartialBase.API.TestTools.TestResources
{
    internal static class DictionaryResources
    {
        internal static void AssertEqual(Dictionary<string, string[]> expected, Dictionary<string, string[]> actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Count, actual.Count);

            foreach (KeyValuePair<string, string[]> expectedEntry in expected)
            {
                Assert.Contains(expectedEntry.Key, actual.Keys);
                Assert.Equal(expectedEntry.Value.Length, actual[expectedEntry.Key].Length);

                foreach (string expectedValue in expectedEntry.Value)
                {
                    Assert.Contains(expectedValue, actual[expectedEntry.Key]);
                }
            }
        }
    }
}