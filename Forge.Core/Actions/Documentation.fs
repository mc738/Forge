namespace Forge.Core.Actions

open System.IO
open Faaz

module Documentation =

    open FXD
    
    let getVersion (sc: ScriptContext) = sc.GetValue("version", "")
    
    let getSrcPath (sc: ScriptContext) = sc.GetValue("src-path", "")
    
    let getDocumentationPath (sc: ScriptContext) = sc.GetValue("document-dir", "")

    let generate (ctx: ScriptContext) =
        attempt (fun _ ->
            // Create template cache.

            ctx.Log("generate-docs", "Creating template cache.")
            let templateCache =
                FXD
                    .Pipelines
                    .Context
                    .TemplateCache
                    .Empty
                    .LoadAndAdd("article", "C:\\Users\\44748\\Projects\\FXD\\Templates\\article.mustache")
                    .LoadAndAdd("fsharp_code_doc", "C:\\Users\\44748\\Projects\\FXD\\Templates\\fsharp_code_document.mustache")

            let src = getSrcPath ctx
            let cfgPath = Path.Combine(src, "fxd.json")
            ctx.Log("generate-docs", $"Loading configuration from `{cfgPath}`.")
            let cfg = FXD.Pipelines.Configuration.PipelineConfiguration.Load cfgPath

            ctx.Log("generate-docs", "Creating document pipeline.")            
            let docPipeline =
                ({ Name = cfg.Name
                   Templates = templateCache
                   RootPath = src
                   OutputRoot = getDocumentationPath ctx
                   GlobalMetaData =
                       [ "fxd_version", "0.1.0"
                         "fxd_repo_url", "https://github.com/mc738/FXD"
                         "fa_url", "https://kit.fontawesome.com/f5ae0cbcfc.js" ]
                       |> Map.ofList
                   DocumentTitle = $"{cfg.Name} documentation"
                   Version = getVersion ctx
                   Configuration = cfg
                 }: FXD.Pipelines.DocumentPipeline)

            ctx.Log("generate-docs", "Running document pipeline.")                            
            docPipeline.Run())