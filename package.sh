#!/bin/bash
rm SongOrganizer.zip
cp bin/Debug/netstandard2.1/SongOrganizer.dll SongOrganizer.dll
zip SongOrganizer.zip icon.png manifest.json README.md SongOrganizer.dll
rm SongOrganizer.dll
