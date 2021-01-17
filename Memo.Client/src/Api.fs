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
    open System.Text.RegularExpressions
    open Fetch.Types
    open Memo.Shared.Web.Csrf

    let extractToken (cookie: string) =
        let r = Regex(apiCsrfCookieName + "=(.+)")
        let cookiesByName = cookie.Split ([| ';' |])
        let result = Array.choose (fun x ->
                let m = r.Match x
                if m.Success then
                    let g = m.Groups.[1]
                    Some g.Value
                else
                    None) cookiesByName
        result.[0]

    let appendToken (Token token)
                     (headers: HttpRequestHeaders list) : HttpRequestHeaders list =
        HttpRequestHeaders.Custom ("RequestVerificationToken", token) :: headers |> List.distinct

    let csrfToken = Token (extractToken Browser.Dom.document.cookie)

    let appendCurrentToken : HttpRequestHeaders list -> HttpRequestHeaders list = appendToken csrfToken

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
            Fetch.tryPost (url = url,
                           data = (match data with
                                  | Some d -> d
                                  | None -> JS.undefined),
                           extra = extraDecoders,
                           headers = match headers with
                                     | Some hdrs -> Csrf.appendCurrentToken hdrs
                                     | None -> Csrf.appendCurrentToken [])
            |> Promise.mapResultError ApiCallFailed
