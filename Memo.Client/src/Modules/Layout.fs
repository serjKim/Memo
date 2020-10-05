namespace Memo.Client.Modules

module Layout =
    module AppToolBar =
        open Fable.React
        module MuiProps = Fable.MaterialUI.Props
        module Mui = Fable.MaterialUI.Core
        module MuiIcons = Fable.MaterialUI.Icons
        open Fable.React.Props
        open Auth
        open Memo.Shared.Web.Auth

        let toolbarButtons = FunctionComponent.Of(fun (props: {| Dispatch: SignDispatch |}) ->
            let user = useAppUser ()
            div [] [
                match user with
                | Guest ->
                    Mui.button [
                        MuiProps.Color MuiProps.ComponentColor.Inherit
                        OnClick (fun _ -> props.Dispatch <| SignIn ("test1@test.com", "test"))
                    ] [ str "Sign In" ]
                | Authenticated au ->
                    Mui.tooltip [ MuiProps.TooltipProp.Title (str au.FullName) ] [
                        Mui.iconButton [
                            MuiProps.Color MuiProps.ComponentColor.Inherit
                        ] [
                            MuiIcons.accountCircleIcon []
                        ]
                    ]
                    Mui.button [
                        MuiProps.Color MuiProps.ComponentColor.Inherit
                        OnClick (fun _ -> props.Dispatch <| SignOut)
                    ] [ str "Sign Out" ]
            ])

    module NavBar =
        open Fable.React
        open Fable.Core.JsInterop
        open Memo.Client
        open NodeModules.Router5
        open Fable.React.Props
        open Fable.MaterialUI.Props
        module Mui = Fable.MaterialUI.Core
        module MuiIcons = Fable.MaterialUI.Icons
        open Routing

        let navBar = FunctionComponent.Of(fun (props: {| Items: string array |}) ->
            let counter = (match useRouteNode "" |> extractRouteName with
                           | Dashboard -> []
                           | Github -> [ str <| sprintf " (%i)" (Seq.length props.Items) ])

            let router = useRouter ()

            let listItem (routeName: RouteName) label icon counter =
                let route: string = !!routeName
                Mui.listItem [
                    ListItemProp.Button true
                    Component (ReactElementType.ofHtmlElement "a")
                    HTMLAttr.Selected (router.isActive route)
                    HTMLAttr.Href (router.buildUrl route)
                    OnClick (fun e ->
                        e.preventDefault()
                        router.navigate route |> ignore)
                ] [
                    Mui.listItemIcon [] [
                        icon
                    ]
                    Mui.listItemText [
                        ListItemTextProp.Primary (str label)
                    ] []
                    Mui.listItemSecondaryAction [] [
                        span [] counter
                    ]
                ]

            Mui.list [
                Component (ReactElementType.ofHtmlElement "nav")
            ] [
                listItem RouteName.Dashboard "Dashboard" (MuiIcons.dashboardIcon []) []
                listItem RouteName.Github "Github" (MuiIcons.listIcon []) counter
            ])

    module MainLayout =
        open Fable.Core.JsInterop
        open Fable.React
        open Fable.React.Props
        open Fable.MaterialUI.Themes
        open Fable.MaterialUI.Props
        module MuiProps = Fable.MaterialUI.Props
        module Mui = Fable.MaterialUI.Core
        module MuiIcons = Fable.MaterialUI.Icons
        open Memo.Client
        open Modules.Error

        type MainLayoutProps =
            { Content: ReactElement
              NavBar: ReactElement
              ToolbarButtons: ReactElement }
            interface Mui.IClassesProps with
                member _.classes: Mui.IClasses = { new Mui.IClasses }

        let private drawerWidth = 200

        let private mainLayoutStyles: ITheme -> IStyles list =
            fun (theme: ITheme) -> [
                Styles.Root [
                    CSSProp.Display DisplayOptions.Flex
                ]
                Styles.Custom ("appBar", [
                    CSSProp.ZIndex (theme.zIndex.drawer + 1)
                ])
                Styles.Custom ("drawer", [
                    CSSProp.Width drawerWidth
                    CSSProp.FlexShrink 0
                ])
                Styles.Custom ("drawerPaper", [
                    CSSProp.Width drawerWidth
                ])
                Styles.Custom ("content", [
                    CSSProp.FlexGrow 1
                    CSSProp.Padding "24px"
                ])
                Styles.Custom ("title", [
                    CSSProp.FlexGrow 1
                ])
            ]

        let private mainLayout' (props : MainLayoutProps) =
            let classes = (props :> Mui.IClassesProps).classes;
            div [ Class !!classes?root ] [
                Mui.cssBaseline []
                Mui.appBar [
                    Class !!classes?appBar
                    AppBarProp.Position AppBarPosition.Fixed
                ] [
                    Mui.toolbar [] [
                        Mui.typography [
                            TypographyProp.Variant TypographyVariant.H6
                            TypographyProp.NoWrap true
                            Class !!classes?title
                        ] [ str "Memo" ]
                        props.ToolbarButtons
                    ]
                ]
                Mui.drawer [
                    Class !!classes?drawer
                    DrawerProp.Variant DrawerVariant.Permanent
                    DrawerProp.Anchor Anchor.Left
                ] [
                    Mui.toolbar [] []
                    aside [ Class !!classes?drawerContainer ]
                        [
                            props.NavBar
                        ]
                ]
                main [ Class !!classes?content ] [
                    Mui.toolbar [] []
                    errorBoundary props.Content
                ]
            ]

        let mainLayout = Mui.withStyles<MainLayoutProps> (StyleType.Func mainLayoutStyles) [] mainLayout'

    module MainContent =
        open Fable.React
        open Memo.Client
        open NodeModules.Router5
        open Routing
        open Dashboard
        open Github

        let mainContent = FunctionComponent.Of(fun props ->
            match useRouteNode "" |> extractRouteName with
            | Dashboard -> MemoListComponent.memoList ()
            | Github    -> ListComponent.list props)