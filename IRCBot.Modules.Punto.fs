module IRCBot.Modules.Punto

open IRCBot
open IRCBot.Public.Prefix

open System

let fixLayout s =
    let latinToCyrillic =
        [
         ('q', 'й'); ('w', 'ц');
         ('e', 'у'); ('r', 'к');
         ('t', 'е'); ('y', 'н');
         ('u', 'г'); ('i', 'ш');
         ('o', 'щ'); ('p', 'з');
         ('[', 'х'); (']', 'ъ');
         ('a', 'ф'); ('s', 'ы');
         ('d', 'в'); ('f', 'а');
         ('g', 'п'); ('h', 'р');
         ('j', 'о'); ('k', 'л');
         ('l', 'д'); (';', 'ж');
         ('z', 'я'); ('x', 'ч');
         ('c', 'с'); ('v', 'м');
         ('b', 'и'); ('n', 'т');
         ('m', 'ь'); (',', 'б');
         ('.', 'ю'); ('/', '.');
         ('?', '.')] |> Map.ofList

    let convert (c : char) =
        let fix (c : char) =
            if latinToCyrillic.ContainsKey c then latinToCyrillic.[c]
            else c

        if Char.IsUpper c then
            fix (Char.ToLower c) |> Char.ToUpper
        else
            fix c

    String.map convert s

let mutable (lastMessages : Map<string, string>) = [] |> Map.ofList

let punto(msg) =
    match msg with
    | Some { nick = nick; ident = ident },
      Some { command = "PRIVMSG"; args = [channel; text] } ->
        match text with
        | Prefix "!fix" _ ->
            if lastMessages.ContainsKey nick then
                [{ command = "PRIVMSG";
                   args = [ channel;
                            sprintf ":fixed: %s" (fixLayout lastMessages.[nick]) ] }]
            else []
        | _ -> []
    | _ -> []

let saveLastMessage(msg) =
    match msg with
    | Some { nick = nick },
      Some { command = "PRIVMSG"; args = [_; text] } -> // FIXME: ignoring channel!
        lastMessages <- Map.add nick text lastMessages
        []
    | _ -> []
