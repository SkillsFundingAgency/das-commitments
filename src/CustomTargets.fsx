open Fake

let testDirectory = getBuildParamOrDefault "buildMode" "Debug"
let nUnitRunner = "nunit3-console.exe"
let mutable nUnitToolPath = @"tools\NUnit.ConsoleRunner\"
let acceptanceTestPlayList = getBuildParamOrDefault "playList" ""
let nunitTestFormat = getBuildParamOrDefault "nunitTestFormat" "nunit2"

Target "Dotnet Restore" (fun _ ->
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\SFA.DAS.Commitments.Api.Client" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\SFA.DAS.Commitments.Api.Types" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\Approvals\\SFA.DAS.ProviderCommitments" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\Approvals\\SFA.DAS.ProviderCommitments.UnitTests" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\Approvals\\SFA.DAS.ProviderCommitments.Api" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\Approvals\\SFA.DAS.ProviderCommitments.Host" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\Approvals\\SFA.DAS.ProviderCommitments.Jobs" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\Approvals\\SFA.DAS.ProviderCommitments.Messages" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\Approvals\\SFA.DAS.ProviderCommitments.MessageHandlers" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\Approvals\\SFA.DAS.ProviderCommitments.MessageHandlers.UnitTests" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\Approvals\\SFA.DAS.ProviderCommitments.Web" })
)