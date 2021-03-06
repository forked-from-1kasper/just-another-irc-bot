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
open IRCBot.Modules
open IRCBot.Modules.Vote
open IRCBot.Modules.Title
open IRCBot.Modules.Punto
open IRCBot.Modules.Jokes

open System

let channel = "#borsch"
let botNick = "NurNochMal"

let safeJoin(msg) = chat {
    let! (userInfo, msgInfo) =
        match msg with
        | Some userInfo', Some msgInfo' -> Some (userInfo', msgInfo')
        | _ -> None

    let! text =
        match msgInfo.args with
        | [_; text'] -> Some text'
        | _ -> None
    
    return (if text.StartsWith "You are now identified for" &&
               userInfo.nick = "NickServ" then
               [join channel]
            else [])
}

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

    return [privmsg channel result]
}

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
      "девять часов вечера";
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
    
        [notice channel time]
    let showTimePredicat (time : DateTime) =
        (time.Minute = 0 || time.Minute = 30) && (time.Second = 0)
    Event (showTime, showTimePredicat)

do printf "Password: "
let sendPassword =
    privmsg "NickServ" <| (sprintf "identify %s" <| Console.ReadLine ())

let myBot = IrcBot({ server = server;
                     port = port;
                     botNick = botNick;
                     ident = "frei";
                     funcs = [ showLinksTitle;
                               vote;
                               bindAsyncFunctions [regexp; saveLastMessage];
                               admin;
                               safeJoin;
                               Sample.joinleave ];
                     mode = { order = Parallel; debug = false };
                     regular = [ showTimeEvent ];
                     period = 1000.0;
                     atStart = [ sendPassword ] })

myBot.Cron ()
myBot.Loop ()
