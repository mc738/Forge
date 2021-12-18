

open Faaz
open Faaz.ScriptHost
open Forge.Core.Agents
open Freql.MySql

printfn "Starting fsi"
let fsi = ScriptHost.fsiSession ()
printfn "Complete."

let hostContext =  ({ FsiSession = fsi } : HostContext)

let context = MySqlContext.Connect("Server=localhost;Database=forge;Uid=max;Pwd=letmein;")

let buildAgent = BuildAgent(hostContext, context, "C:\\Users\\44748\\Projects\\Forge\\Scripts\\BuildScripts.fsx")

buildAgent.StartRevision("TestRepo")

Async.Sleep 100000 |> Async.RunSynchronously

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"