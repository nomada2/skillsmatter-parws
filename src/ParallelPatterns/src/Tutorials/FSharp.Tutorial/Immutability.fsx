module Immutability

let value = 1
printfn "%d" value
value = 2
printfn "%d" value
printfn "%b" (value = 2)

let mutable value2 = 1
printfn "%d" value2
value2 <- 2
printfn "%d" value2


let x = 1

let greeting = "Hello"

let greetingFor name =
    greeting + ", " + name

// Error:
greeting <- "Goodbye"




/// Part 2 - records, immutability for datatypes, copy & update for sharing

type Person =
    { Name : string;
      HomeTown: string }

let bob = {Name = "Bob"; HomeTown = "Seattle"}

//Error:
bob.HomeTown <- "L.A"

//Okay:
let bobJr = {bob with HomeTown = "L.A"}


/// Part 3 - collections, mutability and control flow, recursion, pipelining for functional compoisiotnal programming, simple parallelism with PSeq

#load "PSeq.fsx"
let nums = [|1..100|]

let sqr x = x * x

let sumOfSquaresI nums =
    let mutable acc = 0
    for n in nums do
        acc <- acc + sqr n
    acc

let rec sumOfSquaresF nums =
    match nums with
    | [] -> 0
    | n :: rest -> (sqr n) + (sumOfSquaresF rest)

let rec sumOfSquares nums =
    nums
    |> PSeq.map sqr
    |> PSeq.sum