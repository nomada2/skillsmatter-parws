module AsyncWorker

open System.IO
open System
open System.Threading
open System.Collections.Generic
open System.Net
open AgentModule



type AsyncWorker<'T>(jobs: seq<Async<'T>>) =

    // Capture the synchronization context to allow us
    // to raise events back on the GUI thread
    let syncContext = System.Threading.SynchronizationContext.Current

    // Check that we are being called from a GUI thread
    do match syncContext with
        | null -> failwith "Failed to capture the synchronization context of the calling thread. The System.Threading.SynchronizationContext.Current of the calling thread is null"
        | _ -> ()


    let allCompleted  = new Event<unit>()
    let error         = new Event<System.Exception>()
    let canceled      = new Event<System.OperationCanceledException>()
    let jobCompleted  = new Event<int * 'T>()


    let asyncGroup = new CancellationTokenSource()

    let raiseEventOnGuiThread (event:Event<_>) args =
        syncContext.Post(SendOrPostCallback(fun _ -> event.Trigger args),state=null)

    member x.Start()    =

        // Mark up the jobs with numbers
        let jobs = jobs |> Seq.mapi (fun i job -> (job,i+1))

        let work =
            Async.Parallel
               [ for (job,jobNumber) in jobs do
                    yield
                       async { let! result = job
                               raiseEventOnGuiThread jobCompleted (jobNumber,result) } ]
             |> Async.Ignore

        Async.StartWithContinuations
            ( work,
              (fun res -> raiseEventOnGuiThread allCompleted res),
              (fun exn -> raiseEventOnGuiThread error exn),
              (fun exn -> raiseEventOnGuiThread canceled exn ),
              asyncGroup.Token)

    member x.CancelAsync() = asyncGroup.Cancel();

    member x.JobCompleted  = jobCompleted.Publish
    member x.AllCompleted  = allCompleted.Publish
    member x.Canceled   = canceled.Publish
    member x.Error      = error.Publish