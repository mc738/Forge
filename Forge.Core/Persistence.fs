namespace Forge.Core.Persistence

open System
open System.Text.Json.Serialization
open Freql.Core.Common
open Freql.MySql

/// Module generated on 18/12/2021 12:12:46 (utc) via Freql.Sqlite.Tools.
module Records =
    type BuildLogItemRecord =
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
) ENGINE=InnoDB AUTO_INCREMENT=53 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
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
    
    type BuildRecord =
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
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
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
    
    type ProjectRecord =
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
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
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
    
module Operations =
    type AddBuildLogItemParameters =
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
    
    let insertBuildLogItem (context: MySqlContext) (parameters: AddBuildLogItemParameters) =
        context.Insert("build_logs", parameters)
    
    type AddBuildParameters =
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
    
    let insertBuild (context: MySqlContext) (parameters: AddBuildParameters) =
        context.Insert("builds", parameters)
    
    type AddProjectParameters =
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
    
    let insertProject (context: MySqlContext) (parameters: AddProjectParameters) =
        context.Insert("projects", parameters)
    