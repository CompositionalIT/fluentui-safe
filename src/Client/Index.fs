module Index

open Elmish
open Fable.Remoting.Client
open Shared

type Model = { Todos: Todo list; Input: string }

type Msg =
    | GotTodos of Todo list
    | SetInput of string
    | AddTodo
    | AddedTodo of Todo

let todosApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<ITodosApi>

let init () =
    let model = { Todos = []; Input = "" }
    let cmd = Cmd.OfAsync.perform todosApi.getTodos () GotTodos
    model, cmd

let update msg model =
    match msg with
    | GotTodos todos -> { model with Todos = todos }, Cmd.none
    | SetInput value -> { model with Input = value }, Cmd.none
    | AddTodo ->
        let todo = Todo.create model.Input

        let cmd = Cmd.OfAsync.perform todosApi.addTodo todo AddedTodo

        { model with Input = "" }, cmd
    | AddedTodo todo ->
        {
            model with
                Todos = model.Todos @ [ todo ]
        },
        Cmd.none

open Feliz

open FS.FluentUI
let private todoAction model dispatch =
    Html.div [
        prop.className "flex flex-col sm:flex-row mt-4 gap-4"
        prop.children [
            Fui.input [
                input.className "w-full"
                input.value model.Input
                input.placeholder "What needs to be done?"
                input.autoFocus true
                input.onChange (SetInput >> dispatch)
                input.onKeyPress (fun ev ->
                    if ev.key = "Enter" then
                        dispatch AddTodo)
            ]
            Fui.button [
                button.disabled (Todo.isValid model.Input |> not)
                button.onClick (fun _ -> dispatch AddTodo)
                button.text "Add"
            ]
        ]
    ]

let private todoList model dispatch =
    Html.div [
        prop.className "bg-white/80 rounded-md shadow-md p-4 w-5/6 lg:w-3/4 lg:max-w-2xl"
        prop.children [
            Html.ol [
                prop.className "list-decimal ml-6"
                prop.children [
                    for todo in model.Todos do
                        Html.li [ prop.className "my-1"; prop.text todo.Description ]
                ]
            ]

            todoAction model dispatch
        ]
    ]

let view model dispatch =
    Fui.fluentProvider [
        fluentProvider.theme.webLightTheme
        fluentProvider.children [
            Html.section [
                prop.className "h-screen w-screen"
                prop.style [
                    style.backgroundSize "cover"
                    style.backgroundImageUrl "https://unsplash.it/1200/900?random"
                    style.backgroundPosition "no-repeat center center fixed"
                ]

                prop.children [
                    Html.a [
                        prop.href "https://safe-stack.github.io/"
                        prop.className "absolute block ml-12 h-12 w-12 bg-teal-300 hover:cursor-pointer hover:bg-teal-400"
                        prop.children [ Html.img [ prop.src "/favicon.png"; prop.alt "Logo" ] ]
                    ]

                    Html.div [
                        prop.className "flex flex-col items-center justify-center h-full"
                        prop.children [
                            Html.h1 [
                                prop.className "text-center text-5xl font-bold text-white mb-3 rounded-md p-4"
                                prop.text "fluentuitest"
                            ]
                            todoList model dispatch
                        ]
                    ]
                ]
            ]
        ]
    ]