#r "IRCBot.dll"
#r "IRCBot.Public.Prefix.dll"
#r "IRCBot.Public.Constants.dll"
#r "IRCBot.Modules.Vote.dll"
#r "IRCBot.Modules.Jokes.dll"
#r "IRCBot.Modules.Title.dll"
#r "IRCBot.Modules.Sample.dll"
#r "IRCBot.Modules.Punto.dll"
#r "Markov.dll"

open System.Text.RegularExpressions
open IRCBot
open IRCBot.Public.Prefix
open IRCBot.Public.Constants
open IRCBot.Modules.Vote
open IRCBot.Modules.Jokes
open IRCBot.Modules.Title
open IRCBot.Modules.Sample
open IRCBot.Modules.Punto
open Markov

open System

let regexp(msg) =
    match msg with
    | Some { nick = nick },
      Some { command = "PRIVMSG"; args = [channel; text] } ->
        match text with
        | Prefix "!regexp" rest ->
            if lastMessages.ContainsKey nick then
                match rest.Split ([| " // " |], StringSplitOptions.None) with
                    | [| pattern; forReplace |] ->
                        let toFix = lastMessages.[nick]
                        let nowFixed = Regex.Replace(toFix, pattern, forReplace)
                        let toPrint =
                            sprintf ":%s хотел сказать: «%s»" nick nowFixed
                        [{ command = "PRIVMSG";
                           args = [ channel; toPrint ] }]
                    | [| nickname; patter; forReplace |] ->
                        let toFix = lastMessages.[nickname]
                        let nowFixed = Regex.Replace(toFix, patter, forReplace)
                        let toPrint =
                            sprintf
                                ":%s думает, что %s хотел сказать: «%s»"
                                nick nickname nowFixed
                        [{ command = "PRIVMSG";
                           args = [ channel; toPrint ] }]
                    | _ -> []
            else []
        | _ -> []
    | _ -> []

let funcs = [showLinksTitle;
             vote;
             bindAsyncFunctions [regexp; saveLastMessage]]
//let funcs = [vote]

let channel = "#lor"

let showHour =
    [ "полночь";
      "час ночи";
      "два часа ночи";
      "три часа ночи";
      "четыре часа ночи";
      "пять часов ночи";
      "шесть часов утра";
      "семь часов утра";
      "восемь часов утра";
      "девять часов утра";
      "десять часов утра";
      "одиннадцать часов утра";
      "полдень";
      "один час дня";
      "два часа дня";
      "три часа дня";
      "четыре часа дня";
      "пять часов дня";
      "шесть часов вечера";
      "семь часов вечера";
      "восемь часов вечера";
      "девять часов вечере";
      "десять часов вечера";
      "одиннадцать часов вечера"]

let showMinute minute =
    if minute = 0 then " ровно"
    elif minute = 30 then ", тридцать минут"
    else ""

let showTimeEvent =
    let showTime (time : DateTime) =
        let hour = time.Hour
        let minute = time.Minute
        let time = sprintf ":Сейчас в Красноярске %s%s." showHour.[hour]
                           (showMinute minute)
    
        [{ command = "NOTICE";
           args = [ channel; time ] }]
    let showTimePredicat (time : DateTime) =
        (time.Minute = 0 || time.Minute = 30) && (time.Second = 0)
    Event (showTime, showTimePredicat)

let myBot = IrcBot({server = server;
                    port = port;
                    channel = channel;
                    botNick = botNick;
                    funcs = funcs;
                    mode = { order = Parallel; debug = false };
                    regular = [showTimeEvent];
                    period = 1000.0})

myBot.Cron ()
myBot.Loop ()
