#r "bot.dll"

open IRCBot
open System
open System.Text
open System.IO

let server = "chat.freenode.net"
let port = 6667
let channel = "#lor"
let nick = "Poehavshy"

let lines =
    use sr = new StreamReader ("megav2.txt")

    let rec support buf =
        if sr.EndOfStream then buf
        else
            if String.IsNullOrEmpty buf then
                support (sr.ReadLine ())
            else
                support (buf + " " + sr.ReadLine ())

    support ""

let makeOrAdd key elem (map : Map<_, _>) =
    if map.ContainsKey key then
        Map.add key (List.append elem map.[key]) map
    else
        Map.add key elem map

let DasIstMagic (someList : 'a array) =
    let magic (b, a) c = (c, (makeOrAdd b [c] a))

    Array.fold magic (someList.[0], [someList.[0], [someList.[1]]] |> Map.ofList) (Array.tail someList)

let mergeMap a (b : Map<_, _>) =
    let rec support acc temp =
        if List.isEmpty temp then
            acc
        else
            let key = List.head temp
            support (makeOrAdd key b.[key] acc) (List.tail temp)
            
    let keys = Map.toList b |> List.map (fun (a, _) -> a)
    support a keys

let megahitlersplit (str : string) =
    Array.chunkBySize 2 (str.Split [| ' ' |])
    |> Array.map Seq.ofArray
    |> Array.map (String.concat " ")

let wordsMap =
    lines
    |> fun x -> x.Split([| ". " |], StringSplitOptions.None)
    //|> Array.map (fun elem -> elem.Split([| ' ' |]))
    |> Array.map megahitlersplit
    |> Array.map (fun elem -> Array.append [| "*START*" |] elem
                              |> fun x -> Array.append x [| "*END*" |])
    |> Array.map DasIstMagic
    |> Array.map (fun (_, b) -> b)
    |> fun x -> Array.fold mergeMap (Array.head x) (Array.tail x)

let makeShiz (map : Map<_, _>) =
    let rec support acc current =
        let (currentList : List<_>) = map.[current]
        let next =
            let rnd = Random ()
            currentList.[rnd.Next (currentList.Length)]

        if next = "*END*" then
            acc + "?"
        else
            if String.IsNullOrEmpty acc then
                support next next
            else
                support (acc + " " + next) next

    support "" "*START*"

let rec degenerate () =
    let current = makeShiz wordsMap
    if Encoding.UTF8.GetByteCount current > 256 then
        degenerate ()
    else
        current

let (|Prefix|_|) (p : string) (s : string) =
    if s.StartsWith p then
        if p.Length + 1 > s.Length then
            Some ""
        else
            Some (s.Substring(p.Length + 1))
    else
        None

let trigger = ["ты"; "python"; "блядь";
               "фап"; "линукс"; "шиндошс";
               "сперма"; "зига"; "тимка";
               "сосёт"; "сосет"; "генту";
               "слака"; "ЛОР"; "овцы";
               "мамбет"; "#"; "геи";
               "гей"; "пидор"; "пидорас"]

let lenin msg channel =
    match msg with
    | Some({nick = "awesomelackware"},
           { command = "PRIVMSG"; text = Some("!die")}) ->
        Some { command = "QUIT"; subject = None; text = None }
    | Some(_, { command = "PRIVMSG"; text = Some(text)}) ->
        match text with
        | Prefix "!version" _ ->
            Some { command = "PRIVMSG";
                   subject = Some channel;
                   text = Some (sprintf "I on %A" Environment.Version) }
        | Prefix "!date" _ ->
            Some { command = "PRIVMSG";
                   subject = Some channel;
                   text = Some (sprintf "%A" System.DateTime.Now) }
        | Prefix "!help" _ ->
            Some { command = "PRIVMSG";
                   subject = Some channel;
                   text = Some "Google it!" }
        | Prefix "!echo" rest ->
            Some { command = "PRIVMSG";
                   subject = Some channel;
                   text = Some rest }
        | Prefix nick _ ->
            Some { command = "PRIVMSG";
                   subject = Some channel;
                   text = Some (degenerate ()) }
        | s when (List.map s.Contains trigger
                  |> List.exists ((=) true)) ->
            Some { command = "PRIVMSG";
                   subject = Some channel;
                   text = Some (degenerate ()) }
        | _ -> None
    | None | Some(_) -> None

let funcs = [lenin]
let myBot = new IrcBot(server, port, channel, nick, funcs)
myBot.loop ()
