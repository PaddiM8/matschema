#!/usr/bin/sh

git pull
./build.sh
sudo systemctl restart pricescraper
