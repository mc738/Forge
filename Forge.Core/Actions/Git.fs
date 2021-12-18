namespace Forge.Core.Actions

open System
open Faaz

[<RequireQualifiedAccess>]
module Git =

    let getLastCommitHash (gitPath: string) (path: string) =
        match Process.run
                  { Name = gitPath
                    Args = "rev-parse HEAD"
                    StartDirectory = (Some path) } with
        | Ok r when r.Length > 0 -> Ok r.Head
        | Ok r -> Ok "Not commit hash found."
        | Error e -> Error e
        
    let clone (gitPath: string) (sourceUrl: string) (path: string) =
            let output, errors =
                Process.execute gitPath $"clone {sourceUrl}" (path |> Some)
                
            match errors.Length = 0 with
            | true -> Ok output
            | false ->
                match errors.[0].StartsWith("Cloning into") with
                | true -> Ok [ "Cloned" ]
                | false -> Error errors
    
    let addTag (gitPath: string) (path: string) (tag:string) =
        let output, errors =
            Process.execute gitPath $"tag {tag}" (path |> Some)
            
        match errors.Length = 0 with
        | true -> Ok output
        | false -> Error "Tag not added"

    let pushTag (gitPath: string) (path: string) (tag:string) =
        let output, errors =
            Process.execute gitPath $"push origin {tag}" (path |> Some)
        // For whatever reason the results are returned in STDERR...    
        
        match errors.Length > 1 with
        | true ->
            match errors.[1].StartsWith(" * [new tag]") with
            | true -> errors |> String.concat Environment.NewLine |> Ok
            | false -> Error "Tag not added"
        | false -> Error "Tag not added"
