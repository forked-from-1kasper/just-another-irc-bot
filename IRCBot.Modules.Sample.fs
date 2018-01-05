module IRCBot.Modules.Sample

open Markov

open IRCBot
open IRCBot.Public.Constants
open IRCBot.Public.Prefix

open System

let sample(msg) =
    match msg with
    | Some { nick = "awesomelackware" },
      Some { command = "PRIVMSG"; args = [_; "!die"] } ->
        [{ command = "QUIT"; args = [] }]

    | _, Some { command = "PRIVMSG"; args = [nickArg; text] }
      when nickArg = botNick ->
        [{ command = "PRIVMSG";
           args = [ channel;
                    sprintf ":%s" (degenerate ()) ] }]

    | Some { nick = someNick },
      Some { command = "PRIVMSG"; args = [channel; text] } ->
        match text with
        | Prefix "!join" newChannel ->
            if someNick = "awesomelackware" then
                [{ command = "JOIN";
                   args = [ newChannel ] }]
            else []
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

