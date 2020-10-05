namespace Memo.Client

module Routing =
    open Fable.Core
    open Fable.Core.JsInterop
    open NodeModules.Router5

    [<StringEnum>]
    type RouteName =
        | Dashboard
        | Github

    let extractRouteName (ctx: IRouteContext) : RouteName =
        !!ctx.route.name.Split([| '.' |]).[0]

    let routes =
        [| { name = "dashboard"
             path = "/dashboard" }
           { name = "github"
             path = "/github" } |]

    let configureRouter () =
        let router = createRouter routes {| defaultRoute = "dashboard" |}
        router.usePlugin (browserPlugin()) |> ignore
        router

    let router = configureRouter ()
