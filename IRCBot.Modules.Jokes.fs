module IRCBot.Modules.Jokes

open IRCBot
open IRCBot.Public.Prefix
open IRCBot.Public.Constants

let isGay(msg) =
    match msg with
    | Some { nick = nick },
      Some { command = "PRIVMSG"; args = [channel; text] } ->
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

let sorry(msg) =
    match msg with
    | Some { nick = nick },
      Some { command = "PRIVMSG"; args = [channel; text] } ->
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

let admin(msg) =
    match msg with
    | Some { nick = nick },
      Some { command = "PRIVMSG"; args = [channel; text] } when
      List.contains nick ["awesomelackware"; "timdorohin";
                          "Bratishka"; "slaykovsky"] ->
          match text with
          | Prefix "!+v" _ ->
              [{ command = "MODE";
                 args = [ channel; "+v"; nick ] }]
          | Prefix "!-v" _ ->
              [{ command = "MODE";
                 args = [ channel; "-v"; nick ] }]
          | _ -> []
    | _ -> []