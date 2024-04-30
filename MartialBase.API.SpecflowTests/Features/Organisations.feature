Feature: Organisations

A suite of tests to cover organisation management

@organisations
Scenario: User can create an organisation and add members to it
	Given the current user does not belong to any organisation
	When the current user creates the organisation "UK Specflow Association"
    And the current user requests a list of all organisations
	Then the response contains the created organisation "UK Specflow Association"
    When the current user creates a new person record for "Jason Statham" under organisation "UK Specflow Association"
    And the current user requests a list of organisation people for "UK Specflow Association"
    Then the created person "Jason Statham" is in the list of requested organisation people
