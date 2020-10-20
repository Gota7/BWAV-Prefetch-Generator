# BWAV Prefetch Generator
Create prefetch BWAVs given a BWAV. This is useful for editing music for SMM2 (Super Mario Maker 2) and possibly other Switch games.

## Usage
Drag and drop your BWAV onto the EXE and you will get the prefetch version of it.

## What's A Prefetch?
As you may have noticed, BWAVs contain many channels of streamed audio, making the filesize quite large for each file. However, when the music is to play, it takes time to load. But since lag would be noticeable, the game needs some way of loading part of the start of the audio while the data finishes loading. This is where prefetches come in handy, to give the game a demo version of the music before the full version loads in memory.

## How Do I Make A BWAV?
BWAVs can be created in Isabelle Sound Maker in my tool, Citric Composer:
https://gota7.github.io/Citric-Composer/

## I'm A Developer! How Does A BWAV Work?
See my specification on my reverse engineering of the format here:
https://gota7.github.io/Citric-Composer/specs/binaryWav.html

## Why Is Replacing A Prefetch BWAV Needed To Fix Static?
BWAVs use what is called DSP-ADPCM encoding, which has certain coefficients to decode the audio data to PCM (which your speakers like). Since the game reads a prefetch file first, it assumes the loop and coefficients match the actual one, which should be the case! However, if the prefetch is not replaced it will still try and decode your custom audio as if it were the original, which causes static.

## Instructions For Mario Maker 2
1. Drag your BWAV into the tool above.
2. Open bars.pack in Switch Toolbox: https://github.com/KillzXGaming/Switch-Toolbox/releases
3. Replace the BWAV with the same name as your custom song with the generated prefetch BWAV.
4. Save, and enjoy!