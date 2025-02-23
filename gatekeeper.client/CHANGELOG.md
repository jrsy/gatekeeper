This file explains how Visual Studio created the project.

The following tools were used to generate this project:
- create-vite

The following steps were used to generate this project:
- Create vue project with create-vite: `npm init --yes vue@latest gatekeeper.client -- --eslint `.
- Update `vite.config.js` to set up proxying and certs.
- Update `HelloWorld` component to fetch and display weather information.
- Create project file (`gatekeeper.client.esproj`).
- Create `launch.json` to enable debugging.
- Add project to solution.
- Update proxy endpoint to be the backend server endpoint.
- Add project to the startup projects list.
- Write this file.
