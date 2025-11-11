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
        Arguments = "fantomas ."
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
        Arguments = "fantomas --check ."
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

Task("Coverage")
    .Description("Run tests with code coverage collection and generate HTML report.")
    .IsDependentOn("Build")
    .Does(() =>
{
    // Clean previous coverage results
    if (DirectoryExists("./TestResults"))
    {
        DeleteDirectory("./TestResults", new DeleteDirectorySettings
        {
            Recursive = true,
            Force = true
        });
    }

    // Run tests with coverage collection
    DotNetTest("./OrderTaking.sln", new DotNetTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
        NoRestore = true,
        ArgumentCustomization = args => args
            .Append("--collect:\"XPlat Code Coverage\"")
            .Append("--results-directory:./TestResults")
    });

    // Generate HTML coverage report using ReportGenerator
    var coverageFiles = GetFiles("./TestResults/**/coverage.cobertura.xml");
    if (coverageFiles.Count == 0)
    {
        Warning("No coverage files found. Skipping report generation.");
        return;
    }

    var reportPath = "./TestResults/CoverageReport";
    var exitCode = StartProcess("reportgenerator", new ProcessSettings
    {
        Arguments = new ProcessArgumentBuilder()
            .Append($"-reports:{coverageFiles.First()}")
            .Append($"-targetdir:{reportPath}")
            .Append("-reporttypes:Html;Cobertura")
    });

    if (exitCode != 0)
    {
        throw new Exception($"ReportGenerator failed with exit code {exitCode}");
    }

    // Parse and display coverage summary
    var summaryFile = File(reportPath + "/Summary.json");
    if (FileExists(summaryFile))
    {
        Information("========================================");
        Information("Code Coverage Summary");
        Information("========================================");
        Information($"HTML Report: {System.IO.Path.GetFullPath(reportPath + "/index.html")}");
    }
    else
    {
        Information("Coverage report generated in: " + reportPath);
    }
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
