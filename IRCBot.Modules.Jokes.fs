module IRCBot.Modules.Jokes

open IRCBot
open IRCBot.Public.Prefix
open IRCBot.Public.Constants

let isGay(msg, channel) =
    match msg with
    | Some({ nick = nick },
           { command = "PRIVMSG"; args = [channel; text] }) ->
        match text with
        | Prefix "!isGay" rest ->
            let isPidoras = function
                | "timdorohin" -> "true"
                | _ -> "false"

            [{ command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%s: %s" nick (isPidoras rest) ] }]
        | _ -> []
    | _ -> []

let SIEGHEIL(msg, channel) =
    match msg with
    | Some({ nick = nick },
           { command = "JOIN"; args = [channel] }) when nick <> botNick ->
        [{ command = "PRIVMSG";
           args = [ channel;
                    sprintf ":%s: ты гей" nick ]}]
    | _ -> []

let sorry(msg, channel) =
    match msg with
    | Some({ nick = nick },
           { command = "PRIVMSG"; args = [channel; text] }) ->
        match text with
        | Prefix "!пожалеть" _ ->
            [{ command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%s: Пожалел тебе за щёку." nick ] };
             { command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%s: Проверяй." nick ] }]
        | Prefix "!лучше" _ ->
            [{ command = "PRIVMSG";
               args = [ channel;
                        ":Дуднет рулит, ко‐ко‐ко" ] }]
        | _ -> []
    | _ -> []

let admin(msg, channel) =
    match msg with
    | Some({ nick = nick },
           { command = "PRIVMSG"; args = [channel; text] }) when
      List.contains nick ["awesomelackware"; "timdorohin"] ->
          match text with
          | Prefix "!дайодменку" _ ->
              [{ command = "MODE";
                 args = [ channel; "+o"; nick ] }]
          | _ -> []
    | _ -> []
