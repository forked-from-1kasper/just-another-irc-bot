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
open IRCBot.Modules.Title
open IRCBot.Modules.Punto

open System

let regexp(msg) = chat {
    let! (userInfo, msgInfo) =
        match msg with
        | Some userInfo', Some msgInfo' -> Some (userInfo', msgInfo')
        | _ -> None
    let! (channel, text) =
        match msgInfo.args with
        | [channel'; text'] -> Some (channel', text')
        | _ -> None
    
    let! result =
        match text with
        | Prefix "!regexp" rest ->
            match rest.Split ([| " / " |], StringSplitOptions.None) with
            | [| pattern; forReplace |] ->
                if lastMessages.ContainsKey userInfo.nick then
                    let toFix = lastMessages.[userInfo.nick]
                    let nowFixed = Regex.Replace(toFix, pattern, forReplace)
                    Some (sprintf ":%s хотел сказать: «%s»" userInfo.nick nowFixed)
                else None
            | [| nick; pattern; forReplace |] ->
                if lastMessages.ContainsKey nick then
                    let toFix = lastMessages.[nick]
                    let nowFixed = Regex.Replace(toFix, pattern, forReplace)
                    Some (sprintf ":%s думает, что %s хотел сказать: «%s»" userInfo.nick nick nowFixed)
                else None
            | _ -> None
        | _ -> None

    return [{ command = "PRIVMSG";
              args = [ channel; result ] }]
}

let funcs = [showLinksTitle;
             vote;
             bindAsyncFunctions [regexp; saveLastMessage]]

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
