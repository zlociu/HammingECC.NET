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


[<EntryPoint>]
let main argv =
    let arguments = argv
    let mutable iter = 0
    let manager = new Hamming()
    while iter < arguments.Length do
        try
            match arguments.[iter] with
            | "-H" | "--help" -> 
                PrintHelp()
                exit 0
            | "-E" | "--encode" -> 
                manager.mode <- Mode.ENCODE
                iter <- iter + 1
            | "-D" | "--decode" -> 
                manager.mode <- Mode.DECODE
                iter <- iter + 1
            | "-C" | "--verify" -> 
                manager.mode <- Mode.VERIFY
                iter <- iter + 1
            | "-T" | "--time" -> 
                manager.time <- true
                iter <- iter + 1
            | "-V" | "--verbose" -> 
                manager.verbose <- true
                iter <- iter + 1
            | file ->
                if file.Contains('.') && not (file.StartsWith("-")) then 
                    manager.fileName <- file
                    iter <- iter + 1
                else
                    raise (ArgumentException("Command error"))
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