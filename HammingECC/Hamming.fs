module Hamming

open System
open System.Collections
open System.IO
open System.Text
open ArrayExtension

(*  ALGORYTM KODOWANIA
         0 1 2 3 4 5 6 7 
       0 _ _ _ 0 _ 1 0 1
       8 _ 1 1 0 0 1 0 1 
      16 _ 1 1 0 0 1 0 1
      24 0 1 1 0 0 1 0 1
      

    dane: 26 bitów
    sumy kontrolne: 5 + 1 bitów

    w pole 0 wstawiamy sume kontrolną wszystkich pozostałych bitów
    w pole 1 wstawiamy sumę po kolumnach 1 3 5 7
    w pole 2 wstawiamy sumę po kolumnach 2 3 6 7
    w pole 4 wstawiamy sumę po kolumnach 4 5 6 7
    w pole 8 wstawiamy sumę po wierszach 8 24
    w pole 16 wstawiamy sumę po wierszach 16 24
*)

type Mode = 
| ENCODE = 0
| DECODE = 1
| VERIFY = 2

type Hamming() = 

    (* właściwości implementowane automatyczne *)
    member val mode = Mode.ENCODE with get, set 
    member val fileName = "" with get, set
    member val time = false with get, set
    member val verbose = false with get, set

    (* metody *)
    member private this.ComputeECC (data:string) = 
        let bitArray = data.[0..31]
                        |> (fun (x:string) -> x.ToCharArray()) 
                        |> Array.map (fun x -> int (Char.GetNumericValue x))
            // 4. dorobić bity parzystości
        for it in seq{1; 2; 4; 8; 16} do
            bitArray.[it] <- bitArray 
                            |> ArrayExtension.filteri (fun x -> (x &&& it = it))
                            |> Array.fold (fun x y -> x ^^^ y) 0 
        bitArray.[0] <- bitArray |> Array.fold (fun x y -> x ^^^ y) 0
            // 5. zapisać jako 4 bajty do pliku 
        String.Join("", bitArray) 
        

    member this.Encode() =
        let data = File.ReadAllBytes(this.fileName)
        let outputFile = new BinaryWriter (File.OpenWrite(this.fileName + ".ecc"))
        let mutable offset = 0;
        let mutable remainBits = ""
    
        while ( ((offset + 2) < data.Length && remainBits.Length > 0) || ((offset + 3) < data.Length) ) do
            // 1. przygotować nową porcję bitów (26 bitów z)
            let strBuild = new StringBuilder(remainBits, 32) 
            while strBuild.Length < 26 do 
                Convert.ToString(data.[offset], 2) 
                |> (fun (x:string) -> x.PadLeft(8, '0'))
                |> strBuild.Append |> ignore
                offset <- offset + 1
            //printfn "%s" (strBuild.ToString())
            // 2. dodać bity parzystości
            for i in seq{0; 1; 2; 4; 8; 16} do
                strBuild.Insert(i, '0') |> ignore
            // 3. nadmiarowe bity przenieś do następnej iteracji
            //printfn "%s" (strBuild.ToString())
            remainBits <- strBuild.ToString().[32..]
            let result = this.ComputeECC (strBuild.ToString())
            printfn "%s" result
            outputFile.Write (Convert.ToInt32(result, 2))
        // 6. dopisać ostatnie bity
        let strBuild = new StringBuilder(remainBits, 32) 
        while offset < data.Length do 
            Convert.ToString(data.[offset], 2) 
            |> (fun (x:string) -> x.PadLeft(8, '0'))
            |> strBuild.Append |> ignore
            offset <- offset + 1
        let concatEndSize = 26 - strBuild.Length
        for i = (32 - strBuild.Length) downto 1 do
            strBuild.Append("0") |> ignore 
        //printfn "%s" (strBuild.ToString())  
        for it in seq{0; 1; 2; 4; 8; 16} do
            strBuild.Insert(it, '0') |> ignore
        //printfn "%s" (strBuild.ToString())        
        let result = this.ComputeECC (strBuild.ToString())
        printfn "%s" result
        outputFile.Write (Convert.ToInt32(result, 2))
        outputFile.Write (byte concatEndSize)
        //8. zapisać plik i zamknąć    
        outputFile.Flush()  
        outputFile.Close() 
        

    member this.Decode() =
        if this.fileName.Contains(".ecc") then 
            let data = File.ReadAllBytes(this.fileName)
            let mutable offset = data.Length
            while offset < data.Length do
                0 |> ignore

    //może dobre na początek
    member this.Verify() =
        if this.fileName.Contains(".ecc") then 
            let data = new BinaryReader (File.OpenRead(this.fileName))
            data.ReadInt32()
            |> (fun x -> Convert.ToString(x, 2))
            |> (fun (x:string) -> x.PadLeft(32, '0'))
            |> printfn "%s"
            data.ReadInt32()
            |> (fun x -> Convert.ToString(x, 2))
            |> (fun (x:string) -> x.PadLeft(32, '0'))
            |> printfn "%s"
            data.Close()
