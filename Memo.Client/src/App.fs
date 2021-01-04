namespace Memo.Client

module App =
    open Fable.Import
    open Browser.Dom
    open Fable.React
    open NodeModules.Router5
    open NodeModules.Notistack
    open Routing
    open Modules.Auth.Hooks
    open Modules.Layout
    open Modules.Github
    open AppResult.Components

    router.start(fun () ->
        let app = FunctionComponent.Of(fun () ->
            let items,searchByTerm = Search.useSearch ()
            let sign = useSign ()

            Hooks.useEffect((fun () -> sign.Dispatch SilentSign), [||])

            snackbarProvider { maxSnack = 10; preventDuplicate = true } [
                routerProvider { router = router } [
                    AppResult.render sign.AppUser (fun user ->
                        authProvider user [
                            ReactElementType.create MainLayout.mainLayout {
                                Content = MainContent.mainContent {| SearchByTerm = searchByTerm; Items = items  |}
                                NavBar = NavBar.navBar {| Items = items |}
                                ToolbarButtons = AppToolBar.toolbarButtons {| Dispatch = sign.Dispatch |}
                            } []
                        ])
                ]
            ])
        ReactDom.render (app (), document.getElementById "memo-app")
    ) |> ignore

