module Hamming

open System
open System.IO
open System.Text
open System.Diagnostics
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
        
    member private this.ComputeXOR (data: string) =
        data
        |> (fun (x:string) -> x.ToCharArray()) 
        |> (fun x -> ArrayExtension.filter_it '1' x)
        |> Array.fold (fun x y -> x^^^y) 0     

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
            if this.verbose = true then printfn "%s" result
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
        if this.verbose = true then printfn "%s" result
        outputFile.Write (Convert.ToInt32(result, 2))
        outputFile.Write (byte concatEndSize)
        //8. zapisać plik i zamknąć    
        outputFile.Flush()  
        outputFile.Close() 
        

    member this.Decode() =
        if this.fileName.Contains(".ecc") then 
            let s1 = Stopwatch()
            let mutable offset = 0
            let mutable remainBits = ""
            let data = File.ReadAllBytes(this.fileName)
            let file = File.OpenWrite(this.fileName.Substring(0, this.fileName.Length - 4))
            s1.Start()
            while offset + 5 < data.Length do
                let strBuild = new StringBuilder(32)
                for it = 3 downto 0 do 
                    Convert.ToString(data.[offset + it], 2) 
                    |> (fun (x:string) -> x.PadLeft(8, '0'))
                    |> strBuild.Append |> ignore
                offset <- offset + 4
                if this.verbose = true then printfn "%s" (strBuild.ToString())
                let xor_res = (strBuild.ToString()) |> this.ComputeXOR 
                if not (xor_res = 0) then
                    if strBuild.Chars(xor_res) = '0' then 
                        strBuild.Chars(xor_res) <- '1'
                    else 
                        strBuild.Chars(xor_res) <- '0'
                for it in seq{16; 8; 4; 2; 1; 0} do // robimy od konca, bo problemy z indeksami
                    strBuild.Remove(it, 1) |> ignore
                strBuild.Insert(0, remainBits) |> ignore
                let result = strBuild.ToString()
                for i = 0 to (result.Length / 8) - 1 do
                    file.WriteByte(Convert.ToByte(result.Substring(i * 8, 8), 2))
                if result.Length < 32 then remainBits <- result.[24..]
                else remainBits <- result.[32..]
            printfn "%d %d" offset (data.Length)
            // koniec while   
            let strBuild = new StringBuilder(32)
            for it = 3 downto 0 do 
                Convert.ToString(data.[offset + it], 2) 
                |> (fun (x:string) -> x.PadLeft(8, '0'))
                |> strBuild.Append |> ignore
            offset <- offset + 4
            let nadmiar = data.[offset]
            if this.verbose = true then printfn "%s" (strBuild.ToString()) 
            let xor_res = (strBuild.ToString()) |> this.ComputeXOR 
            if not (xor_res = 0) then
                if strBuild.Chars(xor_res) = '0' then 
                    strBuild.Chars(xor_res) <- '1'
                else 
                    strBuild.Chars(xor_res) <- '0'
            for it in seq{16; 8; 4; 2; 1; 0} do // robimy od konca, bo problemy z indeksami
                strBuild.Remove(it, 1) |> ignore
            strBuild.Remove(strBuild.Length - int nadmiar, int nadmiar) |> ignore    
            strBuild.Insert(0, remainBits) |> ignore
            let result = strBuild.ToString()
            printf "%d" result.Length
            for i = 0 to (result.Length / 8) - 1 do
                file.WriteByte(Convert.ToByte(result.Substring(i * 8, 8), 2))
            s1.Stop()
            file.Flush()
            file.Close()

    member this.Verify() =
        if this.fileName.Contains(".ecc") then 
            let s1 = Stopwatch()
            let mutable licznik = (0, 0)
            let mutable offset = 0
            let data = File.ReadAllBytes(this.fileName)
            s1.Start()
            while(offset + 3 < data.Length) do
                let strBuild = new StringBuilder(32)
                for it = 3 downto 0 do 
                    Convert.ToString(data.[offset + it], 2) 
                    |> (fun (x:string) -> x.PadLeft(8, '0'))
                    |> strBuild.Append |> ignore
                offset <- offset + 4
                if this.verbose = true then printfn "%s" (strBuild.ToString())
                let xor_res = (strBuild.ToString()) |> this.ComputeXOR 
                if not (xor_res = 0) then
                    licznik <- (fst licznik + 1,snd licznik)
                licznik <- (fst licznik,snd licznik + 1)    
            s1.Stop()
            printfn "Verification complete!"
            ((fst licznik), (((float (fst licznik) / float (snd licznik))).ToString("P")))
            ||> printfn "Error blocks found: %d (%s)" 
            if this.time = true then
                printfn "Time: %dms" (s1.ElapsedMilliseconds)
            

                
                
