#r "Packages\Nake.1.0.1.0\tools\net45\Meta.dll"
#r "Packages\Nake.1.0.1.0\tools\net45\Utility.dll"
#r "System.Xml"
#r "System.Xml.Linq"

using Nake;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Diagnostics;

const string Project = "Orleans.Bus";

public const string RootPath = "$NakeScriptDirectory$";
public const string OutputPath = RootPath + @"\Output";

[Task] public static void Default()
{
	Build();
}

/// <summary> 
/// Wipeout all build output and temporary build files
/// </summary>
[Task] public static void Clean(string path = OutputPath)
{
	FS.RemoveDir(@"**\bin|**\obj");

	FS.Delete(@"{path}\*.*|-:*.vshost.exe");
	FS.RemoveDir(@"{path}\*");
}

/// <summary> 
/// Builds Orleans.IoC sources  
/// </summary>
[Task] public static void Build(string configuration = "Debug", string outputPath = OutputPath)
{
	Clean(outputPath);

	MSBuild
		
		.Projects("{Project}.sln")
			.Property("Platform", "Any CPU")
			.Property("Configuration", configuration)
			.Property("OutDir", outputPath)
			.Property("ReferencePath", outputPath)

	.Build();
}

/// <summary> 
/// Runs unit tests
/// </summary>
[Task] public static void Test(string configuration = "Debug", string outputPath = OutputPath)
{
	Build(configuration, outputPath);

	string tests = new FileSet
	{
		@"{outputPath}\*.Tests.dll"
	};

	Cmd.Exec(@"Packages\NUnit.Runners.2.6.3\tools\nunit-console.exe /framework:net-4.5 /noshadow /nologo {tests}");
}

/// <summary> 
/// Builds official NuGet package for Orleans.IoC
/// </summary>
[Task] public static void Package()
{
	var packagePath = OutputPath + @"\Package";
	var releasePath = packagePath + @"\Release";

	Test("Debug", packagePath + @"\Debug");
	Build("Release", releasePath);

	var version = FileVersionInfo
			.GetVersionInfo(@"{releasePath}\{Project}.dll")
			.FileVersion;

	Cmd.Exec(@"Tools\Nuget.exe pack Build\{Project}.nuspec -Version {version} " +
			  "-OutputDirectory {packagePath} -BasePath {RootPath} -NoPackageAnalysis");

	Cmd.Exec(@"Tools\Nuget.exe pack Build\{Project}.Reactive.nuspec -Version {version} " +
			  "-OutputDirectory {packagePath} -BasePath {RootPath} -NoPackageAnalysis");
}

/// <summary> 
/// Installs Orleans.IoC dependencies (packages) from NuGet
/// </summary>
[Task] public static void Install()
{
	var packagesDir = @"{RootPath}\Packages";

	var configs = XElement
		.Load(packagesDir + @"\repositories.config")
		.Descendants("repository")
		.Select(x => x.Attribute("path").Value.Replace("..", RootPath)); 

	foreach (var config in configs)
	{
		Cmd.Exec(@"Tools\NuGet.exe install {config} -o {packagesDir}");
	}

	// install packages required for building/testing/publishing package
	Cmd.Exec(@"Tools\NuGet.exe install Build/Packages.config -o {packagesDir}");
}