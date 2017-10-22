#r "bot.dll"
#r "FSharp.Data.dll"

open IRCBot
open System
open System.Text
open System.Text.RegularExpressions
open System.Net
open System.IO
open FSharp.Data

let server = "chat.freenode.net"
let port = 6667
let channel = "#lor"
let nick = "NurNochMal"

let lines =
    use sr = new StreamReader ("string.txt")

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
    |> Array.map (String.concat " ")

let mutable wordsMap =
//    [("*START*", ["Да"]);
//     ("Да", ["*END*"])] |> Map.ofList
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
            acc
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
        [{ command = "QUIT"; args = [] }]

    | Some (_, { command = "PRIVMSG"; args = [nickArg; text] })
      when nickArg = nick ->
        [{ command = "PRIVMSG";
           args = [ channel;
                    sprintf ":%s" (degenerate ()) ] }]

    | Some(_, { command = "PRIVMSG"; args = [_; text] }) ->
        match text with
        | Prefix "!version" _ ->
            [{ command = "PRIVMSG";
               args = [ channel;
                        sprintf ":I on %A" Environment.Version ] }]
        | Prefix "!date" _ ->
            [{ command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%A" System.DateTime.Now ] }]
        | Prefix "!help" _ ->
            [{ command = "PRIVMSG";
               args = [ channel; ":Google it!" ] }]
        | Prefix "!echo" rest ->
            [{ command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%s" rest ] }]
        | Prefix nick _ ->
            [{ command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%s" (degenerate ()) ]} ]
        | s when (List.map s.Contains trigger
                  |> List.exists ((=) true)) ->
            [{ command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%s" (degenerate ()) ] }]
        | _ -> []
    | _ -> []

let learn msg channel =
    match msg with
    | Some(_, { command = "PRIVMSG"; args = [_; text] }) ->
        if text.Contains nick then []
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
            []
    | _ -> []

let isGay msg channel =
    match msg with
    | Some({ nick = nick },
           { command = "PRIVMSG"; args = [_; text] }) ->
        match text with
        | Prefix "!isGay" rest ->
            let isPidoras = function
                | "timdorohin" -> "true"
                | _ -> "false"

            [{ command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%s: %s" nick (isPidoras rest) ] }]
        | _ -> []
    | _ -> []

let SIEGHEIL msg channel =
    match msg with
    | Some({ nick = nick },
           { command = "JOIN"; args = [_] }) ->
        [{ command = "PRIVMSG";
           args = [ channel;
                    sprintf ":%s: SIEG HEIL" nick ]}]
    | _ -> []

let sorry msg channel =
    match msg with
    | Some({ nick = nick },
           { command = "PRIVMSG"; args = [_; text] }) ->
        match text with
        | Prefix "!пожалеть" _ ->
            [{ command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%s: Пожалел тебе за щёку." nick ] };
             { command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%s: Проверяй." nick ] }]
        | _ -> []
    | _ -> []

let admin msg channel =
    match msg with
    | Some({ nick = nick },
           { command = "PRIVMSG"; args = [_; text] }) when
      List.contains nick ["awesomelackware"; "timdorohin"] ->
          match text with
          | Prefix "!дайодменку" _ ->
              [{ command = "MODE";
                 args = [ channel; "+o"; nick ] }]
          | _ -> []
    | _ -> []


let mutable questions = ["Сколько гигабайт оперативки нужно, чтобы запустить Atom?"]
let mutable options = [(([("0.1", 0);
                          ("1", 0);
                          ("2", 0);
                          ("4", 0);
                          ("8", 0);
                          ("16", 0);
                          ("32", 0);
                          ("64", 0);
                          ("128", 0);
                          (">128", 0)] |> Map.ofList), "awesomelackware")]
let mutable (alreadyVoted : List<int * string>) = []

let help = ":!results номер; !vote номер/вариант; !question номер; !newquestion вопрос/вариант1 вариант2 …; !removequestion номер; !allquestions;"
let vote msg channel =
    match msg with
    | Some({ ident = ident; nick = nick },
           { command = "PRIVMSG"; args = [_; text] }) ->
        match text with
        | Prefix "!results" optionString ->
            match Int32.TryParse optionString with
            | (true, option) when (option >= 0) && (option < options.Length) ->
                let toPrint =
                    let (current, _) = options.[option]
                    Map.toList current
                    |> List.map (fun (vote, result) -> sprintf "«%s»: %d" vote result)
                    |> String.concat "; "

                [{ command = "PRIVMSG";
                   args = [ channel;
                            sprintf ":%s → %s" questions.[option] toPrint ] }]

            | _ -> []
            
        | Prefix "!vote" rest ->
            match rest.Split [| '/' |] with
            | [| optionString; key |] ->
                match Int32.TryParse optionString with
                | (true, option) when (option < options.Length) && (option >= 0) ->
                    let (currentOptions, author) = options.[option]
                    if (currentOptions.ContainsKey key) &&
                       (not (List.contains (option, ident) alreadyVoted)) then
                        options <- List.mapi (fun index elem ->
                                                  if (index = option) then
                                                      (Map.add key (currentOptions.[key] + 1) currentOptions,
                                                       author)
                                                  else
                                                      elem) options
                        alreadyVoted <- List.append alreadyVoted [(option, ident)]
                        ()
                    []
                | _ -> []
            | _ -> []
            
        | Prefix "!question" optionString ->
            match Int32.TryParse optionString with
            | (true, option) ->
                if (option < options.Length) &&
                   (option >= 0) then
                    [{ command = "PRIVMSG";
                       args = [ channel;
                                sprintf ":%s" questions.[option] ] }]
                else []
            | _ -> []
            
        | Prefix "!newquestion" rest ->
            let make question variants =
                questions <- List.append questions [question]
                options <- List.append options [((Array.map (fun s -> (s, 0)) variants
                                                  |> Map.ofArray), ident)]

            match rest.Split [| '/' |] with
            | [| question; variants |] ->
                make question (variants.Split [| ' ' |])
                []
            | [| question |] ->
                make question [| "Да"; "Нет" |]
                []
            | _ -> []

        | Prefix "!removequestion" optionString ->
            match Int32.TryParse optionString with
            | (true, option) ->
                if (option < options.Length) && (option >= 0) then
                    let (_, author) = options.[option]
                    if (ident = author) || (nick = "awesomelackware") then
                        if (options.Length = 1) || (options.Length = 0) then
                            options <- []
                            questions <- []
                            alreadyVoted <- []
                        else
                            options <- List.append options.[..option-1] options.[option+1..]
                            questions <- List.append questions.[..option-1] questions.[option+1..]
                            alreadyVoted <- List.filter (fun (x, _) -> x <> option) alreadyVoted
                        ()
                    []
                else
                    []
            | _ -> []

        | Prefix "!allquestions" _ ->
            let toPrint =
                List.mapi (fun index s -> sprintf "%d: «%s»" index s) questions
                |> String.concat "; "
            [{ command = "PRIVMSG";
               args = [ channel;
                        sprintf ":%s" toPrint ]}]

        | Prefix "!helpVote" _ ->
            [{ command = "PRIVMSG";
               args = [ channel; help ] }]

        | _ -> []
    | _ -> []

let fixLayout s =
    let latinToCyrillic =
        [
         ('q', 'й'); ('w', 'ц');
         ('e', 'у'); ('r', 'к');
         ('t', 'е'); ('y', 'н');
         ('u', 'г'); ('i', 'ш');
         ('o', 'щ'); ('p', 'з');
         ('[', 'х'); (']', 'ъ');
         ('a', 'ф'); ('s', 'ы');
         ('d', 'в'); ('f', 'а');
         ('g', 'п'); ('h', 'р');
         ('j', 'о'); ('k', 'л');
         ('l', 'д'); (';', 'ж');
         ('z', 'я'); ('x', 'ч');
         ('c', 'с'); ('v', 'м');
         ('b', 'и'); ('n', 'т');
         ('m', 'ь'); (',', 'б');
         ('.', 'ю'); ('/', '.');
         ('?', '.')] |> Map.ofList

    let convert (c : char) =
        let fix (c : char) =
            if latinToCyrillic.ContainsKey c then latinToCyrillic.[c]
            else c

        if Char.IsUpper c then
            fix (Char.ToLower c) |> Char.ToUpper
        else
            fix c

    String.map convert s

let mutable (lastMessages : Map<string, string>) = [] |> Map.ofList

let punto msg channel =
    match msg with
    | Some({ nick = nick; ident = ident },
           { command = "PRIVMSG"; args = [_; text] }) ->
        match text with
        | Prefix "!fix" _ ->
            if lastMessages.ContainsKey nick then
                [{ command = "PRIVMSG";
                   args = [ channel;
                            sprintf ":fixed: %s" (fixLayout lastMessages.[nick]) ] }]
            else []
        | _ -> []
    | _ -> []

let saveLastMessage msg channel =
    match msg with
    | Some({ nick = nick },
           { command = "PRIVMSG"; args = [_; text] }) ->
        lastMessages <- Map.add nick text lastMessages
        []
    | _ -> []

//let wc = new WebClient ()
//let titlePattern = @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>"
let getTitle (link : string) =
    try
        //let source = wc.DownloadString link
        //Some (Regex.Match(source, titlePattern, RegexOptions.IgnoreCase).Groups.["Title"].Value)
        let results = HtmlDocument.Load(link)
        results.Descendants ["title"]
        |> Seq.head
        |> (fun x -> x.InnerText())
        |> Some
    with
        | _ as ex ->
            printfn "Unhandled Exception: %s" ex.Message
            None

let showLinksTitle msg channel =
    match msg with
    | Some (_, { command = "PRIVMSG"; args = [_; text] }) ->
        text.Split [| ' ' |]
        |> List.ofArray
        |> List.filter (fun s ->
                        (s.StartsWith "http://"))
        |> List.map getTitle
        |> List.filter (fun x -> x.IsSome)
        |> List.map (fun x -> x.Value)
        |> List.map (fun x ->
                         { command = "NOTICE";
                           args = [ channel;
                                    sprintf ":Title: %s" x ] })
    | _ -> []

let funcs = [showLinksTitle; sorry]
let myBot = new IrcBot(server, port, channel, nick, funcs)
myBot.loop ()
