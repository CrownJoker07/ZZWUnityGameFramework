set WORKSPACE=..
set LUBAN_DLL=%WORKSPACE%\Tools\Luban\Luban.dll
set CONF_ROOT=.\Config
set outputDataDir=%WORKSPACE%\Project\Assets\GameAssets\DataTables
set outputCodeDir=%WORKSPACE%\Project\Assets\Scripts\DataTables

dotnet %LUBAN_DLL% ^
    -t all ^
    -d json ^
    -c cs-simple-json ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputDataDir=%outputDataDir% ^
    -x outputCodeDir=%outputCodeDir%

pause