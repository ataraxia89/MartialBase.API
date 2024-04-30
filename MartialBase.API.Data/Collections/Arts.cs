// <copyright file="Arts.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.Data
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System.Collections.Generic;

using MartialBase.API.Data.Models.EntityFramework;

namespace MartialBase.API.Data.Collections
{
    internal static class Arts
    {
        internal static IEnumerable<Art> GetArts => new[]
                {
                    new Art { Name = "Aikido" },
                    new Art { Name = "Bojutsu" },
                    new Art { Name = "Boxing" },
                    new Art { Name = "Brazilian Jiu-Jitsu" },
                    new Art { Name = "Capoeira" },
                    new Art { Name = "Haidong Gumdo" },
                    new Art { Name = "Hapkido" },
                    new Art { Name = "Hwa Rang Do" },
                    new Art { Name = "Iaido" },
                    new Art { Name = "Jeet Kune Do" },
                    new Art { Name = "Judo" },
                    new Art { Name = "Jujutsu" },
                    new Art { Name = "Karate" },
                    new Art { Name = "Kendo" },
                    new Art { Name = "Kenjutsu" },
                    new Art { Name = "Kenpo" },
                    new Art { Name = "Kickboxing" },
                    new Art { Name = "Krav Maga" },
                    new Art { Name = "Kung Fu" },
                    new Art { Name = "MMA" },
                    new Art { Name = "Muay Thai" },
                    new Art { Name = "Ninjutsu" },
                    new Art { Name = "Sumo" },
                    new Art { Name = "Taekkyeon" },
                    new Art { Name = "Taekwon-Do" },
                    new Art { Name = "Tai Chi" },
                    new Art { Name = "Tang Soo Do" },
                    new Art { Name = "Wing Chun" },
                    new Art { Name = "Won Hwa Do" },
                    new Art { Name = "Wushu" }
                };
    }
}
