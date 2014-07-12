#r "System.Xml"
#r "System.Xml.Linq"

using Nake.FS;
using Nake.Run;
using Nake.Log;

using System.Linq;
using System.Xml.Linq;
using System.Diagnostics;

const string Project = "Orleans.Bus";
const string RootPath = "$NakeScriptDirectory$";
const string OutputPath = RootPath + @"\Output";

/// Builds sources in Debug mode
[Task] void Default()
{
	Build();
}

/// Wipeout all build output and temporary build files
[Step] void Clean(string path = OutputPath)
{
    Delete(@"{path}\*.*|-:*.vshost.exe");
    RemoveDir(@"**\bin|**\obj|{path}\*|-:*.vshost.exe");
}

/// Builds sources using specified configuration and output path
[Step] void Build(string config = "Debug", string outDir = OutputPath)
{
    Clean(outDir);

    MSBuild("{Project}.sln", 
            "Configuration={config};OutDir={outDir};ReferencePath={outDir}");
}

/// Runs unit tests 
[Step] void Test(string outputPath = OutputPath)
{
    Build("Debug", outputPath);

    FileSet tests = @"{outputPath}\*.Tests.dll";
    Cmd(@"Packages\NUnit.Runners.2.6.3\tools\nunit-console.exe /framework:net-4.0 /noshadow /nologo {tests}");
}

/// Builds official NuGet package 
[Step] void Package()
{
    var packagePath = OutputPath + @"\Package";
    var releasePath = packagePath + @"\Release";

    Test(packagePath + @"\Debug");
    Build("Release", releasePath);

	var version = FileVersionInfo
			.GetVersionInfo(@"{releasePath}\{Project}.dll")
			.FileVersion;

	Cmd(@"Tools\Nuget.exe pack Build\{Project}.nuspec -Version {version} " +
		 "-OutputDirectory {packagePath} -BasePath {RootPath} -NoPackageAnalysis");

	Cmd(@"Tools\Nuget.exe pack Build\{Project}.Reactive.nuspec -Version {version} " +
		 "-OutputDirectory {packagePath} -BasePath {RootPath} -NoPackageAnalysis");
}

/// Installs dependencies (packages) from NuGet 
[Task] void Install()
{
    var packagesDir = @"{RootPath}\Packages";

    var configs = XElement
        .Load(packagesDir + @"\repositories.config")
        .Descendants("repository")
        .Select(x => x.Attribute("path").Value.Replace("..", RootPath)); 

    foreach (var config in configs)
        Cmd(@"Tools\NuGet.exe install {config} -o {packagesDir}");

	// install packages required for building/testing/publishing package
    Cmd(@"Tools\NuGet.exe install Build/Packages.config -o {packagesDir}");
}