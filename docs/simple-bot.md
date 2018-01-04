How to: write a bot (using this).

# What you need

## bash

You need `bash`.
If you have Linux or macOS, then you can go further.
For Windows: you can install https://cygwin.com, mingw, or https://git-scm.com/download/win.

## F#

See here: http://fsharp.org/

## Other

GNU Make and git.

# Installation

Clone repository:
```bash
$ git clone https://github.com/forked-from-1kasper/just-another-irc-bot
```

If you have Linux or macOS: open Makefile and change `FSHARP=fsc` to `FSHARP=fsharpc`.

Make all:
```bash
$ make
```

# Writing
Make a file:
```bash
$ touch mybot.fsx
```

Write to the top:
```fsharp
#r "IRCBot.dll"

open IRCBot
```

Specify the server, port, bot nickname, and the channel to which it will connect at startup:
```fsharp
let server = "chat.freenode.net"
let port = 6667

let channel = "#lor"
let nick = "just-nothing"
```

Initialize the bot:
```
let myBot = IrcBot({ server = server;
                     port = port;
                     channel = channel;
                     botNick = nick;
                     funcs = [];
                     mode = { order = Parallel;
                              debug = false };
                     regular = [];
                     period = 1000.0 })
```

Add it:
```fsharp
myBot.loop ()
```

And run it:
```bash
$ fsi mybot.fsx # for Windows
$ fsharpi mybot.fsx # for Linux, macOS
```

# Functions
Now the bot does nothing.

Make a function:
```fsharp
// the second argument appeared as a result of an erruneous desing and will be removed. Do not use it.
let test(msg, _) =
    match msg with
    // we get: information about the user (nickname and ident), information about the message (here: command, channel, and text)
    | Some({ nick = nick; ident = ident },
           { command = "PRIVMSG"; args = [ channel; text ] }) ->
           if text = "ping" then
               // we send: command, channel, and text
               [{ command = "PRIVMSG";
                  args = [ channel; ":pong!" ] };
                { command = "PRIVMSG";
                  args = [ channel; ":pong-2!" ] }]
           else []
    | _ -> []
```

Replace `funcs = []` with `funcs = [test]`.
Now if we write “ping”, the bot will respond with “pong” and “pong-2”!

# Regular functions
Make a function:

```fsharp
// this is some event
let timingEvent =
    // this is what happens
    let timing date =
        [{ command = "PRIVMSG";
           args = [ channel;
                    sprintfn ":%A" time ] }]
    // this is a condition under which it occurs
    // if it returns true, it will occur
    let predicate date = true
    // “let predicate _ = true” looks better

    Event (timing, predicate)
```

Replace `regular = []` with `regular = [timingEvent]`.
Period is 1000 milliseconds (1 second): `period = 1000.0`.
Before `myBot.loop ()`, add `myBot.cron ()`.

Now the bot sends time every second.
Every second the bot checks the events and launches, if necessary.