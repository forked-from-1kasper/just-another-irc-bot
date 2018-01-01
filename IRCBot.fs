module IRCBot

open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Text.RegularExpressions
open System.Threading

type Person = { nick : string;
                ident : string}

type Message = { command : string;
                 args : List<string> }

type VesitorMessage = Person * Message

type Order = Parallel | Consistently
type botMode =
    { order: Order;
      debug : bool }

let private ircPing (writer : StreamWriter) server =
    printfn "+ PONG %s" server
    writer.WriteLine(sprintf "PONG %s\n" server)
    ()

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

let private wrapper func msg channel =
    async {
        return (func (msg, channel)
                |> List.map messageToString)
    }

let private getStream (client : TcpClient) =
    (new StreamReader(client.GetStream())),
    (new StreamWriter(client.GetStream()))

let silentPrint x =
    printfn "%A" x
    x

let private writeAndPrint (writer : StreamWriter) s =
    printfn "+ %s" s
    writer.WriteLine(s)

let private parallelProcessing (writer : StreamWriter) =
    Async.Parallel
    >> Async.RunSynchronously
    >> Array.filter ((<>) [])
    >> List.ofArray
    >> List.concat
    >> List.iter (writeAndPrint writer)

let private simpleProcessing (writer : StreamWriter) =
    List.concat
    >> List.map messageToString
    >> List.iter (writeAndPrint writer)

type botDescription =
    { server : string;
      port : int;
      channel : string;
      botNick : string;
      funcs : List<(Person * Message) option * string -> Message list>;
      mode : botMode;
      regular : List<DateTime -> Message list>;
      period : int}

type public IrcBot(desc) =
    let client = new TcpClient()        
    do client.Connect(desc.server, desc.port)
    do printfn "Connected!"

    let ircReader, ircWriter = getStream client
    do ircWriter.WriteLine (sprintf "USER %s %s %s %s" desc.botNick desc.botNick
                                                       desc.botNick desc.botNick)
    do ircWriter.AutoFlush <- true
    do ircWriter.WriteLine(sprintf "NICK %s" desc.botNick)
    do ircWriter.WriteLine(sprintf "JOIN %s" desc.channel)

    member this.desc = desc
    member this.ircClient = client
    member this.reader = ircReader
    member this.writer = ircWriter

    member this.cron () =
        while true do
            let now = DateTime.Now
    
            if this.desc.mode.order = Parallel then
                List.map(fun func -> async {
                                         return (func now
                                                 |> List.map messageToString)
                                     }) this.desc.regular
                |> parallelProcessing this.writer
            else
                List.map(fun func -> func now) this.desc.regular
                |> simpleProcessing this.writer
            Thread.Sleep this.desc.period

    member this.loop () =
        while ircReader.EndOfStream = false do
            let line = this.reader.ReadLine ()
            printfn "- %s" line
            
            let msg = ircParseMsg line

            if line.StartsWith "PING" then
                match (line.Split [| ' ' |] |> List.ofArray) with
                    | ["PING"; server] -> ircPing this.writer server // server must be without spaces
                    | _ -> ()

            let stopwatch = System.Diagnostics.Stopwatch()
            if this.desc.mode.debug then
                stopwatch.Start()

            if this.desc.mode.order = Parallel then
                List.map (fun func ->
                          wrapper func msg this.desc.channel) this.desc.funcs
                // TODO: remove this.desc.channel here
                |> parallelProcessing this.writer
            else
                List.map (fun x -> x (msg, this.desc.channel)) this.desc.funcs
                |> simpleProcessing this.writer
            
            if this.desc.mode.debug then
                stopwatch.Stop()

            printfn "(%f ms)" stopwatch.Elapsed.TotalMilliseconds
