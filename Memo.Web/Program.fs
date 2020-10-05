namespace Memo.Web

module Program =
    open System.IO
    open System.Threading.Tasks
    open Microsoft.AspNetCore.Hosting
    open Microsoft.Extensions.Hosting
    open Microsoft.Extensions.Logging
    open Microsoft.AspNetCore.Builder
    open Microsoft.Extensions.DependencyInjection
    open Giraffe
    open Microsoft.AspNetCore.Http
    open Microsoft.AspNetCore.Antiforgery
    open Microsoft.Extensions.FileProviders
    open Microsoft.AspNetCore.Authentication.Cookies
    open Giraffe.Serialization
    open Thoth.Json.Net
    open Thoth.Json.Giraffe
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open RouteHandlers
    open AuthHandlers
    open MemoHandlers
    open CsrfHandlers

    let inline private ( ^ ) f x  = f x
    
    let authorized =
        RequestErrors.UNAUTHORIZED
            CookieAuthenticationDefaults.AuthenticationScheme
            ""
            "User is not authorized." |> requiresAuthentication

    let onlyAdmin = 
        RequestErrors.FORBIDDEN
            "Permission denied."
        |> requiresRole "Admin"
        
    let csrfTokenized =
        fun next (ctx: HttpContext) ->
            let csrf = ctx.GetService<IAntiforgery> ()
            task {
                let! isValid = csrf.IsRequestValidAsync ctx
                if isValid then
                   return! next ctx
                else
                   return! RequestErrors.BAD_REQUEST "CSRF Token is required" next ctx 
            }

    [<RequireQualifiedAccess>]
    module ClientBuild =
        let rootPath = "../Memo.Client/public"
        let indexHtmlPath = sprintf "%s/index.html" rootPath

    let webApp =
        choose [
            subRoute "/api" ^choose [
                GET >=> route "/csrfToken" >=> getCsrfToken
                GET >=> route "/ping" >=> json "pong"
                GET >=> route "/test" >=> authorized >=> json "test 123"
                GET >=> route "/admin-test" >=> authorized >=> onlyAdmin >=> json "admin-test 123"

                subRoute "/auth" ^choose [
                    GET >=> route "/user" >=> getAppUser
                    POST >=> route "/signIn" >=> csrfTokenized >=> parseRequest<LoginRequest> (signIn >> withDep<ILogger<LoginRequest>>)
                    POST >=> route "/signOut" >=> csrfTokenized >=> singOut
                ]

                subRoute "/memos" ^choose [
                    GET >=> getAllMemos
                    POST >=> (withDep<ILogger<PostMemoRequest >> postMemo)
                ]
            ]
            routex "/(.*)" >=> htmlFile ClientBuild.indexHtmlPath
        ]

    type Startup() =
        member __.ConfigureServices (services : IServiceCollection) =
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(fun options ->
                        options.Events.OnRedirectToLogin <-
                            fun context ->
                                context.Response.StatusCode <- StatusCodes.Status401Unauthorized
                                Task.CompletedTask
                        options.Events.OnRedirectToAccessDenied <- 
                            fun context ->
                                context.Response.StatusCode <- StatusCodes.Status403Forbidden
                                Task.CompletedTask)
            |> ignore

            services.AddAntiforgery() |> ignore                
            
            services.AddGiraffe() |> ignore
            
            let extraCoders =
                Extra.empty
                |> Extra.withInt64

            services.AddSingleton<IJsonSerializer>(ThothSerializer(extra = extraCoders)) |> ignore

            services.AddLogging(fun builder ->
                builder
                    .AddConsole()
                    .AddDebug()
                |> ignore)
            |> ignore
    
        member __.Configure (app : IApplicationBuilder)
                            (env : IHostEnvironment)
                            (logger : ILogger<Startup>) =

            logger.LogInformation (sprintf "Environment: %s" <| env.EnvironmentName)
            
            let staticFilesOptions = StaticFileOptions(FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, ClientBuild.rootPath)))
            app.UseStaticFiles (staticFilesOptions) |> ignore
            
            app.UseAuthentication() |> ignore
            app.UseGiraffe webApp
            

    [<EntryPoint>]
    let main _ =
        Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(
                fun webHostBuilder ->
                    webHostBuilder
                        .UseStartup<Startup>()
                        |> ignore)
            .Build()
            .Run()
        0
