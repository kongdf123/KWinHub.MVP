For Kunwei's Hub platform, I recommend separating the frontend and backend into different projects within the same solution/repository, at least for the main web application.

The key distinction is that you should separate them at the project and architectural levels, but you do not necessarily need separate Git repositories, separate development teams, or separate deployments from day one.

Given the architecture we've discussed for Kunwei — a centralized Hub, multiple hardware devices, desktop/iPad client applications, and reusable sports-science calculations — I'd organize the code around clear responsibilities and stable interfaces.
1. Recommended architecture
Frontend applications

Hub Web UI · Desktop App · iPad App

Presentation, user interaction, local device integration

HTTPS / REST API · WebSocket when needed

Backend API

Authentication · Business logic · Device registry · Data ingestion

Authorization, validation, workflows, API contracts


Data layer

Hub database, athlete profiles, organizations, device metadata


Domain services

Sports-science calculations, test results, reports, integrations




Device and acquisition layer

Force plates · Timing gates · Cameras · Desktop acquisition software

Local network, hardware SDKs, buffered uploads, device status


The most important architectural decision is this: the frontend calls the backend through a defined API, rather than accessing the database directly. Desktop and iPad clients can use the same API contracts as the web application, even when they have additional local hardware responsibilities.

One qualification: a desktop acquisition application may need to communicate directly with local devices for low-latency data capture. That is a separate concern from how it synchronizes results with the Hub.
2. Should you create two separate projects?
Yes. For the main Hub application, start with at least these two projects.
Project	Responsibility	Example technology
Kunwei.Hub.Web	Web UI, pages, dashboards, forms	React + TypeScript
Kunwei.Hub.Api	HTTP endpoints, authentication, API orchestration	ASP.NET Core Web API
I would also add supporting projects as the codebase grows, rather than putting all backend functionality into one large project.
3. Suggested project and folder structure
For Kunwei, I'd use a monorepo initially: one Git repository containing the frontend, backend, shared contracts, and related documentation.

For a .NET-based backend, the structure could look like this:

Kunwei.Hub/
├── docs/
│   ├── architecture/
│   ├── api/
│   └── product-requirements/
│
├── src/
│   ├── frontend/
│   │   └── Kunwei.Hub.Web/
│   │       ├── src/
│   │       │   ├── pages/
│   │       │   ├── components/
│   │       │   ├── features/
│   │       │   ├── services/
│   │       │   └── types/
│   │       ├── public/
│   │       └── package.json
│   │
│   ├── backend/
│   │   ├── Kunwei.Hub.Api/
│   │   ├── Kunwei.Hub.Application/
│   │   ├── Kunwei.Hub.Domain/
│   │   ├── Kunwei.Hub.Infrastructure/
│   │   └── Kunwei.Hub.Contracts/
│   │
│   ├── clients/
│   │   ├── Kunwei.Hub.Desktop/
│   │   └── Kunwei.Hub.Mobile/
│   │
│   └── tests/
│       ├── Kunwei.Hub.UnitTests/
│       ├── Kunwei.Hub.IntegrationTests/
│       └── Kunwei.Hub.ApiTests/
│
├── Kunwei.Hub.sln
├── README.md
└── .gitignore



This is a target structure, not a requirement to create every project immediately. For the first version, you can omit the mobile project, separate test projects, and even some backend class libraries until they provide real value.
What each backend project does
Project	Responsibility
Kunwei.Hub.Api	HTTP endpoints, authentication middleware, request/response handling
Kunwei.Hub.Application	Use cases, orchestration, application services
Kunwei.Hub.Domain	Core business entities, domain rules and invariants
Kunwei.Hub.Infrastructure	Database access, file storage, external API integrations
Kunwei.Hub.Contracts	API request/response DTOs and shared contract definitions
For example, when a user registers a device:
1. The frontend submits a request to POST /api/v1/devices.
2. The API validates the request and identifies the authenticated user.
3. The application layer executes the registration use case.
4. The infrastructure layer persists the device information.
5. The API returns the result to the frontend.
The frontend does not need to know how the device record is stored in the database.
4. Keep the Hub frontend separate from the desktop acquisition application
This distinction is particularly important for Kunwei because your existing products include force plates, Basler cameras, timing equipment, and specialized measurement workflows.

I would separate the applications by responsibility.

A. Hub Web frontend

React + TypeScript
- Organization and user management
- Athlete profiles and historical records
- Device registry and status dashboards
- Test-result review and reports
- Permissions and platform configuration




B. Desktop acquisition application

Your existing WPF/.NET application, where applicable
- Connect to local measurement devices
- Capture high-frequency force data
- Control camera acquisition and synchronization
- Run latency-sensitive processing
- Buffer test data and upload results to the Hub




C. Hub backend

ASP.NET Core Web API
- Accept device and test-result uploads
- Maintain shared athlete and organization records
- Manage authorization and device registration
- Provide data for web dashboards and reports



I would not rewrite your existing desktop acquisition software as part of the first Hub frontend project. Instead, integrate it incrementally through the Hub API.

For example, a desktop client can authenticate, obtain its assigned configuration, capture a test locally, and upload the completed session. For high-frequency raw data, use chunked uploads or a dedicated ingestion mechanism rather than sending every sample through ordinary request/response calls.
5. How should the frontend and backend be developed together?
Use an API-first workflow, even if you're developing both sides yourself.
1. Define the feature and data model. For example, device registration or athlete management.
2. Define the API contract. Specify endpoint paths, request/response DTOs, validation rules, and error responses.
3. Develop the frontend and backend independently. The frontend can use mock responses while the backend implements its endpoints.
4. Integrate and test. Verify authorization, data validation, error handling, and complete user workflows.
5. Deploy and version. Keep the API compatible with the desktop clients and other applications that consume it.
For ASP.NET Core, use OpenAPI to document the endpoints. You can then generate TypeScript client code for the React frontend, reducing inconsistencies between frontend models and backend DTOs.

One useful rule: share API contracts, not database entities. The frontend should consume API DTOs, rather than sharing Entity Framework models or directly referencing backend domain classes.
6. Should frontend and backend be separate Git repositories?
For your first Hub release, I recommend a single repository.
Consideration	One repository	Separate repositories
Initial development	Simpler	More setup
Cross-layer changes	One pull request can cover both	Changes span repositories
Shared API contracts	Easier to coordinate	Requires versioned packages or a dedicated contract workflow
Independent deployment	Fully supported	Fully supported
Independent team ownership	Suitable initially	Useful for larger teams
Separate repositories become more attractive when the teams, release cycles, access controls, or deployment pipelines genuinely need to be independent. You do not need them simply because the frontend and backend use different languages.

Also, separate projects do not require separate servers or deployments. In the initial release, you could serve the compiled React application through a web server or CDN and deploy the ASP.NET Core API as a separate service. You can evolve the hosting arrangement later.
7. My recommendation for Kunwei's first version
Start with this manageable setup:
- One Git repository: Kunwei.Hub
- One solution: Kunwei.Hub.sln
- One frontend project: Kunwei.Hub.Web
- One backend API project: Kunwei.Hub.Api
- A small number of backend class libraries where clear boundaries justify them
- One Hub database, accessed exclusively through the backend
- A versioned REST API for the desktop and future mobile clients
- OpenAPI-generated TypeScript clients and automated API tests
Keep the sports-science calculation engine as a separate module or library when it needs to be reused across products. It does not necessarily need to become a separate network service.

This gives you a clean separation of concerns without introducing premature microservices, excessive repositories, or complicated deployment infrastructure.

One implementation detail would help refine this design: are you planning to use React for the Hub web frontend and ASP.NET Core for the backend, while retaining your existing WPF desktop applications?
My answer is: currently we want to use the Vue for frontend, ASP.NET Core for backend