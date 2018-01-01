#r "IRCBot.dll"

open IRCBot
open System

let server = "chat.freenode.net"
let port = 6667
let channel = "#lor"
let nick = "just-nothing"

let funcs = []
let myBot = new IrcBot({ server = server;
                         port = port;
                         channel = channel;
                         botNick = nick;
                         funcs = funcs;
                         mode = { order = Parallel;
                                  debug = false };
                         regular = [];
                         peroid = 1000})
myBot.loop ()
