module IRCBot.Modules.Sample

open Markov

open IRCBot
open IRCBot.Public.Constants
open IRCBot.Public.Prefix

open System

// Führer is not only Hitler.
let fuehrer = "awesomelackware"
let join(msg) =
    match msg with
    | Some { nick = fuehrer },
      Some { command = "PRIVMSG"; args = [_; text] } ->
        match text with
        | Prefix "!join" channel ->
            [join channel]
        | Prefix "!leave" channel ->
            [{ command = "PART";
               args = [ channel ] }]
        | _ -> []
    | _ -> []

let sample(msg) =
    match msg with
    | Some { nick = fuehrer },
      Some { command = "PRIVMSG"; args = [_; "!die"] } ->
        [{ command = "QUIT"; args = [] }]
    | Some { nick = someNick },
      Some { command = "PRIVMSG"; args = [channel; text] } ->
        match text with
        | Prefix "!version" _ ->
            [privmsg channel <|
             sprintf "I on %A" Environment.Version]
        | Prefix "!date" _ ->
            [privmsg channel <|
             System.DateTime.Now.ToString ()]
        | Prefix "!help" _ ->
            [privmsg channel "Google it!"]
        | Prefix "!echo" rest ->
            [privmsg channel rest]
        | s when (List.map s.Contains trigger
                  |> List.exists ((=) true)) ->
            [privmsg channel <| degenerate ()]
        | _ -> []
    | _ -> []

