//-------------Updated to V1.3 with new features--------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace OniPlayer
{
    class Location
    {
        public string Nickname { get; set; }
        public string Path { get; set; }
    }

    enum PlayMode
    {
        Current,
        Next
    }

    class Program
    {
        private static List<Location> locations = new List<Location>();
        private static readonly string configFile = "locations.cfg";
        private static readonly string settingsFile = "oniplayer.cfg";
        private static PlayMode currentMode = PlayMode.Current;
        private static readonly string[] videoExtensions = { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".m4v", ".mpg", ".mpeg", ".3gp", ".webm" };

        private static int drawCallCounter = 0;
        private static int titleColorIndex = 0;
        private static readonly ConsoleColor[] titleColors = { ConsoleColor.Red, ConsoleColor.Blue, ConsoleColor.Green };

        private static readonly string[] menuItems =
        {
            "1. Add New Location",
            "2. Remove Location",
            "3. Details Location",
            "4. Reset All",
            "5. Play Mode"
        };

        private static int selectedIndex = 0;
        private static bool focusOnAction = false;

        static void Main(string[] args)
        {
            Console.Title = "Oni Player - Smart Video Resume";
            LoadLocations();
            LoadSettings();
            ClampSelectedIndex();

            bool running = true;
            while (running)
            {
                DrawMainMenu();
                var key = Console.ReadKey(true);
                int totalItems = locations.Count + menuItems.Length;

                if (key.Key == ConsoleKey.UpArrow && totalItems > 0)
                {
                    selectedIndex = (selectedIndex - 1 + totalItems) % totalItems;
                    focusOnAction = false;
                }
                else if (key.Key == ConsoleKey.DownArrow && totalItems > 0)
                {
                    selectedIndex = (selectedIndex + 1) % totalItems;
                    focusOnAction = false;
                }
                else if (key.Key == ConsoleKey.LeftArrow && totalItems > 0)
                {
                    if (selectedIndex < locations.Count && focusOnAction)
                        focusOnAction = false;
                }
                else if (key.Key == ConsoleKey.RightArrow && totalItems > 0)
                {
                    if (selectedIndex < locations.Count && !focusOnAction)
                        focusOnAction = true;
                }
                else if (key.Key == ConsoleKey.Enter && totalItems > 0)
                {
                    if (selectedIndex < locations.Count)
                    {
                        if (focusOnAction)
                        {
                            PlayMode opposite = currentMode == PlayMode.Next ? PlayMode.Current : PlayMode.Next;
                            if (opposite == PlayMode.Current)
                                PlayLastVideo(locations[selectedIndex]);
                            else
                            {
                                string nextPath = GetNextVideoPath(locations[selectedIndex].Path);
                                if (nextPath == null)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("\nNext video not found (maybe last video is the last one).");
                                    Console.ResetColor();
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"\nPlaying next video after last accessed: {Path.GetFileName(nextPath)}");
                                    Console.ResetColor();
                                    try { Process.Start(nextPath); }
                                    catch (Exception ex)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"Error opening player: {ex.Message}");
                                        Console.ResetColor();
                                    }
                                }
                            }
                        }
                        else
                        {
                            PlayVideoBasedOnMode(locations[selectedIndex]);
                        }
                        focusOnAction = false;
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey(true);
                    }
                    else
                    {
                        int menuIdx = selectedIndex - locations.Count;
                        ExecuteMenuAction(menuIdx);
                    }
                }
                else if (key.KeyChar == '1') ExecuteMenuAction(0);
                else if (key.KeyChar == '2') ExecuteMenuAction(1);
                else if (key.KeyChar == '3') ExecuteMenuAction(2);
                else if (key.KeyChar == '4') ExecuteMenuAction(3);
                else if (key.KeyChar == '5') ExecuteMenuAction(4);
                else if (key.Key == ConsoleKey.Escape) running = false;
            }
        }

        private static void ExecuteMenuAction(int idx)
        {
            switch (idx)
            {
                case 0: AddLocation(); LoadLocations(); ClampSelectedIndex(); break;
                case 1: RemoveLocation(); LoadLocations(); ClampSelectedIndex(); break;
                case 2: ShowDetailsWrapper(); break;
                case 3: ResetAll(); LoadLocations(); ClampSelectedIndex(); break;
                case 4: TogglePlayMode(); break;
            }
        }

        private static void ClampSelectedIndex()
        {
            int total = locations.Count + menuItems.Length;
            if (total == 0) total = 1;
            if (selectedIndex >= total) selectedIndex = total - 1;
            if (selectedIndex < 0) selectedIndex = 0;
        }

        // ==================== Settings ====================
        private static void LoadSettings()
        {
            if (!File.Exists(settingsFile)) return;
            string content = File.ReadAllText(settingsFile).Trim();
            currentMode = (content == "Next") ? PlayMode.Next : PlayMode.Current;
        }

        private static void SaveSettings()
        {
            File.WriteAllText(settingsFile, currentMode == PlayMode.Next ? "Next" : "Current");
        }

        private static void TogglePlayMode()
        {
            int selected = 0; // 0 = Current, 1 = Next
            bool done = false;

            while (!done)
            {
                Console.Clear();

                // Header
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("--- Play Mode Settings ---\n");
                Console.ResetColor();

                // Option 1
                if (selected == 0)
                {
                    Console.BackgroundColor = ConsoleColor.DarkCyan;
                    Console.ForegroundColor = ConsoleColor.White;
                }
              
                Console.Write("  1. ");
                Console.WriteLine("Current - (play current vid)");
                Console.ResetColor();

                // Option 2
                if (selected == 1)
                {
                    Console.BackgroundColor = ConsoleColor.DarkCyan;
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.Write("  2. ");
                Console.WriteLine("Next - (play next vid)");
                Console.ResetColor();

                // Cancel hint
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("  Esc. Cancel");
                Console.ResetColor();

                // Prompt
                Console.Write("\nChoose (↑/↓ to select, Enter to confirm, Esc to cancel): ");
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.DownArrow)
                {
                    selected = (selected + 1) % 2; // only two options, toggle
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    done = true;
                    if (selected == 0)
                    {
                        currentMode = PlayMode.Current;
                        SaveSettings();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n Mode set to: Current Video");
                        Console.ResetColor();
                    }
                    else
                    {
                        currentMode = PlayMode.Next;
                        SaveSettings();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n Mode set to: Next Video");
                        Console.ResetColor();
                    }
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    done = true;
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("\nNo changes made.");
                    Console.ResetColor();
                }
                else if (key.KeyChar == '1')
                {
                    done = true;
                    currentMode = PlayMode.Current;
                    SaveSettings();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\n✔ Mode set to: Current Video");
                    Console.ResetColor();
                }
                else if (key.KeyChar == '2')
                {
                    done = true;
                    currentMode = PlayMode.Next;
                    SaveSettings();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n Mode set to: Next Video");
                    Console.ResetColor();
                }
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
        }
        // ==================== Location Management ====================
        private static void LoadLocations()
        {
            locations.Clear();
            if (!File.Exists(configFile)) return;
            var lines = File.ReadAllLines(configFile);
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length == 2)
                    locations.Add(new Location { Nickname = parts[0], Path = parts[1] });
            }
        }

        private static void SaveLocations()
        {
            File.WriteAllLines(configFile, locations.Select(l => $"{l.Nickname}|{l.Path}"));
        }

        // ==================== Video Logic ====================
        private static string GetLastVideoInfo(string folderPath, out string videoPath)
        {
            videoPath = null;
            if (!Directory.Exists(folderPath)) return "Folder not found";

            var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(f => videoExtensions.Contains(Path.GetExtension(f).ToLower()))
                .Select(f => new FileInfo(f)).ToList();

            if (files.Count == 0) return "No videos found";

            var last = files.OrderByDescending(f => f.LastAccessTime).FirstOrDefault();
            if (last == null || last.LastAccessTime.Year < 2000) return "Never played";

            videoPath = last.FullName;
            string name = last.Name;
            const int maxLen = 45;
            if (name.Length > maxLen) name = name.Substring(0, maxLen - 3) + "...";
            return name;
        }

        private static void PlayVideoBasedOnMode(Location loc)
        {
            if (currentMode == PlayMode.Current)
            {
                PlayLastVideo(loc);
                return;
            }

            string nextPath = GetNextVideoPath(loc.Path);
            if (nextPath == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nNext video not found (maybe last video is the last one).");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nPlaying next video after last accessed: {Path.GetFileName(nextPath)}");
            Console.ResetColor();
            try { Process.Start(nextPath); }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error opening player: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static void PlayLastVideo(Location loc)
        {
            string videoPath;
            string videoName = GetLastVideoInfo(loc.Path, out videoPath);
            if (videoPath == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nCould not play: {videoName}");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nPlaying last video from '{loc.Nickname}': {Path.GetFileName(videoPath)}");
            Console.ResetColor();
            try { Process.Start(videoPath); }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error opening player: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static string GetNextVideoPath(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return null;

            var allVideos = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(f => videoExtensions.Contains(Path.GetExtension(f).ToLower()))
                .Select(f => new FileInfo(f))
                .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase).ToList();

            if (allVideos.Count == 0) return null;

            var lastAccessed = allVideos.OrderByDescending(f => f.LastAccessTime).FirstOrDefault();
            if (lastAccessed == null) return null;

            int idx = allVideos.FindIndex(f => f.FullName == lastAccessed.FullName);
            if (idx == -1 || idx + 1 >= allVideos.Count) return null;

            return allVideos[idx + 1].FullName;
        }

        // ==================== UI Helpers ====================
        private static void ShowDetailsWrapper()
        {
            if (locations.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nNo locations to show details.");
                Console.ResetColor();
                Console.WriteLine("\nPress any key...");
                Console.ReadKey(true);
                return;
            }

            int idx = SelectLocation("Select location to show details:", true);
            if (idx >= 0) ShowDetails(locations[idx]);
        }

        private static void ShowDetails(Location loc)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Details for: {loc.Nickname} ({loc.Path})\n");
            Console.ResetColor();

            if (!Directory.Exists(loc.Path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Directory does not exist.");
                Console.ResetColor();
                Console.WriteLine("\nPress any key...");
                Console.ReadKey(true);
                return;
            }

            var videoFiles = Directory.GetFiles(loc.Path, "*.*", SearchOption.AllDirectories)
                .Where(f => videoExtensions.Contains(Path.GetExtension(f).ToLower()))
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastAccessTime)
                .Take(20)
                .ToList();

            if (videoFiles.Count == 0) Console.WriteLine("No video files found.");
            else
            {
                Console.WriteLine("{0,-5} {1,-40} {2}", "No.", "File Name", "Last Access Time");
                Console.WriteLine(new string('-', 70));
                for (int i = 0; i < videoFiles.Count; i++)
                {
                    var f = videoFiles[i];
                    string name = f.Name.Length > 40 ? f.Name.Substring(0, 37) + "..." : f.Name;
                    Console.WriteLine("{0,-5} {1,-40} {2}", i + 1, name, f.LastAccessTime);
                }
                if (videoFiles.Count == 20) Console.WriteLine("\n... showing top 20 videos.");
            }

            Console.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
        }

        private static void AddLocation()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("--- Add New Location (Press Esc to cancel) ---\n");
            Console.ResetColor();

            string path = "";
            while (true)
            {
                Console.Write("Enter folder path (Esc to cancel): ");
                string input = ReadLineWithEsc();
                if (input == null)
                {
                    Console.WriteLine("\nOperation cancelled.");
                    Console.WriteLine("\nPress any key...");
                    Console.ReadKey(true);
                    return;
                }
                path = input.Trim();
                if (Directory.Exists(path)) break;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Directory does not exist. Try again.");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.Write("Enter a nickname (Enter = folder name, Esc to cancel): ");
            string nickname = ReadLineWithEsc();
            if (nickname == null)
            {
                Console.WriteLine("\nOperation cancelled.");
                Console.WriteLine("\nPress any key...");
                Console.ReadKey(true);
                return;
            }
            if (string.IsNullOrEmpty(nickname)) nickname = new DirectoryInfo(path).Name;

            locations.Add(new Location { Nickname = nickname, Path = path });
            SaveLocations();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nLocation '{nickname}' added successfully.");
            Console.ResetColor();
            Console.WriteLine("\nPress any key...");
            Console.ReadKey(true);
        }

        private static string ReadLineWithEsc()
        {
            string result = "";
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape) return null;
                if (key.Key == ConsoleKey.Enter) return result;
                if (key.Key == ConsoleKey.Backspace && result.Length > 0)
                {
                    result = result.Substring(0, result.Length - 1);
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    result += key.KeyChar;
                    Console.Write(key.KeyChar);
                }
            }
        }

        private static void RemoveLocation()
        {
            if (locations.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nNo locations to remove.");
                Console.ResetColor();
                Console.WriteLine("\nPress any key...");
                Console.ReadKey(true);
                return;
            }

            int idx = SelectLocation("Select location to remove:", true);
            if (idx < 0) return;

            var loc = locations[idx];
            Console.WriteLine();
            Console.Write($"Are you sure you want to remove '{loc.Nickname}'? (y/n): ");
            var answer = Console.ReadKey(true);
            Console.WriteLine();
            if (answer.KeyChar == 'y' || answer.KeyChar == 'Y')
            {
                locations.RemoveAt(idx);
                SaveLocations();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Location removed.");
                Console.ResetColor();
            }
            else Console.WriteLine("Removal cancelled.");

            Console.WriteLine("\nPress any key...");
            Console.ReadKey(true);
        }

        private static void ResetAll()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("--- RESET ALL CONFIGURATION ---\n");
            Console.ResetColor();
            Console.Write("This will delete all saved locations and settings. Are you sure? (y/n): ");
            var key = Console.ReadKey(true);
            Console.WriteLine();
            if (key.KeyChar == 'y' || key.KeyChar == 'Y')
            {
                if (File.Exists(configFile)) File.Delete(configFile);
                if (File.Exists(settingsFile)) File.Delete(settingsFile);
                locations.Clear();
                currentMode = PlayMode.Current;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("All data has been reset.");
                Console.ResetColor();
            }
            else Console.WriteLine("Reset cancelled.");

            Console.WriteLine("\nPress any key...");
            Console.ReadKey(true);
        }

        private static int SelectLocation(string prompt, bool showPath)
        {
            int selected = 0;
            bool done = false;
            int result = -1;

            while (!done)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{prompt}\n");
                Console.ResetColor();

                for (int i = 0; i < locations.Count; i++)
                {
                    if (i == selected)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkCyan;
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Black;
                    }

                    if (showPath)
                        Console.WriteLine($"{i + 1}. {locations[i].Nickname}  ->  {locations[i].Path}");
                    else
                        Console.WriteLine($"{i + 1}. {locations[i].Nickname}");

                    Console.ResetColor();
                }

                Console.WriteLine("\n[Up/Down] select, [Enter] confirm, [Esc] cancel");

                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.UpArrow)
                    selected = (selected - 1 + locations.Count) % locations.Count;
                else if (key.Key == ConsoleKey.DownArrow)
                    selected = (selected + 1) % locations.Count;
                else if (key.Key == ConsoleKey.Enter)
                {
                    result = selected;
                    done = true;
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    done = true;  // result remains -1
                }
            }
            return result;
        }

        // ==================== MAIN MENU DRAWING ====================
        private static void DrawMainMenu()
        {
            Console.Clear();
            drawCallCounter++;
            if (drawCallCounter % 7 == 0)
                titleColorIndex = (titleColorIndex + 1) % titleColors.Length;

            Console.ForegroundColor = titleColors[titleColorIndex];
            string art = @"
   ___        _   ____  _                       
  / _ \ _ __ (_) |  _ \| | __ _ _   _  ___ _ __ 
 | | | | '_ \| | | |_) | |/ _` | | | |/ _ \ '__|
 | |_| | | | | | |  __/| | (_| | |_| |  __/ |   
  \___/|_| |_|_| |_|   |_|\__,_|\__, |\___|_|   
                  Made By Sobie |___/      
  ---------------------------------------------
";
            Console.WriteLine(art);
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"  [Play Mode: {(currentMode == PlayMode.Current ? "Current Video" : "Next Video")}]");
            Console.ResetColor();

            if (locations.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nNo locations added yet. Use menu below to add.\n");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine();
                for (int i = 0; i < locations.Count; i++)
                {
                    var loc = locations[i];
                    string lastVideo = GetLastVideoInfo(loc.Path, out _);
                    string display = $"{loc.Nickname} || {lastVideo}";

                    if (i == selectedIndex && selectedIndex < locations.Count)
                    {
                        string actionSuffix = " || Play " + (currentMode == PlayMode.Next ? "Current" : "Next");
                        int mainMax = Console.WindowWidth - 2 - actionSuffix.Length;
                        string mainDisplay = display;
                        if (mainDisplay.Length > mainMax)
                            mainDisplay = mainDisplay.Substring(0, mainMax - 3) + "...";

                        int sepIdx = mainDisplay.IndexOf("||");
                        string locPart = (sepIdx >= 0) ? mainDisplay.Substring(0, sepIdx + 2) : mainDisplay;
                        string vidPart = (sepIdx >= 0) ? mainDisplay.Substring(sepIdx + 2) : "";

                        if (!focusOnAction)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkCyan;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("> ");
                            Console.Write(locPart);
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(vidPart);
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(actionSuffix);
                            Console.ResetColor();
                            Console.WriteLine();
                        }
                        else
                        {
                            Console.BackgroundColor = ConsoleColor.DarkCyan;
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("> ");
                            Console.Write(locPart);
                            Console.ForegroundColor = ConsoleColor.White;     
                            Console.Write(vidPart);

                            
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write(" || ");

                            
                            Console.BackgroundColor = ConsoleColor.DarkBlue;    
                            Console.Write("Play " + (currentMode == PlayMode.Next ? "Current" : "Next"));

                            Console.ResetColor();
                            Console.WriteLine();
                        }
                    }
                    else
                    {
                        int maxWidth = Console.WindowWidth - 5;
                        string lineDisplay = display;
                        if (lineDisplay.Length > maxWidth)
                            lineDisplay = lineDisplay.Substring(0, maxWidth - 3) + "...";
                        Console.Write("  ");
                        int sep = lineDisplay.IndexOf("||");
                        if (sep >= 0)
                        {
                            Console.Write(lineDisplay.Substring(0, sep + 2));
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Write(lineDisplay.Substring(sep + 2));
                            Console.ResetColor();
                        }
                        else Console.Write(lineDisplay);
                        Console.WriteLine();
                    }
                }
            }

            Console.WriteLine();
            for (int i = 0; i < menuItems.Length; i++)
            {
                int unifiedIdx = locations.Count + i;
                if (unifiedIdx == selectedIndex)
                {
                    Console.BackgroundColor = ConsoleColor.DarkCyan;
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.WriteLine(menuItems[i]);
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Esc. Back/Exit");
            Console.ResetColor();

            if (locations.Count > 0)
                Console.WriteLine("\n↑/↓: Select   ←/→: Toggle Play action   Enter: Execute   Number keys: Direct");
            else
                Console.WriteLine("\n↑/↓: Select   Enter: Execute   Number keys: Direct");
        }
    }
}
