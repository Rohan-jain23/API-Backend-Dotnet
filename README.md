# Framework API

The Framework API is a service acting as a GraphQL server which communicates with various other RUBY backend
services.

It allows you to

- query data
- mutate data
- subscribe to data changes

## Licence Check

## Architecture

- [Licence Check](./LICENCE_CHECK.md)
- [More information](./ARCHITECTURE.md)

## Diagrams

- [Data Flow](./diagrams/Data_Flow.md)

## UI

- [GraphiQL (/wuh/graphiql) (local)](https://localhost:5001/wuh/graphiql)
- [GraphiQL (/wuh/graphiql) (ft1)](https://lx64ispft1.wuh-intern.de/wuh/graphiql)
- [More information](./src/FrameworkAPI/GraphiQL/README.md)

## Cloning this repository

Cloning this repository might fail with the following error:

`error: unable to create file test/FrameworkAPI.test/..: Filename too long`

(if the git config value of `core.longpaths` is set to `false`)

The value can be changed on a global level with the following command:

`git config --system core.longpaths true`

More information: https://stackoverflow.com/questions/52699177/how-to-fix-filename-too-long-error-during-git-clone
