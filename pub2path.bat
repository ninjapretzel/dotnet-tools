dotnet publish -r win-x64 -p:PublishSingleFile=true --self-contained false
copy /y bin\Debug\net5.0\win-x64\publish\*.exe D:\NON-OS\pth\