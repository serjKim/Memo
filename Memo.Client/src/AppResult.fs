namespace Memo.Client.AppResult

module Types =
    open Thoth.Fetch

    type AppError =
        | ApiCallFailed of FetchError

    type AppResult<'T> = Result<'T, AppError>

module Components =
    [<RequireQualifiedAccess>]
    module AppResult =
        open Fable.Core
        open Fable.React
        open Memo.Client.NodeModules.Notistack
        open Types

        let renderError' = FunctionComponent.Of(fun (props: {| appError: AppError |}) ->
            let snackbar = useSnackbar ()
            Hooks.useEffect((fun () ->
                match props.appError with
                | ApiCallFailed err ->
                    JS.console.error err
                    snackbar.enqueueSnackbar (sprintf "Error has occured during api call: %A" err,
                                              { variant = VariantType.Error }) |> ignore), [| props.appError |])
            nothing)

        let inline renderError err = renderError' {| appError = err |}

        let render (appResult: AppResult<'a>)
                   (okRenderer: 'a -> ReactElement) : ReactElement =
            match appResult with
            | Ok x -> okRenderer x
            | Error err -> renderError err