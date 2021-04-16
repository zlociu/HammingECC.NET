module Hamming

open System
open System.Collections
open System.IO

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

(* KODOWANIE
    1.

    2. w pętli:
    2.1 pobierz bajty az dostepne bedzie 26 bitow
    2.2 oblicz sumy kontrolne
    2.3 zapisz 32 bity jako 4 bajty do pliku

    3. jeśli pozostałe bits.Lenght != 26, dopełnienie '0' do 26

    4. Doklejenie na końcu Bajt informujący ile '0' dopisano żeby je potem zabrać przy dekodowaniu
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
    member this.Encode() =
        let data = File.ReadAllBytes(this.fileName)
        let outputFile = File.OpenWrite(this.fileName + ".ecc")
        let mutable offset = 0;
        let mutable remainBits = BitArray(0)
    
        while offset < data.Length do
            let newData = BitArray(32, false)
            let mutable index = 3
            while offset < data.Length do
                0 |> ignore
        

    member this.Decode() =
        let data = File.ReadAllBytes(this.fileName)  
        let mutable offset = 0;
        let bits = BitArray(data.[offset..(offset+4)])
    
        while offset < data.Length do
        0 |> ignore 

    //może dobre na początek
    member this.Verify() =
        let data = File.ReadAllBytes(this.fileName)
        let mutable offset = 0;
        0 |> ignore
    
