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
    | Some(_, { command = "PRIVMSG"; text = Some(text)}) ->
        match text with
        | Prefix "!version" rest ->
            Some { command = "PRIVMSG";
                   subject = Some channel;
                   text = Some (sprintf "I on %A" Environment.Version) }
        | Prefix "!date" rest ->
            Some { command = "PRIVMSG";
                   subject = Some channel;
                   text = Some (sprintf "%A" System.DateTime.Now) }
        | Prefix "!help" rest ->
            Some { command = "PRIVMSG";
                   subject = Some channel;
                   text = Some "Google it!" }
        | Prefix "!echo" rest ->
            Some { command = "PRIVMSG";
                   subject = Some channel;
                   text = Some rest }
        | Prefix "!sosnool" rest ->
            let result =
                if List.contains (rest.ToLower()) ["awesomelackware"] then
                    Some "NIET."
                elif List.contains (rest.ToLower()) ["timdorohin"] then
                    Some "Несколько раз."
                else
                    Some "Да."
            Some { command = "PRIVMSG";
                   subject = Some channel;
                   text = result }
        | s when s.Contains "ЖМУ/Пинус" ->
            Some { command = "PRIVMSG";
                   subject = Some channel;
                   text = Some "Жми лучше!" }
        | s when s.Contains " - " ->
            Some { command = "PRIVMSG";
                   subject = Some channel;
                   text = Some "Сдохни, тварь!" }
        | _ -> None
    | None | Some(_) -> None

let server = "chat.freenode.net"
let port = 6667
let channel = "#lor"
let nick = "pidor-2"

let funcs = [test]
let myBot = new IrcBot(server, port, channel, nick, funcs)
myBot.loop ()
