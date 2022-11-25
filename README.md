# SongOrganizer

## Features
- Filters by default/custom, played/unplayed, S-ranked/non-S-ranked tracks
- Saves your sort and filter configuration to `<TromboneChampDir>/BepInEx/config/SongOrganizer.cfg`
- Searches by first letter: typing a character (A-Z, 0-9) in track select will hop to the next track whose short name starts with that character
- Deletes highscores

## Usage
- Install BepInEx if you have not already done so. [Guide](https://trombone.wiki/#/installing-mods)
- Place the .dll in `<TromboneChampDir>/BepInEx/plugins`

## Changelog
v1.0.4
- Ability to delete highscores
- Fix bug in not being able to search by "Z"
- Fix sometimes bug when exiting the level select with stuff filtered out, coming back in, unfiltering, then selecting something near the end of the list

v1.0.3
- Typing a character (A-Z, 0-9) in track select will hop to the next track whose short name starts with that character

v1.0.2
- Fix for Trombone Champ 1.085

v1.0.0
- Ability to filter by default/custom, played/unplayed, S-ranked/non-S-ranked tracks
- Saves your sort and filter configuration to <TromboneChampDir>/BepInEx/config/SongOrganizer.cfg
