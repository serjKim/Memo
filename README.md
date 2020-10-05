# Memo

Demo F# Fable + Giraffe

## Requirements

* [dotnet SDK](https://www.microsoft.com/net/download/core) 3.1 or higher
* [node.js](https://nodejs.org) with [npm](https://www.npmjs.com/)
* An F# editor like Visual Studio, Visual Studio Code with [Ionide](http://ionide.io/) or [JetBrains Rider](https://www.jetbrains.com/rider/).

## Building and running the app

```cmd
dotnet run
npm i
npm start
```

* After the first compilation is finished, in your browser open: http://localhost:8080/

## Checklist
* Frontend
    * SM: Hooks + Context
    * Routing: Router5
        - [x] Basic routing
        - [ ] Gruards
    * Styles:
        - Material UI (+JSS)
        - Static: Linaria + SCSS
        - Dynamic: Emotion
* Backend
    * Giraffe
    * Authentication:
        - [x] Cookie-based
    * [ ] Authorization
    * [ ] Database
    * [x] CSRF token
        - prodvied by the api, can be rendered with index.html
          but webpack-dev-server should support this
    * Memo
        - [x] Get all
        - [ ] Edit
