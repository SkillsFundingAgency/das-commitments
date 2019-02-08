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
                Project = ".\\SFA.DAS.Commitments.Api.Client.TestHarness" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\SFA.DAS.Commitments.Api.Types" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\CommitmentsV2\\SFA.DAS.CommitmentsV2" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\CommitmentsV2\\SFA.DAS.CommitmentsV2.UnitTests" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\CommitmentsV2\\SFA.DAS.CommitmentsV2.Api" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\CommitmentsV2\\SFA.DAS.CommitmentsV2.Api.UnitTests" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\CommitmentsV2\\SFA.DAS.CommitmentsV2.Api.IntegrationTests" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\CommitmentsV2\\SFA.DAS.CommitmentsV2.Api.Client" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\CommitmentsV2\\SFA.DAS.CommitmentsV2.Api.Types" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\CommitmentsV2\\SFA.DAS.CommitmentsV2.Jobs" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\CommitmentsV2\\SFA.DAS.CommitmentsV2.MessageHandlers" })
    DotNetCli.Restore(fun p ->
        { p with
                Project = ".\\CommitmentsV2\\SFA.DAS.CommitmentsV2.Messages" })

)