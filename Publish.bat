@ECHO OFF
SET DIR=%~dp0%
%DIR%\Packages\Nake.1.0.1.0\tools\net45\Nake.exe -f %DIR%\Publish.csx -d %DIR% -r publish %*