#!/bin/bash

fbi -T 1 -d /dev/fb0 -a -noverbose /home/display/AutoTf.TabletOS/AutoTf.TabletOS/Images/TabletOSLoadingOS.png 

eval "$(ssh-agent -s)"
    
ssh-add /home/display/githubKey

check_internet() {
  sleep 5
    echo "Checking lan connection"
    if ethtool eth0 | grep -q "Link detected: yes"; then
        echo "Internet is connected."
        return 0 
    else
        echo "No internet connection. Not pulling updates..."
        return 1 
    fi
}

check_git_changes() {
    echo "Checking for changes in the repository..."
    git fetch

    LOCAL=$(git rev-parse @)
    REMOTE=$(git rev-parse @{u})

    if [ "$LOCAL" = "$REMOTE" ]; then
        echo "No new changes detected."
        return 1
    else
        echo "New changes detected. Pulling updates..."
        return 0  
    fi
}


cd /home/display/AutoTf.TabletOS/AutoTf.TabletOS.Avalonia


if check_internet; then
    fbi -T 1 -d /dev/fb0 -a -noverbose /home/display/AutoTf.TabletOS/AutoTf.TabletOS/Images/TabletOSGettingUpdates.png 
    if check_git_changes; then
        git pull
        fbi -T 1 -d /dev/fb0 -a -noverbose /home/display/AutoTf.TabletOS/AutoTf.TabletOS/Images/TabletOSApplyingUpdates.png 
        dotnet build -m -c RELEASE
    else
        echo "Skipping git pull as there are no changes."
    fi
fi
fbi -T 1 -d /dev/fb0 -a -noverbose /home/display/AutoTf.TabletOS/AutoTf.TabletOS/Images/TabletOSStartingUp.png

dotnet run -c RELEASE --drm < /dev/tty1 > /dev/tty1 2>&1