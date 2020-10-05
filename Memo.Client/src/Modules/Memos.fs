namespace Memo.Client.Modules

module Memos =
    [<RequireQualifiedAccess>]
    module private MemosApi =
        open Thoth.Fetch
        open Memo.Core
        open Memo.Client.AppResult
        open Memo.Client.Api

        let fetchAllMemos () : AppPromise<Memo list> =
            Fetch.tryGet ("/api/memos", extra = extraDecoders)
            |> Promise.mapResultError ApiCallFailed

    open Fable.React.HookBindings
    open Memo.Client.AppResult
    open Memo.Core

    let useMemoList () : AppResult<Memo list> =
        let loadedMemos = Hooks.useState (Ok [])
        Hooks.useEffect((fun () ->
            promise {
                let! memos = MemosApi.fetchAllMemos ()
                loadedMemos.update memos
            } |> ignore), [||])
        loadedMemos.current
