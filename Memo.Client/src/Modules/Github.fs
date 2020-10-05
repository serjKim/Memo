namespace Memo.Client.Modules

module Github =
    open Memo.Client

    module Search =
        open Fable.React
        open NodeModules.Rxjs
        open NodeModules.Rxjs.Operators
        open NodeModules.Rxjs.Ajax

        module GitHubResponse =
            type RepoItem =
                { name: string }

            type Repo =
                { total_count: int
                  items: RepoItem[] }

        open GitHubResponse

        let fetchRepos term _ =
            ajax.Get<Repo> ("https://api.github.com/search/repositories?q=" + term)
            |> catchError (fun _ _ ->
                { new IAjaxResponse<Repo> with
                    member _.response = { total_count = 0; items = [||] }
                    member _.responseText = "" } |> ``of`` )

        let mapRepos response =
            response.items
            |> Seq.map (fun item -> item.name)
            |> Seq.toArray

        let useSearch () =
            let searchTerm = Hooks.useMemo((fun () -> Subject.create ()), [||])
            let searchResult = Hooks.useState ([||])
            Hooks.useEffectDisposable(fun () ->
                searchTerm
                |> debounceTime 100
                |> switchMap fetchRepos
                |> map (fun res -> res.response)
                |> map mapRepos
                |> subscribeDisposable searchResult.update
            ,[| searchTerm |]);
            (searchResult.current, fun x -> searchTerm.Next(x))

    module ListComponent =
        open Fable.React
        open Fable.React.Props
        open Fable.MaterialUI.Props
        open NodeModules.Linaria
        module Mui = Fable.MaterialUI.Core

        type ListAction =
        | Input of term:string
        | Echo of string

        let reposStyle = createClass (css, "
            p {
                border-bottom: 1px solid #eee;
            }
        ")

        let list = FunctionComponent.Of(fun (props: {| SearchByTerm: string -> unit; Items: string array |}) ->
            div []
                [
                    Mui.textField
                        [ HTMLAttr.Label "Search"
                          TextFieldProp.Variant TextFieldVariant.Outlined
                          OnInput (fun e -> props.SearchByTerm e.Value )][]
                    div [ ClassName reposStyle ]
                        (props.Items
                        |> Seq.map(fun item -> p [] [str item])
                        |> Seq.toList)
                ]
            )

        