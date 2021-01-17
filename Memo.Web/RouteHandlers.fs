namespace Memo.Web

module RouteHandlers =
    open Giraffe
    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open System.Threading.Tasks

    let ok obj = Successful.ok (json obj)
    
    let toResponse okResult = function
        | Ok x -> okResult x
        | Error err ->
            match err with
            | BadInput message -> RequestErrors.BAD_REQUEST message
    
    let toResponseAsync okResult next ctx (result: Task<_>) = 
        task {
            let! x = result
            return! toResponse okResult x next ctx
        }

    let withDep<'T> handler : HttpHandler =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            let service = ctx.GetService<'T>()
            handler next ctx service

module CsrfHandlers =
    open Giraffe
    open Microsoft.AspNetCore.Http
    open Microsoft.AspNetCore.Antiforgery
    open RouteHandlers
    open Memo.Shared.Web.Csrf

    let csrfCookie =
        fun next (ctx: HttpContext) ->
            let csrf = ctx.GetService<IAntiforgery> ()
            let store = csrf.GetAndStoreTokens ctx
            let cookieOptions = CookieOptions(SameSite = SameSiteMode.Lax)
            ctx.Response.Cookies.Append (apiCsrfCookieName, store.RequestToken, cookieOptions)
            next ctx

module AuthHandlers =
    open System.Security.Claims
    open Microsoft.AspNetCore.Authentication.Cookies
    open Microsoft.AspNetCore.Authentication
    open Microsoft.AspNetCore.Http
    open Microsoft.Extensions.Logging
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Giraffe
    open Memo.Core
    open Memo.Shared.Web.Auth

    [<CLIMutable>]
    type LoginRequest =
        { Email: string
          Password: string }

    let parseRequest<'T> handlerGetter =
        fun next (ctx: HttpContext) ->
            task {
                try
                    let! req = ctx.BindJsonAsync<'T>()
                    return! handlerGetter req next ctx
                with
                | ex -> return! RequestErrors.BAD_REQUEST (sprintf "Bad json: %s" <| ex.Message) next ctx
            }

    [<Literal>]
    let fullNameClaimName = "FullName"

    let signIn (req: LoginRequest) =
        fun next (ctx: HttpContext) (logger: ILogger<LoginRequest>) ->
            task {
                let testUser =
                    { UserId = if req.Email = "test1@test.com" then UserId 1L else UserId 2L                      
                      Email = req.Email
                      FullName = "Mr.User" }
                let basicClaims = [
                    Claim(ClaimTypes.Name, testUser.Email)
                    Claim(fullNameClaimName, testUser.FullName)
                ]
                let claims = (
                    if req.Email = "test2@test.com"
                    then basicClaims @ [Claim(ClaimTypes.Role, "Admin")]
                    else basicClaims)

                let claimsIdentity = ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)
                let authProperties = AuthenticationProperties()
                do! ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, ClaimsPrincipal(claimsIdentity), authProperties);
                logger.LogInformation (sprintf "User signed (Email = %s, FullName = %s)" testUser.Email testUser.FullName)
                return! Successful.ok (json (Authenticated testUser)) next ctx
            }
    
    let singOut =
        fun next (ctx: HttpContext) ->
            task {
                do! ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme)
                return! Successful.NO_CONTENT next ctx
            }

    let getAppUser =
        fun next (ctx: HttpContext) ->
            if ctx.User.Identity.IsAuthenticated then
                let user = { UserId = UserId 1L
                             Email = ctx.User.Identity.Name
                             FullName = ctx.User.FindFirst(fullNameClaimName).Value }
                Successful.ok (json (Authenticated user)) next ctx
            else 
                Successful.ok (json Guest) next ctx

module MemoHandlers =
    open System
    open Microsoft.AspNetCore.Http
    open Giraffe
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Microsoft.Extensions.Logging
    open Memo.Core
    open RouteHandlers

    let getAllMemos =
        fun next ctx ->
            let generateMemo userId i =
                { MemoId = MemoId i
                  Title = sprintf "Memo - %i" i
                  ChangeDate = DateTime.Now
                  Text = "Lorem ipsum dolor sit amet, consectetur adipisicing elit. Tempore, at!"
                  UserId = userId }
            let user1memos = 
                seq { 1L .. 5L }
                |> Seq.map (generateMemo (UserId 1L))
                |> Seq.toList
            let user2memos =
                seq { 6L .. 10L }
                |> Seq.map (generateMemo (UserId 2L))
                |> Seq.toList
            let allMemos = user1memos @ user2memos
            ok allMemos next ctx

    type NewType = { 
        Name: string 
        Full: bool option
    } 

    type PostMemoRequest = 
        | New of NewType
        | Edit of name:string
        | Empty

    let postMemo =
        fun next (ctx: HttpContext) (logger: ILogger<PostMemoRequest>) ->
            task {
                let! reqStr = ctx.ReadBodyFromRequestAsync()
                let req = Thoth.Json.Net.Decode.Auto.fromString<PostMemoRequest> reqStr
                match req with
                | Ok x ->
                    logger.LogInformation (sprintf "Req: %A" x)
                    return! Successful.ok (json x) next ctx
                | Error err ->
                    return! RequestErrors.BAD_REQUEST err next ctx
            }

    