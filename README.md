# SongOrganizer

## Features
- Filter by default/custom, played/unplayed, S-ranked/non-S-ranked tracks
- Save your sort, filter, last song index to `<TromboneChampDir>/BepInEx/config/SongOrganizer.cfg`
- Search by first letter: typing a character (A-Z, 0-9) in track select will hop to the next track whose short name starts with that character
- Delete highscores individually and by song

## Usage
- Install BepInEx if you have not already done so. [Guide](https://trombone.wiki/#/installing-mods)
- Place the .dll in `<TromboneChampDir>/BepInEx/plugins`

## Changelog
v1.0.8
- Save last played song in config. Thanks @Electrostats!

v1.0.7
- Max score changes for TC 1.0881

v1.0.6
- Fix alignment issues for Trombone Champ 1.087

v1.0.51
- Fix bug in calculating max score

v1.0.5
- Ability to delete all highscores of a song

v1.0.4
- Ability to delete highscores individually
- Fix bug in not being able to search by "Z"
- Fix sometimes bug when exiting the level select with stuff filtered out, coming back in, unfiltering, then selecting something near the end of the list

v1.0.3
- Typing a character (A-Z, 0-9) in track select will hop to the next track whose short name starts with that character

v1.0.2
- Fix for Trombone Champ 1.085

v1.0.0
- Ability to filter by default/custom, played/unplayed, S-ranked/non-S-ranked tracks
- Saves your sort and filter configuration to <TromboneChampDir>/BepInEx/config/SongOrganizer.cfg
