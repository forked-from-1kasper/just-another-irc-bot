#r "IRCBot.dll"

open IRCBot

let server = "chat.freenode.net"
let port = 6667
let nick = "just-nothing"
let myBot = IrcBot({ server = server;
                     port = port;
                     botNick = nick;
                     ident = "nothing";
                     funcs = [];
                     mode = { order = Consistently;
                              debug = false };
                     regular = [];
                     period = 1000.0;
                     atStart = [] })
myBot.Loop ()
