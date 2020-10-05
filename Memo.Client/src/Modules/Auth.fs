namespace Memo.Client.Modules

module Auth =
    open Browser
    open Thoth.Fetch
    open Fable.React.Helpers
    open Fable.React.HookBindings
    open Memo.Client.AppResult
    open Memo.Shared.Web.Auth

    type SignAction =
        | SilentSign
        | SignIn of email:string * password:string
        | SignOut

    type SignState = { User: AppUser }

    [<RequireQualifiedAccess>]
    module private AuthApi =
        open Memo.Client.Api

        (* Rx-way implementation:
        open Fetch.Types
        let fetchUser () =
            getJson "/api/auth/user"
            |> map deserialize<AppUser>

        let login (email: string) (password: string) =
            postJson "/api/auth/login" {| Email = email; Password = password |} {| ``content-type`` = "application/json; charset=utf-8" |}
            |> map deserialize<AppUser>

        let logout () =
            postJson "/api/auth/logout" undefined undefined
        *)

        let fetchUser () : AppPromise<AppUser> =
            Fetch.tryGet ("/api/auth/user", extra = extraDecoders)
            |> Promise.mapResultError ApiCallFailed

        let signIn (email: string) (password: string) : AppPromise<AppUser> =
            Csrf.withToken (fun headers ->
                Fetch.tryPost (url = "/api/auth/signIn",
                               data = {| Email = email; Password = password |},
                               extra = extraDecoders,
                               headers = headers)
                |> Promise.mapResultError ApiCallFailed)

        let singOut () : AppPromise<unit> =
            Csrf.withToken (fun headers ->
                Fetch.tryPost (url = "/api/auth/signOut",
                               headers = headers)
                |> Promise.mapResultError ApiCallFailed)

    type SignDispatch = SignAction -> unit
    type SignHookResult = { Dispatch: SignDispatch; AppUser: AppResult<AppUser> }

    let private useUser init =
        let userState = Hooks.useState (Ok init)
        let inline setUser (user: AppResult<AppUser>) : unit = userState.update user
        (userState.current, setUser)

    let useSign () =
        let user,setUser = useUser Guest
        let dispatch = Hooks.useMemo((fun () ->
            fun action ->
                promise {
                    match action with
                    | SilentSign ->
                        let! user = AuthApi.fetchUser ()
                        setUser user
                    | SignIn (email,pass) ->
                        let! _ = AuthApi.signIn email pass
                        window.location.reload ()
                    | SignOut ->
                        let! _ = AuthApi.singOut ()
                        window.location.reload ()
                } |> ignore), [||])
        { Dispatch = dispatch
          AppUser = user }

    let private authContext = createContext Guest
    let authProvider user = contextProvider authContext user

    let useAppUser () = Hooks.useContext(authContext)