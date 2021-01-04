namespace Memo.Client.Api

module Types =
    open Fable.Core
    open Memo.Client.AppResult.Types

    type AppPromise<'T> = JS.Promise<AppResult<'T>>

    (*
        open NodeModules.Rxjs.Operators
        open NodeModules.Rxjs.Fetch
        
        Rx-way:
        let inline private decoder<'T> = Decode.Auto.generateDecoderCached<'T> ()
        let inline deserialize<'T> json = Decode.fromString decoder<'T> json

        let getJson (url: string) =
            From.Fetch (url, {| selector = fun (res: Response) -> res.text() |})

        let postJson url (body: obj) (headers: obj) =
            From.Fetch (url, { body = JSON.stringify body; method = "POST"; headers = headers  }) |> mergeMapPromise (fun x _ -> x.text())
    *)

[<RequireQualifiedAccess>]
module Csrf =
    open Fable.Core
    open Fetch.Types
    open Memo.Shared.Web.Csrf
    open Thoth.Fetch

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

module Methods =
    open Fable.Core
    open Fetch.Types
    open Thoth.Fetch
    open Thoth.Json
    open Memo.Client.AppResult.Types
    open Types

    let extraDecoders =
        Extra.empty
        |> Extra.withInt64

    type Methods =
        static member inline tryGet<'T> (url: string) : AppPromise<'T> =
            Fetch.tryGet (url, extra = extraDecoders)
            |> Promise.mapResultError ApiCallFailed

        static member inline tryPost<'T> (url: string, ?data: obj, ?headers: HttpRequestHeaders list) : AppPromise<'T> =
            Csrf.withToken (fun headers' ->
                Fetch.tryPost (url = url,
                               data = (match data with
                                      | Some d -> d
                                      | None -> JS.undefined),
                               extra = extraDecoders,
                               headers = match headers with
                                         | Some h -> headers' @ h |> List.distinct
                                         | None -> headers')
                |> Promise.mapResultError ApiCallFailed)
