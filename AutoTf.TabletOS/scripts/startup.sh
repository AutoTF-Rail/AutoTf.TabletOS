#!/bin/bash

check_internet() {
    echo "Checking internet connection..."
    if ping -c 1 8.8.8.8 &>/dev/null; then
        echo "Internet is connected."
        return 0 
    else
        echo "No internet connection. Exiting..."
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

cd /home/display

if check_internet; then
    eval "$(ssh-agent -s)"
    
    ssh-add /home/display/githubKey
    
    cd /home/display/AutoTf.TabletOS/AutoTf.TabletOS
    
    if check_git_changes; then
        git pull
    dotnet build
    else
        echo "Skipping git pull as there are no changes."
    fi
    
fi

dotnet run
