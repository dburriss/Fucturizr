namespace Tests

module UpdateTests =
    open Xunit
    open Swensen.Unquote
    open Helper
    
    [<Fact>]
    let ``Replace when matches predicate`` () =
        let lst = [1;2;3;4;5]
        let check value existing = value < existing
        let mappingAdv predicate newValue state listElement =
            let c = Update.define predicate newValue listElement
            let s = (Update.isReplaced c) || state
            let v = Update.get c
            let r = (v, s)
            r

        let mapping = mappingAdv check 3
        let result = lst |> List.mapFold mapping false
        test <@ result = ([3;3;3;4;5],true) @>

    let ``Replace when matches predicate2`` () =
        let lst = [1;2;3;4;5]
        let map = fun x -> if(x > 3) then 3 else x
        let result = lst |> Update.collectioni map |> Seq.toList |> List.map (snd >> Update.get) 
        test <@ result = [3;3;3;4;5] @>