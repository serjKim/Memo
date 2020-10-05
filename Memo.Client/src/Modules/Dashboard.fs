namespace Memo.Client.Modules

module Dashboard =
    module MemoListComponent =
        open Memo.Client
        open NodeModules.Linaria
        open Fable.React
        open Fable.React.Props
        module Mui = Fable.MaterialUI.Core
        module MuiProps = Fable.MaterialUI.Props
        open AppResult.Components
        open Memo.Core
        open Memos
        open Auth
        open Memo.Shared.Web.Auth

        let private cardClass = createClass (css, "
            margin-left: 10px
        ")

        let private textClass = createClass (css, "
            overflow: hidden;
            display: -webkit-box;
            -webkit-box-orient: vertical;
            -webkit-line-clamp: 3;
        ")

        let memoItem = FunctionComponent.Of((fun (props: {| Memo: Memo; User: AppUser |}) ->
            Mui.card [
                HTMLAttr.ClassName cardClass
            ] [
                Mui.cardContent [  ] [
                    Mui.typography [
                        MuiProps.TypographyProp.Variant MuiProps.TypographyVariant.H5
                    ] [ str props.Memo.Title ]
                    Mui.typography [
                        MuiProps.TypographyProp.Variant MuiProps.TypographyVariant.Body2
                        MuiProps.Component (ReactElementType.ofHtmlElement "p")
                        HTMLAttr.ClassName textClass
                    ] [ str props.Memo.Text ]
                    Mui.cardActions [] [
                        match props.User with
                        | Authenticated user ->
                            if user.UserId = props.Memo.UserId then
                                Mui.button [
                                    MuiProps.ButtonProp.Size MuiProps.ButtonSize.Small
                                ] [ str "Edit"  ]
                            else
                                nothing
                        | Guest ->
                            nothing
                    ]
                ]
            ]), memoizeWith = equalsButFunctions)

        let private listContainerClass = createClass (css, "
            display: flex;
        ")

        let memoList = FunctionComponent.Of(fun () ->
            let loadedMemos = useMemoList ()
            let user = useAppUser ()
            AppResult.render loadedMemos (fun memos ->
                div [ HTMLAttr.ClassName listContainerClass ]
                    (memos
                    |> List.map (fun memo -> memoItem {| Memo = memo; User = user |})
                    |> Seq.ofList)
                ))
