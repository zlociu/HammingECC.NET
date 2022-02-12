// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp
module Program

open System
open Hamming

// print help in command line
let PrintHelp() = 
    printfn "ECC with Hamming Code - v1.0.1"
    printfn "usage: ./hammingecc.exe [-E|-D|-C] <options> filename"
    printfn "-H --help\t\t show this help info"
    printfn "-E --encode\t\t save file as .ecc"
    printfn "-D --decode\t\t read .ecc file and save decoded file"
    printfn "-C --verify\t\t check if .ecc file has corrupted bits"
    printfn "-T --time\t\t show process time"
    printfn "-V --verbose\t\t show debug output"

let rec ProcessArguments (manager:Hamming) (arguments: List<string>) =
    match arguments with
    | "-H" :: _  | "--help" :: _ -> 
        PrintHelp()
        exit 0
    | "-E" :: list | "--encode" :: list -> 
        manager.mode <- Mode.ENCODE
        ProcessArguments manager list
    | "-D" :: list | "--decode" :: list -> 
        manager.mode <- Mode.DECODE
        ProcessArguments manager list
    | "-C" :: list | "--verify" :: list -> 
        manager.mode <- Mode.VERIFY
        ProcessArguments manager list
    | "-T" :: list | "--time" :: list -> 
        manager.time <- true
        ProcessArguments manager list
    | "-V" :: list | "--verbose" :: list -> 
        manager.verbose <- true
        ProcessArguments manager list
    | "--" :: file :: list->
        if file.Contains('.') then 
            manager.fileName <- file
            ProcessArguments manager list
        else
            raise (ArgumentException("Command error"))
    | file :: list ->
        if file.Contains('.') && not (file.StartsWith("-")) then 
            manager.fileName <- file
            ProcessArguments manager list
        else
            raise (ArgumentException("Command error"))
    | [] -> 0 |> ignore

[<EntryPoint>]
let main argv =
    let arguments = argv
    let mutable iter = 0
    let manager = new Hamming()
    try
        (Array.toList arguments) |> (ProcessArguments manager)   
    with 
    | ex -> 
        printfn "%s" ex.Message
        exit -1
    try
        if manager.fileName = "" then raise (ArgumentException("No input file"))
        match manager.mode with 
        | Mode.ENCODE -> 
            manager.Encode()
        | Mode.DECODE -> 
            manager.Decode()
        | Mode.VERIFY -> 
            manager.Verify()
        | _ -> exit 0
    with 
    | ex -> printfn "%s" (ex.Message)

    0 // return an integer exit code