module IRCBot.Modules.Sample

open Markov

open IRCBot
open IRCBot.Public.Constants
open IRCBot.Public.Prefix

open System

let sample(msg, channel) =
    match msg with
    | Some({ nick = "awesomelackware" },
           { command = "PRIVMSG"; args = [_; "!die"] }) ->
        [{ command = "QUIT"; args = [] }]

    | Some (_, { command = "PRIVMSG"; args = [nickArg; text] })
      when nickArg = botNick ->
        [{ command = "PRIVMSG";
           args = [ channel;
                    sprintf ":%s" (degenerate ()) ] }]

    | Some(_, { command = "PRIVMSG"; args = [_; text] }) ->
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
        | Prefix botNick _ ->
            [{ command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%s" (degenerate ()) ]} ]
        | s when (List.map s.Contains trigger
                  |> List.exists ((=) true)) ->
            [{ command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%s" (degenerate ()) ] }]
        | _ -> []
    | _ -> []

