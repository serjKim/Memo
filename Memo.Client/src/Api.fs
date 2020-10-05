namespace Memo.Client

module Api =
    open Fable.Core
    open Thoth.Json
    open NodeModules.Rxjs.Operators
    open NodeModules.Rxjs.Fetch
    open Memo.Client.AppResult

    type AppPromise<'T> = JS.Promise<AppResult<'T>>

    (*
        Rx-way:
        let inline private decoder<'T> = Decode.Auto.generateDecoderCached<'T> ()
        let inline deserialize<'T> json = Decode.fromString decoder<'T> json

        let getJson (url: string) =
            From.Fetch (url, {| selector = fun (res: Response) -> res.text() |})

        let postJson url (body: obj) (headers: obj) =
            From.Fetch (url, { body = JSON.stringify body; method = "POST"; headers = headers  }) |> mergeMapPromise (fun x _ -> x.text())
    *)

    let extraDecoders =
        Extra.empty
        |> Extra.withInt64

    [<RequireQualifiedAccess>]
    module Csrf =
        open Thoth.Fetch
        open Fetch.Types
        open Memo.Shared.Web.Csrf

        type CSRFTokenStore() =
            let mutable currentToken = Option<JS.Promise<CsrfToken>>.None
            member _.CurrentToken() =
                if currentToken.IsNone then
                    currentToken <- Some <| promise {
                        match! Fetch.tryGet "api/csrfToken" with
                        | Ok tkn ->
                            return tkn
                        | Error _ ->
                            JS.console.log (sprintf "Couldn't load a CSRF token")
                            return CsrfToken.Token ""
                    }
                currentToken

        let tokenStore = CSRFTokenStore()

        let withToken (f: HttpRequestHeaders list -> JS.Promise<'a>) : JS.Promise<'a> =
            promise {
                let! token = (tokenStore.CurrentToken() |> Option.get)
                let strToken = stringifyToken token
                let headers = [HttpRequestHeaders.Custom ("RequestVerificationToken", strToken)]
                return! f headers
            }
