#!/bin/bash

echo "Initializing Bluetooth..."
sudo btmgmt find 
sleep 2 

echo "Starting the .NET application..."

cd /home/display/AutoTf.TabletOS/AutoTf.TabletOS.Avalonia
/usr/local/bin/dotnet run --no-build -c RELEASE -m --drm