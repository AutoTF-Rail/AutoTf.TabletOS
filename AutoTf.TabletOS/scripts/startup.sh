#!/bin/bash

#exec > /dev/tty1 2>&1
TTY1="/dev/tty1"

eval "$(ssh-agent -s)"
    
ssh-add /home/display/githubKey

check_internet() {
    echo "Checking internet connection..."
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

echo "Redirecting script output to $TTY1..."
exec > $TTY1 2>&1 

cd /home/display/AutoTf.TabletOS/AutoTf.TabletOS

if check_internet; then
    if check_git_changes; then
        git pull
    dotnet build
    else
        echo "Skipping git pull as there are no changes."
    fi
    
fi

echo "Running the application on tty1..."
dotnet run 2>&1 | tee /dev/tty1
