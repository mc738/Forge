namespace Forge.Core.Persistence

open System
open System.Text.Json.Serialization
open Freql.Core.Common
open Freql.MySql

/// Module generated on 04/03/2022 20:02:30 (utc) via Freql.Sqlite.Tools.
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
) ENGINE=InnoDB AUTO_INCREMENT=373 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
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
) ENGINE=InnoDB AUTO_INCREMENT=29 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci
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
    
    /// A record representing a row in the table `deployment_location`.
    type DeploymentLocation =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Id = 0
              Name = String.Empty }
    
        static member CreateTableSql() = """
        CREATE TABLE `deployment_location` (
  `id` int NOT NULL AUTO_INCREMENT,
  `name` text NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              name
        FROM deployment_location
        """
    
        static member TableName() = "deployment_location"
    
    /// A record representing a row in the table `deployments`.
    type Deployments =
        { [<JsonPropertyName("id")>] Id: int
          [<JsonPropertyName("buildId")>] BuildId: int
          [<JsonPropertyName("locationId")>] LocationId: int
          [<JsonPropertyName("artifactBucket")>] ArtifactBucket: string
          [<JsonPropertyName("artifactKey")>] ArtifactKey: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("completedOn")>] CompletedOn: DateTime option
          [<JsonPropertyName("complete")>] Complete: byte
          [<JsonPropertyName("hadErrors")>] HadErrors: byte option
          [<JsonPropertyName("hadWarnings")>] HadWarnings: byte option }
    
        static member Blank() =
            { Id = 0
              BuildId = 0
              LocationId = 0
              ArtifactBucket = String.Empty
              ArtifactKey = String.Empty
              CreatedOn = DateTime.UtcNow
              CompletedOn = None
              Complete = 0uy
              HadErrors = None
              HadWarnings = None }
    
        static member CreateTableSql() = """
        CREATE TABLE `deployments` (
  `id` int NOT NULL AUTO_INCREMENT,
  `build_id` int NOT NULL,
  `location_id` int NOT NULL,
  `artifact_bucket` text NOT NULL,
  `artifact_key` text NOT NULL,
  `created_on` datetime NOT NULL,
  `completed_on` datetime DEFAULT NULL,
  `complete` tinyint(1) NOT NULL,
  `had_errors` tinyint(1) DEFAULT NULL,
  `had_warnings` tinyint(1) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `deployments_FK` (`build_id`),
  KEY `deployments_FK_1` (`location_id`),
  CONSTRAINT `deployments_FK` FOREIGN KEY (`build_id`) REFERENCES `builds` (`id`),
  CONSTRAINT `deployments_FK_1` FOREIGN KEY (`location_id`) REFERENCES `deployment_location` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3
        """
    
        static member SelectSql() = """
        SELECT
              id,
              build_id,
              location_id,
              artifact_bucket,
              artifact_key,
              created_on,
              completed_on,
              complete,
              had_errors,
              had_warnings
        FROM deployments
        """
    
        static member TableName() = "deployments"
    
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
    

/// Module generated on 04/03/2022 20:02:30 (utc) via Freql.Tools.
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
    
    
    /// A record representing a new row in the table `deployment_location`.
    type NewDeploymentLocation =
        { [<JsonPropertyName("name")>] Name: string }
    
        static member Blank() =
            { Name = String.Empty }
    
    
    /// A record representing a new row in the table `deployments`.
    type NewDeployments =
        { [<JsonPropertyName("buildId")>] BuildId: int
          [<JsonPropertyName("locationId")>] LocationId: int
          [<JsonPropertyName("artifactBucket")>] ArtifactBucket: string
          [<JsonPropertyName("artifactKey")>] ArtifactKey: string
          [<JsonPropertyName("createdOn")>] CreatedOn: DateTime
          [<JsonPropertyName("completedOn")>] CompletedOn: DateTime option
          [<JsonPropertyName("complete")>] Complete: byte
          [<JsonPropertyName("hadErrors")>] HadErrors: byte option
          [<JsonPropertyName("hadWarnings")>] HadWarnings: byte option }
    
        static member Blank() =
            { BuildId = 0
              LocationId = 0
              ArtifactBucket = String.Empty
              ArtifactKey = String.Empty
              CreatedOn = DateTime.UtcNow
              CompletedOn = None
              Complete = 0uy
              HadErrors = None
              HadWarnings = None }
    
    
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
    
    
/// Module generated on 04/03/2022 20:02:30 (utc) via Freql.Tools.
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
    
    /// Select a `Records.DeploymentLocation` from the table `deployment_location`.
    /// Internally this calls `context.SelectSingleAnon<Records.DeploymentLocation>` and uses Records.DeploymentLocation.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectDeploymentLocationRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectDeploymentLocationRecord (context: MySqlContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.DeploymentLocation.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.DeploymentLocation>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.DeploymentLocation>` and uses Records.DeploymentLocation.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectDeploymentLocationRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectDeploymentLocationRecords (context: MySqlContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.DeploymentLocation.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.DeploymentLocation>(sql, parameters)
    
    let insertDeploymentLocation (context: MySqlContext) (parameters: Parameters.NewDeploymentLocation) =
        context.Insert("deployment_location", parameters)
    
    /// Select a `Records.Deployments` from the table `deployments`.
    /// Internally this calls `context.SelectSingleAnon<Records.Deployments>` and uses Records.Deployments.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectDeploymentsRecord ctx "WHERE `field` = @0" [ box `value` ]
    let selectDeploymentsRecord (context: MySqlContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Deployments.SelectSql() ] @ query |> buildSql
        context.SelectSingleAnon<Records.Deployments>(sql, parameters)
    
    /// Internally this calls `context.SelectAnon<Records.Deployments>` and uses Records.Deployments.SelectSql().
    /// The caller can provide extra string lines to create a query and boxed parameters.
    /// It is up to the caller to verify the sql and parameters are correct,
    /// this should be considered an internal function (not exposed in public APIs).
    /// Parameters are assigned names based on their order in 0 indexed array. For example: @0,@1,@2...
    /// Example: selectDeploymentsRecords ctx "WHERE `field` = @0" [ box `value` ]
    let selectDeploymentsRecords (context: MySqlContext) (query: string list) (parameters: obj list) =
        let sql = [ Records.Deployments.SelectSql() ] @ query |> buildSql
        context.SelectAnon<Records.Deployments>(sql, parameters)
    
    let insertDeployments (context: MySqlContext) (parameters: Parameters.NewDeployments) =
        context.Insert("deployments", parameters)
    
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
    