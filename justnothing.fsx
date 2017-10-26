#r "bot.dll"

open IRCBot
open System

let server = "chat.freenode.net"
let port = 6667
let channel = "##lorwiki"
let nick = "just-nothing"

let funcs = []
let myBot = new IrcBot(server, port, channel, nick, funcs)
myBot.loop ()
