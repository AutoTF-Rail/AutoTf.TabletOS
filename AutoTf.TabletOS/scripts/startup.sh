#!/bin/bash
# Yes, this works. Only this way it works.
ssh root@127.0.0.1 "cd /home/display/AutoTf.TabletOS/AutoTf.TabletOS.Avalonia && /usr/local/bin/dotnet run --no-build -c RELEASE -m --drm"
