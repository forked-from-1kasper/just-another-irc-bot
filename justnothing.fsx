#r "IRCBot.dll"

open IRCBot

let server = "chat.freenode.net"
let port = 6667
let channel = "#lor"
let nick = "just-nothing"

let funcs = []
let myBot = IrcBot({ server = server;
                     port = port;
                     channel = channel;
                     botNick = nick;
                     funcs = funcs;
                     mode = { order = Parallel;
                              debug = false };
                     regular = [];
                      period = 1000.0})
myBot.loop ()
