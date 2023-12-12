# SongOrganizer

## Features
- Filter by default/custom, played/unplayed, S-ranked/non-S-ranked, rated/unrated tracks
- Search bar: searches by long name, short name, artist, description
- Save your sort, filter, last song index to `<TromboneChampDir>/BepInEx/config/SongOrganizer.cfg`
- Search by first letter: typing a character (A-Z, 0-9) in track select will hop to the next track whose short name starts with that character
- Delete highscores individually and by song

## Usage
- Install BepInEx if you have not already done so. [Guide](https://trombone.wiki/#/installing-mods)
- Place the .dll in `<TromboneChampDir>/BepInEx/plugins`

## Changelog
v1.3.6
- Fix for TC 1.18B

v1.3.5
- Log mirrors instead of the original download links
- Remove delete buttons because they don't work
- Make search dropdown over buttons if no toottally

v1.3.4
- Fix bug with unrated base game tracks

v1.3.3
- Fix bug with reading rated tracks from toottally

v1.3.2
- Fix bug with reloading tracks when leaving song select

v1.3.1
- Fix bug with reloading tracks
- Fix bug where any track with the same trackref as a rated track will be considered a rated track, even when it's not
- Log the download link of missing rated tracks
- Add sort by long name
- Ctrl+F to search

v1.2.3
- Sort by artist for Trombone Champ 1.11B

v1.2.2
- Fix search bar for Trombone Champ 1.10B

v1.2.1
- Trombloader v2 compatibility

v1.2.0
- Search bar: searches by long name, short name, artist, description

v1.1.0
- Filter by rated/unrated tracks
  - Entering the home screen will call https://toottally.com/api/search/?rated=1&page=1&page_size=100000 and save the result to BepInEx/config/rated.json
  - Entering the level select screen will read the rated.json file to determine which are rated tracks. It'll also log the shortname of rated tracks you don't have.

v1.0.91
- Remove search by long name for TC 1.095

v1.0.9
- Delete button changes, remove #1 star for TC 1.09

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
