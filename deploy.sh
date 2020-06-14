#!/bin/sh

sudo service pink-umbrella stop
dotnet build
dotnet publish --configuration Release
sudo service pink-umbrella start
