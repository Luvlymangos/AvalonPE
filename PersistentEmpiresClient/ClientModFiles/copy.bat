@echo off
set "source_folder=.\GUI"
set "destination_folder=E:\SteamLibrary\steamapps\workshop\content\261550\3263813166\GUI"

echo Copying GUI folder...

xcopy /s /i "%source_folder%" "%destination_folder%"

echo GUI folder copied successfully.
pause