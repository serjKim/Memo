namespace Memo.Client.NodeModules

open Fable.Core.JsInterop
open Fable.Core
open Fable.React

module React =
    module HooksEx =
        let useReducer(reducer: 'S -> 'A -> 'S, initState: 'S): 'S * ('A -> unit) = importMember "react"
        let useLayoutEffect (effect: unit -> unit, deps: obj[]): unit = importMember "react"

module Rxjs =
    open System

    module Subscription =
        type ISubscription = inherit IDisposable

        [<Emit("$0.unsubscribe()")>]
        let unsubscribe (_: ISubscription): unit = jsNative

    type IObservable<'T> =
        [<Emit("$0.pipe($1)")>]
        abstract Pipe: [<ParamArray>] ops:Operator<'T, 'R>[] -> IObservable<'T>
    and
        Operator<'T, 'R> = IObservable<'T> -> IObservable<'R>

    [<Emit("$1.subscribe($0)")>]
    let subscribe (callback: 'T -> unit) (o: IObservable<'T>): Subscription.ISubscription = jsNative

    let subscribeDisposable callback o : IDisposable =
        let subscr = o |> subscribe callback
        { new IDisposable with
            member _.Dispose() = Subscription.unsubscribe subscr }

    let interval (period: int): IObservable<int> = importMember "rxjs";
    let ``of`` (item: 'T): IObservable<'T> = importMember "rxjs";

    [<RequireQualifiedAccess>]
    module Subject =
        type ISubjectCtor = interface end
        type ISubject<'T> =
            inherit IObservable<'T>
            [<Emit("$0.next($1)")>] abstract Next : ?value:'T -> unit

        let private subjectCtor: ISubjectCtor = import "Subject" "rxjs"

        [<Emit("new $0()")>]
        let inline private createSubject (ctor: ISubjectCtor) : ISubject<'T> = jsNative

        let create () = createSubject subjectCtor

        [<Emit("$1.next($0)")>]
        let next (item: 'a) (subject: ISubject<'a>): unit = jsNative

    module Operators =
        open Fable.Core.JS
        let map (project: 'T -> 'R): Operator<'T, 'R> = importMember "rxjs/operators"
        let filter (predicate: 'T -> bool): Operator<'T, 'T> = importMember "rxjs/operators"
        let delay (delay: int): Operator<'T, 'T> = importMember "rxjs/operators"
        let take (count: int): Operator<'T, 'T> = importMember "rxjs/operators"
        let switchMap (project: 'T -> int -> IObservable<'R>): Operator<'T, 'R> = importMember "rxjs/operators"
        let switchAll (): Operator<IObservable<'T>, 'T> = importMember "rxjs/operators"
        let tap (next: 'T -> unit): Operator<'T, 'T> = importMember "rxjs/operators"
        let debounceTime (dueTime: int): Operator<'T, 'T> = importMember "rxjs/operators"
        let catchError (selector: 'a -> IObservable<'T> -> IObservable<'b>): Operator<'T, 'b> = importMember "rxjs/operators"
        let mergeMap (project: 'T -> int -> IObservable<'R>): Operator<'T, 'R> = importMember "rxjs/operators"
        let mergeMapPromise (project: 'T -> int -> Promise<'R>): Operator<'T, 'R> = import "mergeMap" "rxjs/operators"

    /// XHR
    /// Pros:
    ///  + Supports request aborting
    ///  + Supports onprogress event
    module Ajax =
        type IAjaxResponse<'b> =
            abstract response: 'b
            abstract responseText: string

        type IAjaxMethod =
            [<Emit("$0.get($1, $2)")>]
            abstract Get : url:string * ?headers:obj -> IObservable<IAjaxResponse<'b>>

            [<Emit("$0.post($1, $2, $3)")>]
            abstract Post : url:string * ?body:obj * ?headers:obj -> IObservable<IAjaxResponse<'b>>

        let ajax : IAjaxMethod = importMember "rxjs/ajax"

    /// Fetch
    /// Pros:
    ///  +  Able to get the response text despite the contentType: application/json as opposed to Rxjs.Ajax (XHR),
    ///     so Thoth.Json.Decore.fromString can be used.
    /// Cons:
    ///  +  Thoth.Fetch is not cancellable (lakes of signal, AbortController), use the Rxjs.Fetch instead
    ///  +  No progress events
    module Fetch =
        open Fable.Core.JS
        open Browser.Types

        type Body =
            abstract bodyUsed: bool with get, set
            abstract arrayBuffer: unit -> JS.Promise<JS.ArrayBuffer>
            abstract blob: unit -> JS.Promise<Blob>
            abstract formData: unit -> JS.Promise<FormData>
            abstract json : unit -> JS.Promise<obj>
            abstract json<'T> : unit -> JS.Promise<'T>
            abstract text : unit -> JS.Promise<string>

        type RequestInit =
            { body: string
              method: string
              headers: obj }

        type Response =
            inherit Body

        type From =
            static member Fetch(url: string, init: {| Selector: Response -> Promise<'R> |}) : IObservable<'R> = import "fromFetch" "rxjs/fetch"
            static member Fetch(url: string, init: RequestInit) : IObservable<Response> = import "fromFetch" "rxjs/fetch"

module Emotion =
    type IEmotionCss = interface end

    let private css : IEmotionCss = importMember "emotion"

    [<Emit("$0`${$1}`")>]
    let private invokeCss (css:IEmotionCss, styles:string) : string = jsNative

    let createClass styles = invokeCss (css, styles)
    let cx (classNames: string[]): string = importMember "emotion"

module Linaria =
    type ILinariaCss = interface end
    type ILinariaCx = interface end

    (* Beware (...sort of leaky abstraction):
       Using the attribute enforces to produce
          import { css } from 'linaria'
       rather than
          import { css as css$$$ } from 'linaria'
       The alias breaks the linaria loader. *)
    [<Import("css",from="linaria")>]
    let css : ILinariaCss = jsNative

    let cx : ILinariaCx = importMember "linaria"

    // Only css`...` can be replaced by linaria loader
    [<Emit("$0`${$1}`")>]
    let createClass (css:ILinariaCss, styles:string) : string = jsNative

    [<Emit("$0(...$1)")>]
    let merge (cx: ILinariaCx) (classNames: string[]) : string = jsNative

module Router5 =
    type Route = { name: string; path: string}
    type RouteState =
        { name: string
          (*params: Params
          path: string
          meta?: StateMeta*) }
    type PluginFactory = class end
    type Unsubscribe = unit -> unit
    type CancelFn = unit -> unit
    type DoneFn = obj -> RouteState -> unit

    type IRouter =
        abstract usePlugin : [<System.ParamArray>]plugins:PluginFactory[] -> Unsubscribe
        abstract start : (unit -> unit) -> IRouter
        abstract buildUrl : name:string * ?queryParams:obj -> string
        abstract navigate : routeName:string * ?doneFn:DoneFn ->  CancelFn;
        abstract isActive : name:string * ?queryParams: obj * ?strictEquality:bool * ?ignoreQueryParams: bool -> bool

    let createRouter (routes: Route[]) (options: {| defaultRoute: string |}) : IRouter = importDefault "router5"
    let browserPlugin : unit -> PluginFactory = importDefault "router5-plugin-browser"

    type IRouteContext =
        abstract route : RouteState

    let useRouteNode : nodeName:string -> IRouteContext = importMember "react-router5"
    let useRouter : unit -> IRouter = importMember "react-router5"

    type RouteProviderProps =
        { router: IRouter
          (* children: ReactNode *) }

    let inline routerProvider (props: RouteProviderProps) (elems: ReactElement list) : ReactElement =
        ofImport "RouterProvider" "react-router5" props elems

    let inline link (props: {| routeName: string |}) (elems: ReactElement list) : ReactElement =
        ofImport "Link" "react-router5" props elems

module Notistack =
    type ISnackbarKey = interface end

    [<StringEnum>]
    [<RequireQualifiedAccess>]
    type VariantType =
        | Default
        | Error
        | Success
        | Warning
        | Info

    type OptionsObject = { variant: VariantType }

    type SnackbarProviderProps = { maxSnack: int; preventDuplicate: bool }

    let inline snackbarProvider (props: SnackbarProviderProps) (elems: ReactElement list) : ReactElement =
        ofImport "SnackbarProvider" "notistack" props elems

    type IProviderContext =
        abstract enqueueSnackbar: message: string * ?options: OptionsObject -> ISnackbarKey
        abstract closeSnackbar: ?key:ISnackbarKey -> unit

    let useSnackbar () : IProviderContext = importMember "notistack"
