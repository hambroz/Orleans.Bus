#r "Packages\Nake.1.0.1.0\tools\net45\Meta.dll"
#r "Packages\Nake.1.0.1.0\tools\net45\Utility.dll"

#r "System.Xml"
#r "System.Xml.Linq"
#r "System.IO.Compression"
#r "System.IO.Compression.FileSystem"

#r "Packages\EasyHttp.1.6.58.0\lib\net40\EasyHttp.dll"
#r "Packages\JsonFx.2.0.1209.2802\lib\net40\JsonFx.dll"

using Nake;

using System;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Dynamic;
using System.Collections.Generic;

using EasyHttp.Http;
using EasyHttp.Infrastructure;

const string Project = "Orleans.Bus";

static string OutputPath = @"$NakeScriptDirectory$\Output";
static string PackagePath = @"{OutputPath}\Package";

static string DebugOutputPath = @"{PackagePath}\Debug";
static string ReleaseOutputPath = @"{PackagePath}\Release";

static Func<string> PackageFile = () => PackagePath + @"\{Project}.{Version()}.nupkg";
static Func<string> ArchiveFile = () => OutputPath + @"\{Version()}.zip";

/// <summary> 
/// Zips all binaries for standalone installation
/// </summary>
[Task] public static void Zip()
{
	var files = new FileSet
	{
		@"{ReleaseOutputPath}\{Project}.*",
		"-:*.Tests.*"
	};

	FS.Delete(ArchiveFile());

	using (ZipArchive archive = ZipFile.Open(ArchiveFile(), ZipArchiveMode.Create))
	{
		foreach (var file in files)
			archive.CreateEntryFromFile(file, Path.GetFileName(file));
	}
}

/// <summary>
/// Publishes package to NuGet gallery
/// </summary>
[Task] public static void NuGet()
{
	Cmd.Exec(@"Tools\Nuget.exe push {PackageFile()} $NuGetApiKey$");
}


/// <summary> 
/// Publishes standalone version to GitHub releases
/// </summary>
[Task] public static void Standalone(string description = null, bool beta = false)
{
	Zip();

	string release = CreateRelease(description, beta);
	Upload(release, ArchiveFile(), "application/zip");
}

static string CreateRelease(string description, bool beta)
{
	IDictionary<string, object> data = new ExpandoObject();

	data["tag_name"] = data["name"] = Version();
	data["target_commitish"] = "master";
	data["prerelease"] = beta;
	data["body"] = "Standalone release {Version()}";

	if (!string.IsNullOrEmpty(description))
		data["body"] = description;

	return GitHub().Post("https://api.github.com/repos/yevhen/{Project}/releases",
						  data, HttpContentTypes.ApplicationJson).Location;
}

static void Upload(string release, string filePath, string contentType)
{
	GitHub().Post(GetUploadUri(release) + "?name=" + Path.GetFileName(filePath), null, new List<FileData>
	{
		new FileData()
		{
			ContentType = contentType,
			Filename = filePath
		}
	});
}

static string GetUploadUri(string release)
{
	var body = (IDictionary<string, object>)GitHub().Get(release).DynamicBody;
	return ((string)body["upload_url"]).Replace("{{?name}}", "");
}

static HttpClient GitHub()
{
	var client = new HttpClient();

	client.Request.Accept = "application/vnd.github.manifold-preview";
	client.Request.ContentType = "application/json";
	client.Request.AddExtraHeader("Authorization", "token $GitHubToken$");

	return client;
}

static string Version()
{
	return FileVersionInfo
			.GetVersionInfo(@"{ReleaseOutputPath}\{Project}.dll")
			.FileVersion;
}