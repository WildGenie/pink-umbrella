#!/bin/sh

sudo service pink-umbrella stop
dotnet build
dotnet ef database update --context SimpleDbContext
dotnet ef database update --context AhPushItDbContext
dotnet ef database update --context LogDbContext
dotnet publish --configuration Release
sudo service pink-umbrella start
