#r "IRCBot.dll"
#r "IRCBot.Public.Prefix.dll"
#r "IRCBot.Public.Constants.dll"
#r "IRCBot.Modules.Vote.dll"
#r "IRCBot.Modules.Jokes.dll"
#r "IRCBot.Modules.Title.dll"
#r "IRCBot.Modules.Sample.dll"
#r "IRCBot.Modules.Punto.dll"
#r "Markov.dll"

open IRCBot
open IRCBot.Public.Constants
open IRCBot.Modules.Vote
open IRCBot.Modules.Jokes
open IRCBot.Modules.Title
open IRCBot.Modules.Sample
open IRCBot.Modules.Punto
open Markov

//all: [showLinksTitle; sorry; SIEGHEIL; sample; vote; punto; saveLastMessage; zogControl]

let botNick = "sample"
let ident = "sample"
let channel = "#lor"

let funcs = [showLinksTitle;
             vote;
             admin;
             sample;
             learn;
             bindAsyncFunctions [punto; saveLastMessage]]

let timingEvent =
    let timing time =
        [{ command = "PRIVMSG";
           args = [channel;
                   sprintf ":%A" time] }]
    Event (timing, fun _ -> true)

let myBot = IrcBot ({ server = server;
                      port = port;
                      botNick = botNick;
                      ident = ident;
                      funcs = funcs;
                      mode = { order = Parallel;
                               debug = true };
                      regular = [timingEvent];
                      period = 1000.0;
                      atStart = [ join channel ] })

let public saveDBAsync (sleepTime : int) =
    async {
        while true do
            printfn "[db-save] saving db"
            Markov.saveDB Markov.DBLocation Markov.wordsMap
            printfn "[db-save] done saving, sleep"
        
            do! Async.Sleep(sleepTime)
        return ()
    }

myBot.Cron ()
let asynced f = async { f () }
[ asynced myBot.Loop; (saveDBAsync 5000) ]
|> Async.Parallel
|> Async.RunSynchronously
