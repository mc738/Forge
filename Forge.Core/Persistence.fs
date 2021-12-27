namespace Forge.Core.Persistence

open System
open System.Text.Json.Serialization
open Freql.Core.Common
open Freql.MySql

/// Module generated on 19/12/2021 21:33:51 (utc) via Freql.Sqlite.Tools.
[<RequireQualifiedAccess>]
module Records =
    /// A record representing a row in the table `build_logs`.
    type BuildLogItem =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("buildId")>] BuildId: int
          [<JsonPropertyName("step")>] Step: string
          [<JsonPropertyName("entry")>] Entry: string
          [<JsonPropertyName("isError")>] IsError: bool
          [<JsonPropertyName("isWarning")>] IsWarning: bool }
    
        static member Blank() =
            { Id = 0
              BuildId = 0
              Step = String.Empty
              Entry = String.Empty
              IsError = false
              IsWarning = false }
    
        static member CreateTableSql() = """
        CREATE TABLE `build_logs` (
  `id` int NOT NULL AUTO_INCREMENT,
  `build_id` int NOT NULL,
  `step` varchar(100) NOT NULL,
  `entry` varchar(1000) NOT NULL,
  `is_error` tinyint(1) NOT NULL,
  `is_warning` tinyint(1) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `build_logs_FK` (`build_id`),
  CONSTRAINT `build_logs_FK` FOREIGN KEY (`build_id`) REFERENCES `builds` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=327 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
        """
    
        static member SelectSql() = """
        SELECT
              id,
              build_id,
              step,
              entry,
              is_error,
              is_warning
        FROM build_logs
        """
    
        static member TableName() = "build_logs"
    
    /// A record representing a row in the table `builds`.
    type Build =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("projectId")>] ProjectId: int
          [<JsonPropertyName("reference")>] Reference: Guid
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("commitHash")>] CommitHash: string
          [<JsonPropertyName("buildTime")>] BuildTime: DateTime
          [<JsonPropertyName("major")>] Major: int
          [<JsonPropertyName("minor")>] Minor: int
          [<JsonPropertyName("revision")>] Revision: int
          [<JsonPropertyName("suffix")>] Suffix: string option
          [<JsonPropertyName("builtBy")>] BuiltBy: string
          [<JsonPropertyName("signature")>] Signature: string
          [<JsonPropertyName("successful")>] Successful: bool }
    
        static member Blank() =
            { Id = 0
              ProjectId = 0
              Reference = Guid.NewGuid()
              Name = String.Empty
              CommitHash = String.Empty
              BuildTime = DateTime.UtcNow
              Major = 0
              Minor = 0
              Revision = 0
              Suffix = None
              BuiltBy = String.Empty
              Signature = String.Empty
              Successful = false }
    
        static member CreateTableSql() = """
        CREATE TABLE `builds` (
  `id` int NOT NULL AUTO_INCREMENT,
  `project_id` int NOT NULL,
  `reference` varchar(36) NOT NULL,
  `name` varchar(255) NOT NULL,
  `commit_hash` varchar(50) NOT NULL,
  `build_time` datetime NOT NULL,
  `major` int NOT NULL,
  `minor` int NOT NULL,
  `revision` int NOT NULL,
  `suffix` varchar(100) DEFAULT NULL,
  `built_by` varchar(100) NOT NULL,
  `signature` varchar(100) NOT NULL,
  `successful` tinyint(1) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `builds_FK` (`project_id`),
  CONSTRAINT `builds_FK` FOREIGN KEY (`project_id`) REFERENCES `projects` (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=26 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
        """
    
        static member SelectSql() = """
        SELECT
              id,
              project_id,
              reference,
              name,
              commit_hash,
              build_time,
              major,
              minor,
              revision,
              suffix,
              built_by,
              signature,
              successful
        FROM builds
        """
    
        static member TableName() = "builds"
    
    /// A record representing a row in the table `projects`.
    type Project =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("reference")>] Reference: Guid
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("nameSlug")>] NameSlug: string
          [<JsonPropertyName("url")>] Url: string
          [<JsonPropertyName("sourceUrl")>] SourceUrl: string
          [<JsonPropertyName("scriptName")>] ScriptName: string }
    
        static member Blank() =
            { Id = 0
              Reference = Guid.NewGuid()
              Name = String.Empty
              NameSlug = String.Empty
              Url = String.Empty
              SourceUrl = String.Empty
              ScriptName = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE `projects` (
  `id` int NOT NULL AUTO_INCREMENT,
  `reference` varchar(36) NOT NULL,
  `name` varchar(100) NOT NULL,
  `name_slug` varchar(100) NOT NULL,
  `url` varchar(255) NOT NULL,
  `source_url` varchar(255) NOT NULL,
  `script_name` varchar(100) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `projects_UN` (`reference`),
  UNIQUE KEY `projects_UN_1` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
        """
    
        static member SelectSql() = """
        SELECT
              id,
              reference,
              name,
              name_slug,
              url,
              source_url,
              script_name
        FROM projects
        """
    
        static member TableName() = "projects"
    

/// Module generated on 19/12/2021 21:33:51 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Parameters =
    /// A record representing a new row in the table `build_logs`.
    type NewBuildLogItem =
        { [<JsonPropertyName("buildId")>] BuildId: int
          [<JsonPropertyName("step")>] Step: string
          [<JsonPropertyName("entry")>] Entry: string
          [<JsonPropertyName("isError")>] IsError: bool
          [<JsonPropertyName("isWarning")>] IsWarning: bool }
    
        static member Blank() =
            { BuildId = 0
              Step = String.Empty
              Entry = String.Empty
              IsError = false
              IsWarning = false }
    
    
    /// A record representing a new row in the table `builds`.
    type NewBuild =
        { [<JsonPropertyName("projectId")>] ProjectId: int
          [<JsonPropertyName("reference")>] Reference: Guid
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("commitHash")>] CommitHash: string
          [<JsonPropertyName("buildTime")>] BuildTime: DateTime
          [<JsonPropertyName("major")>] Major: int
          [<JsonPropertyName("minor")>] Minor: int
          [<JsonPropertyName("revision")>] Revision: int
          [<JsonPropertyName("suffix")>] Suffix: string option
          [<JsonPropertyName("builtBy")>] BuiltBy: string
          [<JsonPropertyName("signature")>] Signature: string
          [<JsonPropertyName("successful")>] Successful: bool }
    
        static member Blank() =
            { ProjectId = 0
              Reference = Guid.NewGuid()
              Name = String.Empty
              CommitHash = String.Empty
              BuildTime = DateTime.UtcNow
              Major = 0
              Minor = 0
              Revision = 0
              Suffix = None
              BuiltBy = String.Empty
              Signature = String.Empty
              Successful = false }
    
    
    /// A record representing a new row in the table `projects`.
    type NewProject =
        { [<JsonPropertyName("reference")>] Reference: Guid
          [<JsonPropertyName("name")>] Name: string
          [<JsonPropertyName("nameSlug")>] NameSlug: string
          [<JsonPropertyName("url")>] Url: string
          [<JsonPropertyName("sourceUrl")>] SourceUrl: string
          [<JsonPropertyName("scriptName")>] ScriptName: string }
    
        static member Blank() =
            { Reference = Guid.NewGuid()
              Name = String.Empty
              NameSlug = String.Empty
              Url = String.Empty
              SourceUrl = String.Empty
              ScriptName = String.Empty }
    
    
/// Module generated on 19/12/2021 21:33:51 (utc) via Freql.Tools.
[<RequireQualifiedAccess>]
module Operations =

    let buildSql (lines: string list) = lines |> String.concat Environment.NewLine

    /// Select a `Records.BuildLogItem` from the table `build_logs`.
    /// Internally this calls `context.SelectSingleAnon<Records.BuildLogItem>` and uses Records.BuildLogItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectBuildLogItemRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectBuildLogItemRecord (context: MySqlContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.BuildLogItem.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.BuildLogItem>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.BuildLogItem>` and uses Records.BuildLogItem.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectBuildLogItemRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectBuildLogItemRecords (context: MySqlContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.BuildLogItem.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.BuildLogItem>(sql, parameters)
    
    let insertBuildLogItem (context: MySqlContext) (parameters: Parameters.NewBuildLogItem) =
        context.Insert("build_logs", parameters)
    
    /// Select a `Records.Build` from the table `builds`.
    /// Internally this calls `context.SelectSingleAnon<Records.Build>` and uses Records.Build.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectBuildRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectBuildRecord (context: MySqlContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Build.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.Build>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.Build>` and uses Records.Build.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectBuildRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectBuildRecords (context: MySqlContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Build.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.Build>(sql, parameters)
    
    let insertBuild (context: MySqlContext) (parameters: Parameters.NewBuild) =
        context.Insert("builds", parameters)
    
    /// Select a `Records.Project` from the table `projects`.
    /// Internally this calls `context.SelectSingleAnon<Records.Project>` and uses Records.Project.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectProjectRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectProjectRecord (context: MySqlContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Project.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.Project>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.Project>` and uses Records.Project.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectProjectRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectProjectRecords (context: MySqlContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Project.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.Project>(sql, parameters)
    
    let insertProject (context: MySqlContext) (parameters: Parameters.NewProject) =
        context.Insert("projects", parameters)
    