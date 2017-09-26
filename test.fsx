#r "bot.dll"

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

let test msg channel =
    match msg with
    | Some(_, { command = "PRIVMSG"; args = [_; text] }) ->
        match text with
        | Prefix "!version" rest ->
            Some { command = "PRIVMSG";
                   args = [ channel;
                            sprintf ":I on %A" Environment.Version ] }
        | Prefix "!date" rest ->
            Some { command = "PRIVMSG";
                   args = [ channel;
                            sprintf ":%A" System.DateTime.Now ] }
        | Prefix "!help" rest ->
            Some { command = "PRIVMSG";
                   args = [ channel;
                            ":Google it!" ] }
        | Prefix "!echo" rest ->
            Some { command = "PRIVMSG";
                   args = [ channel;
                            sprintf ":%s" rest ] }
        | Prefix "!sosnool" rest ->
            let result =
                if List.contains (rest.ToLower()) ["awesomelackware"] then
                    "NIET."
                elif List.contains (rest.ToLower()) ["timdorohin"] then
                    "Несколько раз."
                else
                    "Да."
            Some { command = "PRIVMSG";
                   args = [ channel;
                            sprintf ":%s" result ] }
        | s when s.Contains "ЖМУ/Пинус" ->
            Some { command = "PRIVMSG";
                   args = [ channel;
                            ":Жми лучше!" ] }
        | s when s.Contains " - " ->
            Some { command = "PRIVMSG";
                   args = [ channel;
                            ":Сдохни, тварь!" ] }
        | _ -> None
    | None | Some(_) -> None

let server = "chat.freenode.net"
let port = 6667
let channel = "#lor"
let nick = "pidor-2"

let funcs = [test]
let myBot = new IrcBot(server, port, channel, nick, funcs)
myBot.loop ()
