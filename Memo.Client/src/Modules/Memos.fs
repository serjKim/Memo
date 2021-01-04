namespace Memo.Client.Modules

module Memos =
    [<RequireQualifiedAccess>]
    module private MemosApi =
        open Memo.Client.Api.Types
        open Memo.Client.Api.Methods
        open Memo.Core

        let fetchAllMemos () : AppPromise<Memo list> =
            Methods.tryGet "/api/memos"

    open Fable.React.HookBindings
    open Memo.Client.AppResult.Types
    open Memo.Core

    let useMemoList () : AppResult<Memo list> =
        let loadedMemos = Hooks.useState (Ok [])
        Hooks.useEffect((fun () ->
            promise {
                let! memos = MemosApi.fetchAllMemos ()
                loadedMemos.update memos
            } |> ignore), [||])
        loadedMemos.current
