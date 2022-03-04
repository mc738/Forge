namespace Forge.Core.Actions

[<RequireQualifiedAccess>]
module S3 =

    open Faaz
    open FStore.S3

    let loadContext path = S3Context.Create path

    let upload (ctx: S3Context) bucket key path =
        attempt
            (fun _ ->
                ctx.UploadObject(bucket, key, path)
                |> Async.RunSynchronously)


    let uploadStream (ctx: S3Context) bucket key stream =
        attempt
            (fun _ ->
                ctx.SaveStream(bucket, key, stream)
                |> Async.RunSynchronously)

    let download (ctx: S3Context) bucket key path =
        attempt
            (fun _ ->
                ctx.DownloadObject(bucket, key, path, false)
                |> Async.RunSynchronously)
