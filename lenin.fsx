#r "bot.dll"

open IRCBot
open System
open System.Text
open System.IO

let server = "chat.freenode.net"
let port = 6667
let channel = "#lor"
let nick = "Poehavshy"

//let lines =
//    use sr = new StreamReader ("megav2.txt")
//
//    let rec support buf =
//        if sr.EndOfStream then buf
//        else
//            if String.IsNullOrEmpty buf then
//                support (sr.ReadLine ())
//            else
//                support (buf + " " + sr.ReadLine ())
//
//    support ""

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
    |> Array.map (String.concat " ")

let mutable wordsMap =
    [("*START*", ["Да"]);
     ("Да", ["*END*"])] |> Map.ofList
//    lines
//    |> fun x -> x.Split([| ". " |], StringSplitOptions.None)
//    //|> Array.map (fun elem -> elem.Split([| ' ' |]))
//    |> Array.map megahitlersplit
//    |> Array.map (fun elem -> Array.append [| "*START*" |] elem
//                              |> fun x -> Array.append x [| "*END*" |])
//    |> Array.map DasIstMagic
//    |> Array.map (fun (_, b) -> b)
//    |> fun x -> Array.fold mergeMap (Array.head x) (Array.tail x)

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
           { command = "PRIVMSG"; args = [_; "!die"] }) ->
        Some { command = "QUIT"; args = [] }
    | Some(_, { command = "PRIVMSG"; args = [_; text] }) ->
        match text with
        | Prefix "!version" _ ->
            Some { command = "PRIVMSG";
                   args = [ channel;
                            sprintf ":I on %A" Environment.Version ] }
        | Prefix "!date" _ ->
            Some { command = "PRIVMSG";
                   args = [ channel;
                            sprintf ":%A" System.DateTime.Now ] }
        | Prefix "!help" _ ->
            Some { command = "PRIVMSG";
                   args = [ channel; ":Google it!" ] }
        | Prefix "!echo" rest ->
            Some { command = "PRIVMSG";
                   args = [ channel;
                            sprintf ":%s" rest ] }
        | Prefix nick _ ->
            Some { command = "PRIVMSG";
                   args = [ channel;
                            sprintf ":%s" (degenerate ()) ] }
        | s when (List.map s.Contains trigger
                  |> List.exists ((=) true)) ->
            Some { command = "PRIVMSG";
                   args = [ channel;
                            sprintf ":%s" (degenerate ()) ] }
        | _ -> None
    | _ -> None

let learn msg channel =
    match msg with
    | Some(_, { command = "PRIVMSG"; args = [_; text] }) ->
        if text.Contains nick then None
        else
            let textWithoutNick =
                // remove nick from "nick: xxx"
                let index = text.IndexOf ":" // nick: xxx xxx xxx
                                             //     ^
                if (index < 0) ||
                   (index + 1 > text.Length) ||
                   (index + 2 > text.Length) then
                    text
                else
                    text.Substring (index + 2) // nick: xxx xxx xxx
                                               //     012
                    
            let newKey =
                textWithoutNick
                |> (fun s -> if s.EndsWith "." ||
                                s.EndsWith "?" ||
                                s.EndsWith "!" then s.Remove (s.Length - 1)
                             else s)
                |> megahitlersplit
                |> (fun elem -> Array.append [| "*START*" |] elem
                                |> fun x -> Array.append x [| "*END*" |])
                |> DasIstMagic
                |> fun (_, b) -> b
    
            wordsMap <- mergeMap wordsMap newKey
            None
    | _ -> None

let isGay msg channel =
    match msg with
    | Some({ nick = nick },
           { command = "PRIVMSG"; args = [_; text] }) ->
        match text with
        | Prefix "!isGay" rest ->
            let isPidoras = function
                | "timdorohin" -> "true"
                | _ -> "false"

            Some { command = "PRIVMSG";
                   args = [ channel;
                            sprintf ":%s: %s" nick (isPidoras rest) ] }
        | _ -> None
    | _ -> None

let SIEGHEIL msg channel =
    match msg with
    | Some({ nick = nick },
           { command = "JOIN"; args = [_] }) ->
        Some { command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%s: SIEG HEIL" nick ]}
    | _ -> None

let sorry msg channel =
    match msg with
    | Some({ nick = nick },
           { command = "PRIVMSG"; args = [_; text] }) ->
        match text with
        | Prefix "!пожалеть" _ ->
            Some { command = "PRIVMSG";
                   args = [ channel;
                            sprintf ":%s: Пожалел тебе за щёку." nick ] }
        | _ -> None
    | _ -> None

let admin msg channel =
    match msg with
    | Some({ nick = nick },
           { command = "PRIVMSG"; args = [_; text] }) when
      List.contains nick ["awesomelackware"; "timdorohin"] ->
          match text with
          | Prefix "!дайодменку" _ ->
              Some { command = "MODE";
                     args = [ channel; "+o"; nick ] }
          | _ -> None
    | _ -> None


let question = "Выборы в президенты АСАШАЙ"
let mutable options = [("Трамп", 0);
                       ("Хиллари", 0);
                       ("Шоман", -7)] |> Map.ofList
let mutable (alreadyVoted : List<string>) = []

let vote msg channel =
    match msg with
    | Some({ ident = ident },
           { command = "PRIVMSG"; args = [_; text] }) ->
        match text with
        | Prefix "!results" _ ->
            Some { command = "PRIVMSG";
                   args = [ channel;
                            sprintf ":%s" (options.ToString ()) ] }
        | Prefix "!vote" key ->
            if (options.ContainsKey key) && (not (List.contains ident alreadyVoted)) then
                options <- Map.add key (options.[key] + 1) options
                alreadyVoted <- ident :: alreadyVoted
                ()
            None
        | Prefix "!question" _ ->
            Some { command = "PRIVMSG";
                   args = [ channel;
                            sprintf ":%s" question ] }
        | _ -> None
    | _ -> None

let funcs = [vote]
let myBot = new IrcBot(server, port, channel, nick, funcs)
myBot.loop ()
