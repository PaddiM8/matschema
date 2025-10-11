#!/usr/bin/sh

mkdir -p build
cd PriceScraper
dotnet publish -c Release -o ../build

