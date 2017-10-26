#r "bot.dll"
#r "IRCBot.Public.Prefix.dll"
#r "IRCBot.Public.Constants.dll"
#r "IRCBot.Modules.Vote.dll"
#r "IRCBot.Modules.Jokes.dll"
#r "IRCBot.Modules.Title.dll"
#r "IRCBot.Modules.Sample.dll"
#r "IRCBot.Modules.Punto.dll"

open IRCBot
open IRCBot.Public.Prefix
open IRCBot.Public.Constants
open IRCBot.Modules.Vote
open IRCBot.Modules.Jokes
open IRCBot.Modules.Title
open IRCBot.Modules.Sample
open IRCBot.Modules.Punto

open System

let funcs = [showLinksTitle; sorry; SIEGHEIL; sample; vote; punto; saveLastMessage]
let myBot = new IrcBot(server, port, channel, botNick, funcs)
myBot.loop ()
