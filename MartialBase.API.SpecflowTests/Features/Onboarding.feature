Feature: Onboarding

A suite of tests to cover the onboarding process for new students

@onboarding
Scenario: New student is assigned to a school
    Given there is an existing school on system called "Specflow Martial Arts"
    And there is an existing person on system called "John Wick"
    When existing person "John Wick" is added to existing school "Specflow Martial Arts"
    Then student "John Wick" is returned in the list of school students of "Specflow Martial Arts"

Scenario: New student signs up for MartialBase with an invitation code and is automatically assigned to a school
    Given there is an existing school on system called "Specflow Martial Arts"
    When a new person "John Wick" is created under existing school "Specflow Martial Arts"
    Then the created person "John Wick" has an invitation code
    When the current user's token contains the generated invitation code for "John Wick" and a new Azure ID
    And the current user requests a list of all schools
    Then the school "Specflow Martial Arts" is in the list of requested schools

