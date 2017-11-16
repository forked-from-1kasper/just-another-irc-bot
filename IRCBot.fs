module IRCBot

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Text.RegularExpressions

type Person = { nick : string;
                ident : string}

type Message = { command : string;
                 args : List<string> }

type VesitorMessage = Person * Message

let private ircPing (writer : StreamWriter) server =
    writer.WriteLine(sprintf "PONG %s\n" server)

let private ircPrivmsg (writer : StreamWriter) channel msg =
    writer.WriteLine(sprintf "PRIVMSG %s :%s\n" channel msg)

let private ircGetMsg (line : string) =
    line.Substring(line.Substring(1).IndexOf(":") + 2)

let private ircParseMsg (line : string) =
    let rx = new Regex(@":(\S+)!(\S+) (\S+) (\S+) ?:?(.*)")
    //                     nick!ident comnd chan  teeeext

    let matches = rx.Match line
    if matches.Success then
        let values = List.tail [ for g in matches.Groups -> g.Value ]
        let text = values.[4]
        let args =
            if System.String.IsNullOrEmpty text then
                [ values.[3] ]
            else
                [ values.[3]; text ]
        
        Some ({ nick = values.[0];
                ident = values.[1] },
              { command = values.[2];
                args = args })
    else
        None

let private messageToString { command = command;
                              args = args} =

    let concated = args |> String.concat " "
    sprintf "%s %s" command concated

let public bindAsyncFunctions(funcs) =
    fun (msg, channel) ->
        List.fold (fun last func -> List.append last (func(msg, channel))) [] funcs

type public IrcBot(server : string, port, channel, nick, funcs) =
    member this.server = server
    member this.port = port
    member this.channel = channel
    member this.nick = nick
    member this.funcs = funcs

    member this.loop () =
        let ircClient = new TcpClient()
        ircClient.Connect(this.server, this.port)
        
        let ircReader = new StreamReader(ircClient.GetStream())
        let ircWriter = new StreamWriter(ircClient.GetStream())

        ircWriter.WriteLine(sprintf "USER %s %s %s %s" this.nick this.nick this.nick this.nick)
        ircWriter.AutoFlush <- true
        ircWriter.WriteLine(sprintf "NICK %s" this.nick)
        ircWriter.WriteLine(sprintf "JOIN %s" this.channel)

        let wrapper func msg channel =
            async {
                return (func (msg, channel)
                        |> List.map messageToString)
            }

        while ircReader.EndOfStream = false do
            let line = ircReader.ReadLine ()
            printfn "%s" line
            
            let msg = ircParseMsg line

            if line.Contains "PING" then
                ircPing ircWriter this.server

            List.map (fun func -> wrapper func msg channel) funcs
            |> Async.Parallel
            |> Async.RunSynchronously
            |> Array.filter ((<>) [])
            |> List.ofArray
            |> List.concat
            |> List.iter (fun x -> ircWriter.WriteLine(x))
    
            //List.map (fun x -> x msg channel) funcs
            //|> List.concat
            //|> List.map (fun x -> messageToString x)
            //|> List.iter (fun x -> ircWriter.WriteLine(x))
            
            //|> List.iter (fun x ->
            //              match x with
            //              | Some(s) -> ircPrivmsg ircWriter this.channel s
            //              | None -> ())
