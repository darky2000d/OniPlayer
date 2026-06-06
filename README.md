
Watching an offline course or series and forget which episode was next? this is a C# Console app that remembers the last video you played in a Folder. It shows and plays the last video instantly, or the next one. No more Forgetting!

<img width="1098" height="641" alt="image" src="https://github.com/user-attachments/assets/52cb56e9-e088-4519-8aa4-e95b5af35f12" />


# OniPlayer

A portable C# console app that remembers which video you last watched in any folder, using Windows **Last Access Time**.  
Perfect for offline courses or series with many episodes.

# Key Features
- **Add multiple folders** → give each a nickname, save forever  
- **Shows last played video name** right in the menu for every folder  
- **Play Mode**  
  - `Current` → plays the last accessed video  
  - `Next` → automatically plays the next file (alphabetically, with smart number sorting)  
- **History view** → see last 20 videos with their access times (option 3)  
- **Settings saved** in `locations.cfg` and `oniplayer.cfg` – stays in the same folder as the .exe  

## How to Use (important)
1. Put `OniPlayer.exe` in any folder (e.g., `D:\MyTools`).  
2. Run it once – it will create config files in that same folder.  
3. Create a shortcut to the .exe and place it anywhere you like (desktop, start menu).  
4. Whenever you open the app, your locations and play mode are remembered

> The config files must stay next to the .exe. So don't move the .exe alone – keep the whole folder or use a shortcut.

## Requirements & Note
- Windows 7+  
- .NET Framework 4.7.2 (usually preinstalled)  
- If `LastAccessTime` doesn't update, run once as admin:  
  `fsutil behavior set disablelastaccess 0` and restart.

## Build from source
Open in Visual Studio → Build. Or just use the provided Release .exe.

<img width="761" height="636" alt="image" src="https://github.com/user-attachments/assets/6898969e-fd05-4cef-81ff-a496345f1ff2" />
