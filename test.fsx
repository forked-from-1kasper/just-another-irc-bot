#r "IRCBot.dll"

open IRCBot
open System

let (|Prefix|_|) (p : string) (s : string) =
    if s.StartsWith p then
        if p.Length + 1 > s.Length then
            Some ""
        else
            Some (s.Substring(p.Length + 1))
    else
        None

let test(msg) =
    match msg with
    | _, Some { command = "PRIVMSG"; args = [channel; text] } ->
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
               args = [ channel;
                        ":Google it!" ] }]
        | Prefix "!echo" rest ->
            [{ command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%s" rest ] }]
        | Prefix "!sosnool" rest ->
            let result =
                if List.contains (rest.ToLower()) ["awesomelackware"] then
                    "NIET."
                elif List.contains (rest.ToLower()) ["timdorohin"] then
                    "Несколько раз."
                else
                    "Да."
            [{ command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%s" result ] };
             { command = "NOTICE";
               args = [ channel;
                        ":Проверьте свои щёки!" ] }]
        | s when s.Contains "ЖМУ/Пинус" ->
            [{ command = "PRIVMSG";
               args = [ channel;
                        ":Жми лучше!" ] }]
        | s when s.Contains " - " ->
            [{ command = "PRIVMSG";
               args = [ channel;
                        ":Ой!" ] }]
        | _ -> []
    | _ -> []

let server = "chat.freenode.net"
let port = 6667
let channel = "#lor"
let nick = "mylittlepony"
let ident = "pony"

let funcs = [test]
let myBot = IrcBot({ server = server;
                     port = port;
                     channel = channel;
                     botNick = nick;
                     ident = ident;
                     funcs = funcs;
                     mode = { order = Parallel;
                              debug = false };
                     regular = [];
                     period = 1000.0})
myBot.Loop ()
