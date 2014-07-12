@ECHO OFF
SET DIR=%~dp0%
%DIR%\Packages\Nake.2.0.13\tools\net45\Nake.exe -f %DIR%\Nake.csx -d %DIR% %*