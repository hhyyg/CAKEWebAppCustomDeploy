#tool "KuduSync.NET" "https://www.nuget.org/api/v2/"
#addin "Cake.Kudu" "https://www.nuget.org/api/v2/"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target          = Argument<string>("target", "Default");
var configuration   = Argument<string>("configuration", "vsbuildrelease");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var websitePath     = MakeAbsolute(Directory("./MisoDep"));
var solutionPath    = MakeAbsolute(File("./MisoDep.sln"));

if (!Kudu.IsRunningOnKudu)
{
    throw new Exception("Not running on Kudu");
}

var deploymentPath = Kudu.Deployment.Target;
if (!DirectoryExists(deploymentPath))
{
    throw new DirectoryNotFoundException(
        string.Format(
            "Deployment target directory not found {0}",
            deploymentPath
            )
        );
}

///////////////////////////////////////////////////////////////////////////////
// TASK DEFINITIONS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    //Clean up any binaries
    Information("Cleaning {0}", websitePath);
    CleanDirectories(websitePath + "/bin");
});

Task("Restore")
    .Does(() =>
{
    // Restore all NuGet packages.
    Information("Restoring {0}...", solutionPath);
    NuGetRestore(solutionPath);
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
{
    // Build all solutions.
    Information("Building {0}", solutionPath);
    MSBuild(solutionPath, settings =>
        settings.SetPlatformTarget(PlatformTarget.MSIL)
            .WithProperty("TreatWarningsAsErrors","true")
            .WithTarget("Build")
            .SetConfiguration(configuration));
});

Task("Publish")
    .IsDependentOn("Build")
    .Does(() =>
{
    Information("Deploying web from {0} to {1}", websitePath, deploymentPath);
    Kudu.Sync(websitePath);
});

Task("Default")
    .IsDependentOn("Publish");

///////////////////////////////////////////////////////////////////////////////
// EXECUTION
///////////////////////////////////////////////////////////////////////////////

RunTarget(target);