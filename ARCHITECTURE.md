# Architecture

## Architecture Decision Records (ADRs)

The following decision records are based on [this](https://github.com/joelparkerhenderson/architecture-decision-record/tree/main/locales/en/templates/decision-record-template-by-michael-nygard) template:

#### Title

- Status: What is the status, such as proposed, accepted, rejected, deprecated, superseded, etc.?
- Context: What is the issue that we're seeing that is motivating this decision or change?
- Decision: What is the change that we're proposing and/or doing?
- Consequences: What becomes easier or more difficult to do because of this change?

---

### 1. GraphQL

#### 1.1 GraphQL is used to communicate between possible clients and the backend services

- Status: Accepted
- Context: Interacting with various backend services is unnecessarily complicated for our clients.\
  GraphQL is abstracting those backend services away and the clients can fetch exactly the data which they need
- Decision: Stated in the title
- Consequences
  - Pros
    - Universal, single endpoint for our clients
    - Clients can fetch exactly the data which they need
  - Cons
    - Developing a GraphQL API tends to be more difficult (compared to a REST API)
    - Performance can easily get out of hand (we have to take care of that)
    - It will be time-consuming to transition to a REST API if we decide to later on

#### 1.2 HotChocolate is used as a GraphQL server implementation

- Status: Accepted
- Context: A GraphQL server is necessary to provide a GraphQL API
- Decision: Stated in the title
- Consequences
  - Pros
    - It is available for .NET (ASP.NET)
    - The [monorepo](https://github.com/ChilliCream/graphql-platform) (also includes other products) has 4.8k stars on GitHub (12.02.2024)
    - It is easy to use
    - We can get support from the company behind it
  - Cons
    - It is a vendor lock-in. Transitioning to a different GraphQL server implementation might be time-consuming

#### 1.3 `BatchDataLoader`s are mainly used when fetching entities within a request

- Status: Accepted
- Context: There are various different data loaders available. It might not be easy to pick one
- Decision: Stated in the title. Also recommended by the company behind HotChocolate:
  > CacheDataLoader:<br>No batching, just caching. This data loader is used rarely. You most likely want to use the batch data loader.<br><br>More information:<br>https://chillicream.com/docs/hotchocolate/v13/fetching-data/dataloader
- Consequences
  - Pros
    - A single (batch) data loader can be used when fetching a single entity or multiple entities from a backend service
    - It is also caching the entities within a request
  - Cons
    - More difficult to implement (compared to a `CacheDataLoader`)

#### 1.4 Exceptions which are thrown while resolving fields are catched and then rethrown within the resolver method

- Status: Accepted
- Context: Exceptions can occur and we want to resolve as much as possible when handling a request.\
  An unhandled exception which is thrown within the call tree of a resolver method (service, data handler, etc.) can result in not returning fields at all
- Decision: Stated in the title
- Consequences
  - Pros
    - We can resolve as much as possible
  - Cons
    - A correct handling can be difficult. Even a single, not handled exception can result in not returning fields at all

#### 1.5 Exception details are always included in GraphQL responses

- Status: Accepted
- Context: It is hard to deal with generic errors in production
- Decision: Stated in the title
- Consequences
  - Pros
    - We can pinpoint what the issue is
  - Cons
    - We share internals (file/method names, line numbers, etc.)

### 2. GraphiQL

#### 2.1 GraphiQL is used as a GraphQL UI

- Status: Accepted
- Context: Having a UI for sending queries or subscribing to data changes helps a lot
- Decision: Stated in the title
- Consequences
  - Pros
    - We maintain the solution which enables us to perform updates whenever we feel like it
  - Cons
    - We have to maintain the solution ([more information](./src/FrameworkAPI/GraphiQL/README.md))
