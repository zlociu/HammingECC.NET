open System.IO
open System

let argv = fsi.CommandLineArgs

let change x off p = 
    if p < 3 then (x ^^^ (1 <<< off))
    else x

let mutable offset = 0
let input = new BinaryReader(File.OpenRead(argv.[1]))
let output = new BinaryWriter(File.OpenWrite("broken_" + argv.[1]))
let rnd = new Random()
let mutable loop = true

while loop do
    try
        input.ReadInt32()
        |> (fun x -> change x (rnd.Next(1,32)) (rnd.Next(1,10)) )
        |> output.Write
    with
    | ex -> 
        //printfn "%s" ex.Message
        loop <- false;
        input.BaseStream.Position <- (input.BaseStream.Position - int64 1)
        input.ReadByte() |> output.Write
        output.Flush();
        output.Close();
        input.Close();
        