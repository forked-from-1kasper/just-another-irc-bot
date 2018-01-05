module Markov

open System
open System.IO
open System.Text

open IRCBot
open IRCBot.Public.Constants
open IRCBot.Public.Prefix

open Newtonsoft.Json

let DBLocation = "db.json"

let serializer = Newtonsoft.Json.JsonSerializer ()

let saveDB (fileName : string) db =
    use sw = new StreamWriter (fileName)
    serializer.Serialize ((new JsonTextWriter (sw)), db)

let loadDB (fileName : string) =
    JsonConvert.DeserializeObject<Map<string, List<string>>>(File.ReadAllText (fileName))

let mutable wordsMap = loadDB DBLocation

//let lines =
//    use sr = new StreamReader ("lurka.txt")
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

let megahitlersplit (str : string) =
    Array.chunkBySize 1 (str.Split [| ' ' |])
    |> Array.map (String.concat " ")    

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

//let mutable wordsMap =
//    [("*START*", ["Да"]);
//     ("Да", ["*END*"])] |> Map.ofList
//    //lines
//    //|> fun x -> x.Split([| ". " |], StringSplitOptions.None)
//    //|> Array.map megahitlersplit
//    //|> Array.map (fun elem -> Array.append [| "*START*" |] elem
//    //                          |> fun x -> Array.append x [| "*END*" |])
//    //|> Array.map DasIstMagic
//    //|> Array.map (fun (_, b) -> b)
//    //|> fun x -> Array.fold mergeMap (Array.head x) (Array.tail x)

let rnd = Random ()
let makeShiz (map : Map<_, _>) =
    let rec support acc current =
        let (currentList : List<_>) = map.[current]
        let next =
            currentList.[rnd.Next (currentList.Length)]

        if next = "*END*" then
            acc
        else
            if String.IsNullOrEmpty acc then
                support next next
            else
                support (acc + " " + next) next

    support "" "*START*"

let rec public degenerate () =
    let current = makeShiz wordsMap
    if Encoding.UTF8.GetByteCount current > 256 then
        degenerate ()
    else
        current

let public trigger = ["ты"; "python"; "блядь";
                      "фап"; "линукс"; "шиндошс";
                      "сперма"; "зига"; "тимка";
                      "сосёт"; "сосет"; "генту";
                      "слака"; "ЛОР"; "овцы";
                      "мамбет"; "#"; "геи";
                      "гей"; "пидор"; "пидорас"]

let public learn(msg) =
    match msg with
    | _, Some { command = "PRIVMSG"; args = [_; text] } ->
        if (text.Contains botNick |> not) &&
           (text.StartsWith "!" |> not) then
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
