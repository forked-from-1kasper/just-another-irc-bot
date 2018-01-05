module IRCBot.Modules.Vote
open IRCBot
open IRCBot.Public.Prefix

open System

let mutable private questions = ["Сколько гигабайт оперативки нужно, чтобы запустить Atom?"]
let mutable private options = [(([("0.1", 0);
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

let private help = ":!results номер; !vote номер/вариант; !question номер; !newquestion вопрос/вариант1 вариант2 …; !removequestion номер; !allquestions;"
let public vote(msg) =
    match msg with
    | Some { nick = nick; ident = ident },
      Some { command = "PRIVMSG"; args = [channel; text] } ->
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
