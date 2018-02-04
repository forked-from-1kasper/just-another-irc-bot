module IRCBot.Modules.Sample

open Markov

open IRCBot
open IRCBot.Public.Constants
open IRCBot.Public.Prefix

open System

// Führer is not only Hitler.
let fuehrer = "awesomelackware"
let joinleave(msg) = chat {
    let! text =
        match msg with
        | Some { nick = nick' },
          Some { command = "PRIVMSG"; args = [_; text'] }
          when nick' = fuehrer ->
            Some text'
        | _ -> None
    return
        (match text with
         | Prefix "!join" channel ->
             [join channel]
         | Prefix "!leave" channel ->
             [{ command = "PART";
                args = [ channel ] }]
         | Prefix "!die" _ ->
             [{ command = "QUIT";
                args = [] }]
         | _ -> [])
}

let sample(msg) = chat {
    let! (channel, text) =
        match msg with
        | _, Some { command = "PRIVMSG";
                    args = [channel'; text'] } ->
            Some (channel', text')
        | _ -> None

    return
        (match text with
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
         | _ -> [])
}