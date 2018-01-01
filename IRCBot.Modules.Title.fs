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
            | _ as ex ->
                printfn "Unhandled Exception: %s" ex.Message
                None

let showLinksTitle(msg, channel) =
    match msg with
    | Some (_, { command = "PRIVMSG"; args = [channel; text] }) ->
        text.Split [| ' ' |]
        |> List.ofArray
        |> List.filter (fun s ->
                        (s.StartsWith "http://" ||
                         s.StartsWith "https://"))
        |> List.map getTitle
        |> List.filter (fun x -> x.IsSome)
        |> List.map (fun x -> x.Value)
        |> List.map (fun x ->
                         { command = "NOTICE";
                           args = [ channel;
                                    sprintf ":Title: %s" x ] })
    | _ -> []

