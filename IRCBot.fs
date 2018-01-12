module IRCBot

open System
open System.IO
open System.Net.Sockets
open System.Text.RegularExpressions

type Person = { nick : string;
                ident : string}

type Message = { command : string;
                 args : List<string> }

type VesitorMessage = Person * Message

type Order = Parallel | Consistently
type BotMode =
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
    let rx = Regex(@":(\S+)!(\S+) (\S+) (\S+) ?:?(.*)")
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
        
        (Some { nick = values.[0];
                ident = values.[1] },
         Some { command = values.[2];
                args = args })
    else
        None, None

let private messageToString { command = command;
                              args = args} =

    let concated = args |> String.concat " "
    sprintf "%s %s" command concated

let public bindAsyncFunctions(funcs) =
    fun (msg) ->
        List.fold (fun last func -> List.append last (func (msg))) [] funcs

let private wrapper func msg =
    async {
        return (func msg
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

type AEvent =
    Event of (DateTime -> Message list) * (DateTime -> bool)
type BotDescription =
    { server : string;
      port : int;
      channel : string;
      botNick : string;
      funcs : List<(Person option * Message option) -> Message list>;
      mode : BotMode;
      regular : List<AEvent>;
      period : float}

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

    member __.Desc = desc
    member __.IrcClient = client
    member __.Reader = ircReader
    member __.Writer = ircWriter

    member this.Cron () =
        //while true do
        let timerHandler () =
            let now = DateTime.Now
            let goodFuncs =
                this.Desc.regular
                |> List.filter (fun (Event (_, predicat)) -> predicat now)
                |> List.map (fun (Event (func, _)) -> func)
    
            if this.Desc.mode.order = Parallel then
                goodFuncs
                |> List.map(fun func -> async {
                                         return (func now
                                                 |> List.map messageToString)
                                     })
                |> parallelProcessing this.Writer
            else
                goodFuncs
                |> List.map(fun func -> func now)
                |> simpleProcessing this.Writer
            //Thread.Sleep this.desc.period
        let timer = new System.Timers.Timer(this.Desc.period)
        timer.Elapsed.Add(fun _ -> timerHandler ())
        timer.Start ()

    member this.Loop () =
        while not this.Reader.EndOfStream do
            let line = this.Reader.ReadLine ()
            printfn "- %s" line
            
            let msg = ircParseMsg line

            if line.StartsWith "PING" then
                match (line.Split [| ' ' |] |> List.ofArray) with
                    | ["PING"; server] -> ircPing this.Writer server // server must be without spaces
                    | _ -> ()

            let stopwatch = System.Diagnostics.Stopwatch()
            if this.Desc.mode.debug then
                stopwatch.Start()

            if this.Desc.mode.order = Parallel then
                List.map (fun func ->
                          wrapper func msg) this.Desc.funcs
                // TODO: remove this.desc.channel here
                |> parallelProcessing this.Writer
            else
                List.map (fun x -> x msg) this.Desc.funcs
                |> simpleProcessing this.Writer
            
            if this.Desc.mode.debug then
                stopwatch.Stop()
                printfn "(%f ms)" stopwatch.Elapsed.TotalMilliseconds

type ChatBuilder() =
    member __.Return(v) = v
    member __.Bind (v, f) =
        match v with
        | Some v' -> f v'
        | None -> []
let chat = ChatBuilder()