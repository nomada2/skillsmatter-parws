namespace AgentPatterns

module ParallelWorker =

    // Parallel agent-based 
    // Agents (MailboxProcessor) provide building-block for other 
    // primitives - like parallelWorker

    // TODO 
    // implement an Agent coordinator that send
    // a message to process to a collection sub-agents (children)
    // in a round-robin fashion

    let parallelCoordinator n f =
        MailboxProcessor.Start(fun inbox ->
            let workers = Array.init n (fun i -> MailboxProcessor.Start(f))
            // missing code
            // create a recursive/async lopp with an
            // internal state to track the child-agent index
            // where a message was swend last
            async.Return(())
            )

    // TODO : 5.18
    // A reusable parallel worker model built on F# agents
    // implement a parallel worker based on MailboxProcessor, which coordinates the work in a Round-Robin fashion
    // between a set of children MailboxProcessor(s)
    // use an Array initializer to create the collection of MailboxProcessor(s)
    // the internal state should keep track of the index of the child to the send  the next message
    let parallelWorker (f:_ -> Async<unit>) =
        // TODO : use the "parallelCoordinator" for the implementaion
        Unchecked.defaultof<MailboxProcessor<_>>
       

module ParallelAgentPipeline =
    open ParallelWorker
    open SixLabors.ImageSharp
    open SixLabors.ImageSharp.PixelFormats
    open AgentPatterns
    open System.Threading
    open ImageHandlers
    open System.IO
    open Helpers

    let images = Directory.GetFiles("../../../../../Data/paintings")
    
    let imageProcessPipeline (destination:string) imageSrc = async {
        let imageDestination = Path.Combine(destination, Path.GetFileName(imageSrc))
        load imageSrc
        |> resize 400 400
        |> convert3D
        |> setFilter ImageFilters.Green
        |> saveImage destination }
        
    // TODO :
    //      ParallelAgentPipeline
    //      implement a reusable parallel worker model built on F# agents 
    //      complete the TODOs
    //      
    let start () = 
        let agentImage =
            parallelWorker (imageProcessPipeline "../../../../../Data/Images")
        images
        |> Seq.iter(fun image -> agentImage.Post image)

