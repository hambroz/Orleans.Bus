#r "System.Xml"
#r "System.Xml.Linq"
#r "System.IO.Compression"
#r "System.IO.Compression.FileSystem"

#r "Packages\EasyHttp.1.6.58.0\lib\net40\EasyHttp.dll"
#r "Packages\JsonFx.2.0.1209.2802\lib\net40\JsonFx.dll"

using Nake;
using Nake.FS;
using Nake.Run;

using System.IO.Compression;
using System.Diagnostics;
using System.Dynamic;

using EasyHttp.Http;
using EasyHttp.Infrastructure;

const string Project = "Orleans.Bus";

var OutputPath = @"$NakeScriptDirectory$\Output";
var PackagePath = @"{OutputPath}\Package";

var DebugOutputPath = @"{PackagePath}\Debug";
var ReleaseOutputPath = @"{PackagePath}\Release";

Func<string> PackageFile = () => PackagePath + @"\{Project}.{Version()}.nupkg";
Func<string> ReactivePackageFile = () => PackagePath + @"\{Project}.Reactive.{Version()}.nupkg";
Func<string> ArchiveFile = () => OutputPath + @"\{Version()}.zip";

/// Zips all binaries for standalone installation
[Step] void Zip()
{
    var files = new FileSet("{ReleaseOutputPath}")
    {
        @"{Project}.*",
        "-:*.Tests.*"
    };

    Delete(ArchiveFile());

    using (ZipArchive archive = ZipFile.Open(ArchiveFile(), ZipArchiveMode.Create))
    {
        foreach (var file in files)
            archive.CreateEntryFromFile(file, Path.GetFileName(file));
    }
}

/// Publishes package to NuGet gallery
[Step] void NuGet()
{
    Cmd(@"Tools\Nuget.exe push {PackageFile()} $NuGetApiKey$");
    Cmd(@"Tools\Nuget.exe push {ReactivePackageFile()} $NuGetApiKey$");
}

/// Publishes standalone version to GitHub releases
[Step] void Standalone(bool beta, string description = null)
{
    Zip();

    string release = CreateRelease(beta, description);
    Upload(release, ArchiveFile(), "application/zip");
}

string CreateRelease(bool beta, string description)
{
    dynamic data = new ExpandoObject();

    data.tag_name = data.name = Version();
    data.target_commitish = beta ? "dev" : "master";
    data.prerelease = beta;
    data.body = !string.IsNullOrEmpty(description) 
                ? description 
                : "Standalone release {Version()}";

    return GitHub().Post("https://api.github.com/repos/yevhen/{Project}/releases",
            data, HttpContentTypes.ApplicationJson).Location;
}

void Upload(string release, string filePath, string contentType)
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

string GetUploadUri(string release)
{
    var body = GitHub().Get(release).DynamicBody;
    return ((string)body.upload_url).Replace("{{?name}}", "");
}

HttpClient GitHub()
{
    var client = new HttpClient();

    client.Request.Accept = "application/vnd.github.manifold-preview";
    client.Request.ContentType = "application/json";
    client.Request.AddExtraHeader("Authorization", "token $GitHubToken$");

    return client;
}

string Version()
{
    return FileVersionInfo
           	.GetVersionInfo(@"{ReleaseOutputPath}\{Project}.dll")
           	.ProductVersion;
}