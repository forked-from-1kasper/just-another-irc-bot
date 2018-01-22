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
            [{ command = "JOIN";
               args = [ channel ] }]
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
            [{ command = "PRIVMSG";
               args = [ channel;
                        sprintf ":I on %A" Environment.Version ] }]
        | Prefix "!date" _ ->
            [{ command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%A" System.DateTime.Now ] }]
        | Prefix "!help" _ ->
            [{ command = "PRIVMSG";
               args = [ channel; ":Google it!" ] }]
        | Prefix "!echo" rest ->
            [{ command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%s" rest ] }]
        | s when (List.map s.Contains trigger
                  |> List.exists ((=) true)) ->
            [{ command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%s" (degenerate ()) ] }]
        | _ -> []
    | _ -> []

