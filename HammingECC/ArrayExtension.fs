module ArrayExtension


type ArrayExtension() = 

    static member filteri (fn: int -> bool) (arr: array<'a>)  = 
        let res = Array.zeroCreate<'a> arr.Length
        let mutable dstIdx = 0
        for i = 0 to arr.Length - 1 do
            if (fn i) then
               res.[dstIdx] <- arr.[i]
               dstIdx <- dstIdx + 1
        res


