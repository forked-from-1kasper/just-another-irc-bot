#r "IRCBot.dll"
#r "IRCBot.Public.Prefix.dll"
#r "IRCBot.Public.Constants.dll"
#r "IRCBot.Modules.Vote.dll"
#r "IRCBot.Modules.Jokes.dll"
#r "IRCBot.Modules.Title.dll"
#r "IRCBot.Modules.Sample.dll"
#r "IRCBot.Modules.Punto.dll"
#r "IRCBot.Modules.Online.dll"
#r "Markov.dll"

open IRCBot
open IRCBot.Public.Prefix
open IRCBot.Public.Constants
open IRCBot.Modules.Vote
open IRCBot.Modules.Jokes
open IRCBot.Modules.Title
open IRCBot.Modules.Sample
open IRCBot.Modules.Punto
open IRCBot.Modules.Online
open Markov

open System

//all: [showLinksTitle; sorry; SIEGHEIL; sample; vote; punto; saveLastMessage; zogControl]
let funcs = [showLinksTitle; vote; SIEGHEIL; sample; learn]
let myBot = new IrcBot(server, port, channel, botNick, funcs)

let public saveDBAsync (sleepTime : int) =
    async {
        while true do
            printfn "[db-save] saving db"
            Markov.saveDB Markov.DBLocation Markov.wordsMap
            printfn "[db-save] done saving, sleep"
        
            do! Async.Sleep(sleepTime)
        return ()
    }

[ (async { myBot.loop () }); (saveDBAsync 5000) ]
|> Async.Parallel
|> Async.RunSynchronously
