@ECHO OFF
SET DIR=%~dp0%
%DIR%\Packages\Nake.1.0.1.0\tools\net45\Nake.exe -f %DIR%\Nake.csx -d %DIR% %*