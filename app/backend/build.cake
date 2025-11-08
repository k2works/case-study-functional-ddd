///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   Information("Running tasks...");
   Information("Configuration: {0}", configuration);
});

Teardown(ctx =>
{
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Description("Cleans all directories that are used during the build process.")
    .Does(() =>
{
    CleanDirectories("./src/**/bin/" + configuration);
    CleanDirectories("./src/**/obj/" + configuration);
    CleanDirectories("./tests/**/bin/" + configuration);
    CleanDirectories("./tests/**/obj/" + configuration);
});

Task("Restore")
    .Description("Restores all the NuGet packages that are used by the specified solution.")
    .Does(() =>
{
    DotNetRestore("./OrderTaking.sln");
});

Task("Build")
    .Description("Builds all the different parts of the project.")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
{
    DotNetBuild("./OrderTaking.sln", new DotNetBuildSettings
    {
        Configuration = configuration,
        NoRestore = true
    });
});

Task("Format")
    .Description("Format F# code using Fantomas.")
    .Does(() =>
{
    var exitCode = StartProcess("dotnet", new ProcessSettings
    {
        Arguments = "fantomas src/ tests/"
    });

    if (exitCode != 0)
    {
        throw new Exception($"Fantomas failed with exit code {exitCode}");
    }
});

Task("FormatCheck")
    .Description("Check F# code formatting using Fantomas.")
    .Does(() =>
{
    var exitCode = StartProcess("dotnet", new ProcessSettings
    {
        Arguments = "fantomas --check src/ tests/"
    });

    if (exitCode != 0)
    {
        throw new Exception($"Fantomas check failed. Run 'dotnet cake --target=Format' to fix formatting.");
    }
});

Task("Lint")
    .Description("Run FSharpLint on the solution.")
    .Does(() =>
{
    var exitCode = StartProcess("dotnet", new ProcessSettings
    {
        Arguments = "dotnet-fsharplint lint OrderTaking.sln"
    });

    if (exitCode != 0)
    {
        Warning("FSharpLint found issues.");
    }
});

Task("Test")
    .Description("Run all tests.")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetTest("./OrderTaking.sln", new DotNetTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
        NoRestore = true
    });
});

Task("Quality")
    .Description("Run all quality checks (format check and lint).")
    .IsDependentOn("FormatCheck")
    .IsDependentOn("Lint");

Task("Default")
    .Description("This is the default task which will be ran if no specific target is passed in.")
    .IsDependentOn("Test");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);
