// <copyright file="CommonMemberData.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API.LiveControllerTests
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using MartialBase.API.Models.DTOs.Addresses;
using MartialBase.API.Models.DTOs.People;

namespace MartialBase.API.LiveControllerTests.TestTools
{
    public static class CommonMemberData
    {
        public static IEnumerable<object[]> InvalidCreateAddressDTOs
        {
            get
            {
                object[] line1Missing =
                {
                    new CreateAddressDTO
                    {
                        Town = "Test Town",
                        County = "Test County",
                        PostCode = "TS01 PCD",
                        CountryCode = "TST"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Line1", new[] { "Line 1 required." } }
                    }
                };

                object[] townMissing =
                {
                    new CreateAddressDTO
                    {
                        Line1 = "Test Line 1",
                        County = "Test County",
                        PostCode = "TS01 PCD",
                        CountryCode = "TST"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Town", new[] { "Town required." } }
                    }
                };

                object[] countyMissing =
                {
                    new CreateAddressDTO
                    {
                        Line1 = "Test Line 1",
                        Town = "Test Town",
                        PostCode = "TS01 PCD",
                        CountryCode = "TST"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "County", new[] { "County required." } }
                    }
                };

                object[] postCodeMissing =
                {
                    new CreateAddressDTO
                    {
                        Line1 = "Test Line 1",
                        Town = "Test Town",
                        County = "Test County",
                        CountryCode = "TST"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "PostCode", new[] { "Postcode required." } }
                    }
                };

                object[] countryMissing =
                {
                    new CreateAddressDTO
                    {
                        Line1 = "Test Line 1",
                        Town = "Test Town",
                        County = "Test County",
                        PostCode = "TS01 PCD"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "CountryCode", new[] { "Country code required." } }
                    }
                };

                object[] line1TooLong =
                {
                    new CreateAddressDTO
                    {
                        Line1 = new string('*', 51),
                        Town = "Test Town",
                        County = "Test County",
                        PostCode = "TS01 PCD",
                        CountryCode = "TST"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Line1", new[] { "Line 1 cannot be longer than 50 characters." } }
                    }
                };

                object[] line2TooLong =
                {
                    new CreateAddressDTO
                    {
                        Line1 = "Test Line 1",
                        Line2 = new string('*', 31),
                        Town = "Test Town",
                        County = "Test County",
                        PostCode = "TS01 PCD",
                        CountryCode = "TST"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Line2", new[] { "Line 2 cannot be longer than 30 characters." } }
                    }
                };

                object[] line3TooLong =
                {
                    new CreateAddressDTO
                    {
                        Line1 = "Test Line 1",
                        Line3 = new string('*', 31),
                        Town = "Test Town",
                        County = "Test County",
                        PostCode = "TS01 PCD",
                        CountryCode = "TST"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Line3", new[] { "Line 3 cannot be longer than 30 characters." } }
                    }
                };

                object[] townTooLong =
                {
                    new CreateAddressDTO
                    {
                        Line1 = "Test Line 1",
                        Town = new string('*', 81),
                        County = "Test County",
                        PostCode = "TS01 PCD",
                        CountryCode = "TST"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Town", new[] { "Town cannot be longer than 80 characters." } }
                    }
                };

                object[] countyTooLong =
                {
                    new CreateAddressDTO
                    {
                        Line1 = "Test Line 1",
                        Town = "Test Town",
                        County = new string('*', 31),
                        PostCode = "TS01 PCD",
                        CountryCode = "TST"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "County", new[] { "County cannot be longer than 30 characters." } }
                    }
                };

                object[] postCodeTooLong =
                {
                    new CreateAddressDTO
                    {
                        Line1 = "Test Line 1",
                        Town = "Test Town",
                        County = "Test County",
                        PostCode = new string('*', 9),
                        CountryCode = "TST"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "PostCode", new[] { "Postcode cannot be longer than 8 characters." } }
                    }
                };

                object[] countryTooLong =
                {
                    new CreateAddressDTO
                    {
                        Line1 = "Test Line 1",
                        Town = "Test Town",
                        County = "Test County",
                        PostCode = "TS01 PCD",
                        CountryCode = new string('*', 4)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "CountryCode", new[] { "Country code cannot be longer than 3 characters." } }
                    }
                };

                object[] landlinePhoneTooLong =
                {
                    new CreateAddressDTO
                    {
                        Line1 = "Test Line 1",
                        Town = "Test Town",
                        County = "Test County",
                        PostCode = "TS01 PCD",
                        CountryCode = "TST",
                        LandlinePhone = new string('*', 31)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "LandlinePhone", new[] { "Landline phone cannot be longer than 30 characters." } }
                    }
                };

                return new[]
                {
                    line1Missing,
                    townMissing,
                    countyMissing,
                    postCodeMissing,
                    countryMissing,
                    line1TooLong,
                    line2TooLong,
                    line3TooLong,
                    townTooLong,
                    countyTooLong,
                    postCodeTooLong,
                    countryTooLong,
                    landlinePhoneTooLong
                };
            }
        }

        public static IEnumerable<object[]> InvalidUpdateAddressDTOs
        {
            get
            {
                object[] line1TooLong =
                {
                    new UpdateAddressDTO()
                    {
                        Line1 = new string('*', 51)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Line1", new[] { "Line 1 cannot be longer than 50 characters." } }
                    }
                };

                object[] line2TooLong =
                {
                    new UpdateAddressDTO
                    {
                        Line2 = new string('*', 31)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Line2", new[] { "Line 2 cannot be longer than 30 characters." } }
                    }
                };

                object[] line3TooLong =
                {
                    new UpdateAddressDTO
                    {
                        Line3 = new string('*', 31)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Line3", new[] { "Line 3 cannot be longer than 30 characters." } }
                    }
                };

                object[] townTooLong =
                {
                    new UpdateAddressDTO
                    {
                        Town = new string('*', 81)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Town", new[] { "Town cannot be longer than 80 characters." } }
                    }
                };

                object[] countyTooLong =
                {
                    new UpdateAddressDTO
                    {
                        County = new string('*', 31)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "County", new[] { "County cannot be longer than 30 characters." } }
                    }
                };

                object[] postCodeTooLong =
                {
                    new UpdateAddressDTO
                    {
                        PostCode = new string('*', 9)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "PostCode", new[] { "Postcode cannot be longer than 8 characters." } }
                    }
                };

                object[] countryTooLong =
                {
                    new UpdateAddressDTO
                    {
                        CountryCode = new string('*', 4)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "CountryCode", new[] { "Country code cannot be longer than 3 characters." } }
                    }
                };

                object[] landlinePhoneTooLong =
                {
                    new UpdateAddressDTO
                    {
                        LandlinePhone = new string('*', 31)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "LandlinePhone", new[] { "Landline phone cannot be longer than 30 characters." } }
                    }
                };

                return new[]
                {
                    line1TooLong,
                    line2TooLong,
                    line3TooLong,
                    townTooLong,
                    countyTooLong,
                    postCodeTooLong,
                    countryTooLong,
                    landlinePhoneTooLong
                };
            }
        }

        public static IEnumerable<object[]> InvalidCreatePersonDTOs
        {
            get
            {
                object[] firstNameMissing =
                {
                    new CreatePersonDTO
                    {
                        LastName = "Smith"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "FirstName", new[] { "First name required." } }
                    }
                };

                object[] lastNameMissing =
                {
                    new CreatePersonDTO
                    {
                        FirstName = "John"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "LastName", new[] { "Last name required." } }
                    }
                };

                object[] titleTooLong =
                {
                    new CreatePersonDTO
                    {
                        Title = new string('*', 16),
                        FirstName = "John",
                        LastName = "Smith"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Title", new[] { "Title cannot be longer than 15 characters." } }
                    }
                };

                object[] firstNameTooLong =
                {
                    new CreatePersonDTO
                    {
                        FirstName = new string('*', 16),
                        LastName = "Smith"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "FirstName", new[] { "First name cannot be longer than 15 characters." } }
                    }
                };

                object[] middleNameTooLong =
                {
                    new CreatePersonDTO
                    {
                        FirstName = "John",
                        MiddleName = new string('*', 36),
                        LastName = "Smith"
                    },
                    new Dictionary<string, string[]>
                    {
                        { "MiddleName", new[] { "Middle name cannot be longer than 35 characters." } }
                    }
                };

                object[] lastNameTooLong =
                {
                    new CreatePersonDTO
                    {
                        FirstName = "John",
                        LastName = new string('*', 26)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "LastName", new[] { "Last name cannot be longer than 25 characters." } }
                    }
                };

                object[] mobileNoTooLong =
                {
                    new CreatePersonDTO
                    {
                        FirstName = "John",
                        LastName = "Smith",
                        MobileNo = new string('*', 31)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "MobileNo", new[] { "Mobile no. cannot be longer than 30 characters." } }
                    }
                };

                object[] emailTooLong =
                {
                    new CreatePersonDTO
                    {
                        FirstName = "John",
                        LastName = "Smith",
                        Email = new string('*', 46)
                    },
                    new Dictionary<string, string[]>
                    {
                        { "Email", new[] { "Email cannot be longer than 45 characters." } }
                    }
                };

                string[] invalidDateStrings =
                {
                    "NotADate",
                    DateTime.Now.ToString("s"), // 2008-10-04T16:03:05
                    DateTime.Now.ToString("D"), // Thursday, 10 April 2008
                    DateTime.Now.ToString("d"), // 04/10/2008
                    DateTime.Now.ToString("F"), // Thursday, 10 April 2008 16:03:05
                    DateTime.Now.ToString("f"), // Thursday, 10 April 2008 16:03
                    DateTime.Now.ToString("G"), // 04/10/2008 16:03:00
                    DateTime.Now.ToString("g"), // 04/10/2008 16:03
                    DateTime.Now.ToString("M"), // April 10
                    DateTime.Now.ToString("O"), // 2008-04-10T16:03:05.0000000
                    DateTime.Now.ToString("R"), // Thu, 10 Apr 2008 16:03:05 GMT
                    DateTime.Now.ToString("T"), // 16:03:05
                    DateTime.Now.ToString("t"), // 16:03
                    DateTime.Now.ToString("U"), // Thursday, 10 April 2008 16:03:05
                    DateTime.Now.ToString("u"), // 2008-04-10 16:03:05Z
                    DateTime.Now.ToString("Y"), // 2008 April
                    DateTime.Now.ToString("M/d/yy"), // 4/10/08
                    DateTime.Now.ToString("yy-MM-dd"), // 08/04/10
                    DateTime.Now.ToString("MM/dd/yyyy"), // 04/10/2008
                    DateTime.Now.ToString("yyyy MMMM dd"), // 2008 April 10
                    DateTime.Now.ToString("yy-MMM-dd ddd"), // 08-Apr-10 Tue
                    DateTime.Now.ToString("yyyy-M-d dddd"), // 2008-4-10 Tuesday
                    DateTime.Now.ToString("hh:mm:ss t z"), // 04:03:05 P -7
                    DateTime.Now.ToString("h:mm:ss tt zz"), // 4:03:05 PM -07
                    DateTime.Now.ToString("HH:mm:ss tt zz"), // 16:03:05 PM -07
                    DateTime.Now.ToString("HH:m:s tt zzz"), // 16:3:5 PM -07:00
                };

                var invalidCreatePersonDTOs = new List<object[]>
                {
                    firstNameMissing,
                    lastNameMissing,
                    titleTooLong,
                    firstNameTooLong,
                    middleNameTooLong,
                    lastNameTooLong,
                    mobileNoTooLong,
                    emailTooLong
                };

                foreach (string invalidDateString in invalidDateStrings)
                {
                    object[] invalidDate =
                    {
                        new CreatePersonDTO
                        {
                            FirstName = "John",
                            LastName = "Smith",
                            DateOfBirth = invalidDateString
                        },
                        new Dictionary<string, string[]>
                        {
                            { "DateOfBirth", new[] { "Invalid date format. Ensure date is submitted as yyyy-mm-dd." } }
                        }
                    };

                    invalidCreatePersonDTOs.Add(invalidDate);
                }

                foreach (object[] invalidCreatePersonAddressDTO in InvalidCreateAddressDTOs)
                {
                    var invalidAddressErrors = new Dictionary<string, string[]>();

                    foreach (KeyValuePair<string, string[]> addressError in
                        (Dictionary<string, string[]>)invalidCreatePersonAddressDTO[1])
                    {
                        invalidAddressErrors.Add($"Address.{addressError.Key}", addressError.Value);
                    }

                    object[] invalidAddress =
                    {
                        new CreatePersonDTO
                        {
                            FirstName = "John",
                            LastName = "Smith",
                            Address = (CreateAddressDTO)invalidCreatePersonAddressDTO[0]
                        },
                        invalidAddressErrors
                    };

                    invalidCreatePersonDTOs.Add(invalidAddress);
                }

                return invalidCreatePersonDTOs;
            }
        }
    }
}
