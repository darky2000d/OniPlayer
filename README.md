# OniPlayer  
### *Goldfish memory? 200 episodes? No problem.*
A C# console app that remembers your last watched video In Any Folders.  
One key, no thinking.
---

## What it actually does

You have folders. They’re full of videos.  
You don’t remember which one you watched last.  
**OniPlayer** shows the name of that video right in the menu.

- Press **Enter** → it plays the last video you watched.  
- Press **→** (right arrow) → it switches to “Play Next”. Then Enter plays the next video instead.

That’s the whole idea. No setup, no config, no remembering.

---

<img width="1104" height="639" alt="image" src="https://github.com/user-attachments/assets/4143b610-714c-4409-92a7-a4b976f5e5d8" />


---

## Why you might want it

- You learn in batches and hate the “what did I watch?” guessing game.  
- You have multiple courses, series, or workout videos spread across different folders.  
- You want a tool that’s portable, keyboard‑only, and doesn’t require 47 clicks.  
- You enjoy software that roasts you a little bit.

---

## Key Features (v1.3)

- **Instant resume** – reads Windows’ “Last Access Time” to know what you watched last. Yes, it works even if you double‑clicked the file manually.
- **Two play modes**  
  - `Current` – play the last‑accessed video (the one you *should* continue).  
  - `Next` – play the next file alphabetically (when you actually finished the previous one).
- **Quick‑switch on the fly** – while a folder is highlighted, press **→** to light up `Play Next` (or `Play Current`). Press Enter and it plays the opposite mode *without* changing your global setting. Press **←** to go back.
- **Full keyboard navigation** – move through folders and the menu with **↑/↓**. No mouse required.
- **Multiple folders** – add as many as you like (press `1`), give them nicknames.
- **History peek** – see the last 20 videos in a folder with exact access times (press `3`). Perfect for catching where you left off.

<img width="1104" height="639" alt="image" src="https://github.com/user-attachments/assets/9f0f7775-6e9e-42a7-8191-d56b954f94cc" />


- **Portable** – all settings live in two tiny files (`locations.cfg` & `oniplayer.cfg`) next to the .exe. Put it on a USB stick, sync it with Dropbox, whatever. Your place follows you.
- **Colour‑coded simplicity** – video names glow green, the action button stands out, and the title cycles through red/blue/green. Even a goldfish could read it.

---

## How to Use (the essentials)

1. Put `OniPlayer.exe` in a folder (e.g. `D:\Tools\OniPlayer`).
2. Run it – it creates the config files automatically.
3. Add a video folder (press `1`) and give it a nickname.
4. Use **↑/↓** to choose a folder, **Enter** to play.
5. Press **→** on a folder to highlight `Play Next` / `Play Current`, then Enter to play that mode.
6. Change the global play mode by pressing `5` or navigating to “Play Mode”.
7. Exit with `Esc` – everything is saved.

> ⚠ Keep the config files beside the .exe. Just make a shortcut to the .exe if you want it on your desktop.

---

## Requirements & Note

- Windows 7 or later
- .NET Framework 4.7.2 (usually preinstalled)
- If `LastAccessTime` isn’t updating (rare), run once as admin:  
  `fsutil behavior set disablelastaccess 0` and restart.

---

## What’s new in v1.3

- **Unified navigation** – arrow keys now move through both folders and bottom menu items.
- **Action focus** – press **→** on a folder to highlight the alternative play mode, Enter to play it instantly.
- **Fresh colour scheme** – video filenames in green, action button in magenta (or cyan), title cycling.
- **Play Mode settings** now lets you pick with arrow keys instead of typing numbers.
- **Performance and code improvements** under the hood.

---

## Build from source
Open in Visual Studio → Build. Or download the latest Release .exe from the [Releases page](https://github.com/SobieArts/OniPlayer).

---

*“I binged a 120‑episode course and never once had to wonder where I left off. OniPlayer just… knows.”*
