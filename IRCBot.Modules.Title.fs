module IRCBot.Modules.Title

open System
open FSharp.Data
open IRCBot

let getTitle (link : string) =
    if link.EndsWith ".iso" then None
    else
        try
            let results = HtmlDocument.Load(link)
            results.Descendants ["title"]
            |> Seq.head
            |> (fun x -> x.InnerText())
            |> Some
        with
            | ex ->
                printfn "Unhandled Exception: %s" ex.Message
                None

let internal notAllowedEnds = [ ".png"; ".jpg"; ".rar"; ".gif"; ".gz" ]
let internal notHasNotAllowedEnd (s : string) =
    let rec support = function
    | x :: xs -> if s.EndsWith x then false else support xs
    | [] -> true
    support notAllowedEnds
let showLinksTitle(msg) =
    let getValue (x : 'a option) = x.Value

    match msg with
    | _, Some { command = "PRIVMSG"; args = [channel; text] } ->
        text.Split [| ' ' |]
        |> List.ofArray
        |> List.filter (fun s ->
                        (s.StartsWith "http://" ||
                         s.StartsWith "https://") &&
                        notHasNotAllowedEnd s)
        |> List.map getTitle
        |> List.filter (fun x -> x.IsSome)
        |> List.map (getValue >> sprintf "Title: %s" >> notice channel)
    | _ -> []

