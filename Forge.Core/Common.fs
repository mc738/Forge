namespace Forge.Core

open System


[<AutoOpen>]
module Common =
    
    
    type Version =
        { Major: int
          Minor: int
          Revision: int
          Suffix: string option }

    type BuildStats =
        { Reference: Guid
          Name: string
          Major: int
          Minor: int
          Revision: int
          VersionSuffix: string option
          BuildTime: DateTime
          BuiltBy: string
          LatestCommitHash: string
          Signature: string }

        static member TableSql() =
            """CREATE TABLE build_stats (
                    reference TEXT NOT NULL,
	                name TEXT NOT NULL,
                    major INTEGER NOT NULL,
                    minor INTEGER NOT NULL,
                    revision INTEGER NOT NULL,
                    version_suffix TEXT,
                    build_time TEXT NOT NULL,
                    built_by TEXT NOT NULL,
                    latest_commit_hash TEXT NOT NULL,
                    signature TEXT NOT NULL
                   );"""


        member bs.GetVersion() = $"{bs.Major}.{bs.Minor}.{bs.Revision}"

        member bs.GetVersionWithSuffix() =
            match bs.VersionSuffix with
            | Some vs -> $"{bs.Major}.{bs.Minor}.{bs.Revision}-{vs}"
            | None -> bs.GetVersion()

        member bs.GetLastCommitSlug() = bs.LatestCommitHash.[0..6]

        member bs.GetFullName() =
            let vs =
                match bs.VersionSuffix with
                | Some v -> $"${v}"
                | None -> ""

            $"{bs.Name}-{bs.GetVersion()}{vs}+{bs.GetLastCommitSlug()}.win-x86"
    
    ()

