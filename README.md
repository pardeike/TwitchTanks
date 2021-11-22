# TwitchTanks

With TwitchTanks you can create a tank game overlay for your twitch stream and your viewers can use chat commands to control up to four tanks and battle each other.

<img src="https://github.com/pardeike/TwitchTanks/raw/master/Originals/screenshot.jpg"/>

# Setup

- Download the latest [release](https://github.com/pardeike/TwitchTanks/releases/latest) and unzip it into a folder `TwitchTanks`
- Create a twitch account for the bot
- Go to https://twitchapps.com/tmi/ while being logged into the bot account
- Open the file at `TwitchTanks\TwitchTanks_Data\twitch-auth.txt` and enter
  - bot account name
  - aouth.... from the generator
  - your channel name
- Start `TwitchTanks.exe` to get the green window
- Start OBS and add a new Window Capture
  - Window: TwitchTanks.exe
  - Capture Method: Windows 10 (1903 and up)
  - Window Match Priority: Match title, otherwise find window of same type
  - Client Area: X
- You should now see a green area in OBS, make it fullscreen
- Right-click the tanks layer and add a Filter:
  - Chroma Key
  - Key Color Type: Green
  - Similarity: 209
  - Smoothness: 150
  - Key Color Spill Reduction: 115
  - Opacity: 1
  - Contrast: -0.1
  - Brightness: -0.0544
  - Gamma: -0.07

# Gameplay

Short build in help with `!about`.

Up two four players can play at the same time. You can join the game or the queue with `!join`.

Once in the game, you can move your tank forwards and backwards with `!move +/-N` where N is a number around roughly 20-150.

You can turn your tank with `!turn +/-N` where N is the degrees you want to turn (+ is clockwise).

Finally, you can shoot with `!shoot N` where N is a number around roughly 20-150.

Multiple commands will overwrite each other by type. Per turn, you can only Move+Rotate+Shoot or Rotate+Move+Shoot. The turn time is indicated in the top left corner. Once you're hit you will leave the game and can queue again with `!join`.

ENJOY
