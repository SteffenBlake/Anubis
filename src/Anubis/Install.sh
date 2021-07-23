#!/bin/bash

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
SCRIPT_LOC="$SCRIPT_DIR/Anubis"
SERVICE_LOC="/etc/systemd/system/Anubis.service"

if test -f "$SERVICE_LOC"; then
    echo "Service already installed"
    exit
fi

cat > $SERVICE_LOC <<- EOM
[Unit]
Description=Anubis

[Service]
ExecStart=$SCRIPT_LOC
SyslogIdentifier=Anubis
User=$USER
Environment=DOTNET_ROOT=/opt/dotnet

[Install]
WantedBy=multi-user.target
EOM

systemctl daemon-reload

echo "Anubis installed."
echo "Use 'systemctl start Anubis' to start."
echo "Use 'systemctl status Anubis' for service status"
echo "Use 'sudo journalctl -u Anubis -f -n 50' for service logs"
echo "Use 'sudo systemctl enable Anubis' to enable auto-run on startup"
echo "And 'sudo systemctl disable Anubis' to revert"