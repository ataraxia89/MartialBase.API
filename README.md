## MartialBase-API

### Introduction
MartialBase® is currently a work in progress, and will eventually be released as a commercial product to manage martial arts clubs. This API solution has been used experimentally to practice functional, clean coding as well as R&D such as containerised tests to simulate a live environment.

### Update nuget.config to access required packages
The [nuget.config](https://github.com/ataraxia89/MartialBase.API/blob/main/nuget.config) file contains a placeholder for GitHub credentials, to enable access to GitHub-hosted packages. The packages are public, so any GitHub account can access them. Official GitHub documentation can be found [here](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens), please follow the below steps to obtain a PAT token:

- Click the profile icon in the top right of any GitHub page, then click Settings
- Scroll down and select "Developer settings" in the sidebar to the left
- Go to "Personal access tokens -> Tokens (classic)"
- Any previously-created tokens will appear on this page, from the dropdown at the top select "Generate new token -> Generate new token (classic)"
- Give the token a name, select an expiry time and ensure that it has the `read:packages` permission selected
- Click "Generate token" at the bottom
- The newly-generated token will appear on the list in plaintext, copy this and paste into the `GITHUB_PAT` placeholder in the config file
- Replace the `GITHUB_USER` placeholder with your GitHub username

### Running in debug
The API is configured to use the Swagger UI for ease of debugging. Authorization via JSON tokens is implemented throughout most of the controllers. To authenticate in Swagger, you will need to obtain a JWT token. Please follow these steps:

- Pull the code down and ensure that it is built and run in Debug configuration
- Visit http://localhost:[port]/login on your browser (or send a GET request via an alternative HTTP client such as Postman)
- This will generate a JWT token with super user access (super users are temporary whilst the system is still in development)
- In the Swagger UI, click Authorize
- In the Value field, type "Bearer " followed by the full JWT token
- Click Authorize on the dialog popup, then Close
- All endpoints will now be authorized

### Running tests
The API solution will need to be built in Testing configuration. If running tests which depend on Docker, then the Docker engine will need to be running.

If running on Windows, the easiest way to run the Docker engine is by installing Docker Desktop here >> https://www.docker.com/products/docker-desktop/
