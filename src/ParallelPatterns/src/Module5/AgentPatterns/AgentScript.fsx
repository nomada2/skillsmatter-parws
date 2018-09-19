
#r ".//bin//Debug//netstandard2.0//AgentPatterns.dll"

open AgentPatterns
open System
open System.Collections.Generic


let pipe workersCount map (queue:BlockingQueueAgent<_>) =

    let results = BlockingQueueAgent<_>(workersCount)
    //let results = BlockingQueueAgent<_>(workersCount)

    let rec worker () = async {
        let! input = queue.AsyncGet()
        let! result = map input 
        do! results.AsyncAdd (result)
        return! worker ()
    } 
    worker () |> Async.Start
    
    // fun x -> queue.AsyncAdd x
    results


let buff1 = BlockingQueueAgent<int>(100)

let op i = async {
    do! Async.Sleep i
    return i*i }

let p = pipe 4 (fun (i:int) -> op i) buff1 |> pipe 4 (fun (i:int) ->op i)

buff1.AsyncAdd 9 |> Async.Start
p.AsyncGet() |> Async.RunSynchronously