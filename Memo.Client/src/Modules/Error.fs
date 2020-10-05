namespace Memo.Client.Modules

module Error =
    open Fable.React

    type ErrorBoundaryProps =
        { Inner : ReactElement }

    type ErrorBoundaryState =
        { HasErrors : bool }

    type [<AllowNullLiteral>] InfoComponentObject =
        abstract componentStack: string with get

    type ErrorBoundary(props) =
        inherit Component<ErrorBoundaryProps, ErrorBoundaryState>(props)
        do base.setInitState({ HasErrors = false })

        override x.componentDidCatch(error, info) =
            let info = info :?> InfoComponentObject
            printfn "Error: %A" error
            printfn "Error Info: %A" info.componentStack
            x.setState(fun _ _ -> { HasErrors = true })

        override x.render() =
            if x.state.HasErrors then
                div [] [ str "Error" ]
            else
                x.props.Inner

    let errorBoundary element =
        ofType<ErrorBoundary,_,_> { Inner = element } [ ]
