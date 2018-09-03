namespace Helper

type Update<'a> = 
| Original of 'a
| Replaced of 'a

module Update =
    let get m =
        match m with
        | Original x -> x
        | Replaced x -> x

    let map f a = 
        match a with
        | Original x -> Original (f x)
        | Replaced x -> Replaced (f x)  

    let bind (binder:'a -> Update<'b>) update = update |> get |> binder

    let exists (predicate: 'a -> bool) update = 
        let x = update |> get
        predicate x

    let contains (value:'a when 'a : equality) update = 
        let x = update |> get
        value = x

    let flatten (update:Update<Update<'a>>) =
        match update with
        | Original u -> u
        | Replaced u -> 
            match u with
            | Original x -> Replaced x
            | Replaced x -> Replaced x

    let isOriginal update =
        match update with
        | Original _ -> true
        | Replaced _ -> false

    let isReplaced update =
        match update with
        | Original _ -> false
        | Replaced _ -> true

    let define (check:'a -> 'a -> bool) (value:'a) (existing:'a) =
        if(check value existing) then 
            Replaced existing 
            else Original value

    let join f a b =
        match a,b with
        | Original x, Original y -> Original (f x y)        
        | _ -> Replaced (f (get a) (get b))

    let transitionedToDirty state1 state2 =
        match state1,state2 with
        | Original _, Replaced _ -> true
        | _ -> false

    let collectioni f (xs:seq<'a>) : (int * Update<'a>) seq =
        let mapping (i:int) (x:'a) : (int * Update<'a>) * int =
            let s = i + 1
            let newValue = f x
            let r = if(newValue = x) then (i,(Original x)) else (i,(Replaced newValue))
            r,s

        xs |> Seq.mapFold mapping 0 |> fst

    let hasReplaced xs = xs |> Seq.exists isReplaced     
    let hasOnlyOriginal xs = xs |> hasReplaced |> not     