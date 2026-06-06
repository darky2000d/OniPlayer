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
        Current,  // play last accessed video
        Next      // play next video alphabetically after last accessed
    }

    class Program
    {
        private static List<Location> locations = new List<Location>();
        private static readonly string configFile = "locations.cfg";
        private static readonly string settingsFile = "oniplayer.cfg";
        private static PlayMode currentMode = PlayMode.Current;
        private static readonly string[] videoExtensions = { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".m4v", ".mpg", ".mpeg", ".3gp", ".webm" };

        // Wave colors
        private static int colorPhase = 0;
        private static readonly ConsoleColor[] waveColors = { ConsoleColor.Green, ConsoleColor.Yellow, ConsoleColor.Magenta, ConsoleColor.Red, ConsoleColor.Blue, ConsoleColor.Cyan };

        static void Main(string[] args)
        {
            Console.Title = "Oni Player - Smart Video Resume";
            LoadLocations();
            LoadSettings();

            int selectedIndex = 0;
            bool running = true;

            while (running)
            {
                DrawMainMenu(selectedIndex);
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.UpArrow && locations.Count > 0)
                    selectedIndex = (selectedIndex - 1 + locations.Count) % locations.Count;
                else if (key.Key == ConsoleKey.DownArrow && locations.Count > 0)
                    selectedIndex = (selectedIndex + 1) % locations.Count;
                else if (key.Key == ConsoleKey.Enter && locations.Count > 0)
                {
                    PlayVideoBasedOnMode(locations[selectedIndex]);
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey(true);
                }
                else if (key.KeyChar == '1')
                {
                    AddLocation();
                    LoadLocations();
                    selectedIndex = 0;
                }
                else if (key.KeyChar == '2')
                {
                    RemoveLocation();
                    LoadLocations();
                    if (selectedIndex >= locations.Count) selectedIndex = Math.Max(0, locations.Count - 1);
                }
                else if (key.KeyChar == '3')
                {
                    ShowDetailsWrapper();
                }
                else if (key.KeyChar == '4')
                {
                    ResetAll();
                    LoadLocations();
                    selectedIndex = 0;
                }
                else if (key.KeyChar == '5')
                {
                    TogglePlayMode();
                }
                else if (key.Key == ConsoleKey.Escape)
                    running = false;
            }
        }

        // ==================== Settings ====================
        private static void LoadSettings()
        {
            if (!File.Exists(settingsFile)) return;
            string content = File.ReadAllText(settingsFile).Trim();
            if (content == "Next") currentMode = PlayMode.Next;
            else currentMode = PlayMode.Current;
        }

        private static void SaveSettings()
        {
            File.WriteAllText(settingsFile, currentMode == PlayMode.Next ? "Next" : "Current");
        }

        private static void TogglePlayMode()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- Play Mode Settings ---\n");
            Console.ResetColor();
            Console.WriteLine($"Current mode: {(currentMode == PlayMode.Current ? "Current Video" : "Next Video")}");
            Console.WriteLine("\n1. Current Video (always play last accessed)");
            Console.WriteLine("2. Next Video (play next after last accessed)");
            Console.WriteLine("Esc. Cancel");
            Console.Write("\nChoose (1/2/Esc): ");

            var key = Console.ReadKey(true);
            if (key.KeyChar == '1')
            {
                currentMode = PlayMode.Current;
                SaveSettings();
                Console.WriteLine("\nMode set to: Current Video");
            }
            else if (key.KeyChar == '2')
            {
                currentMode = PlayMode.Next;
                SaveSettings();
                Console.WriteLine("\nMode set to: Next Video");
            }
            else
            {
                Console.WriteLine("\nNo changes made.");
            }
            Console.WriteLine("\nPress any key...");
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
            var lines = locations.Select(l => $"{l.Nickname}|{l.Path}").ToArray();
            File.WriteAllLines(configFile, lines);
        }

        // ==================== Video Logic ====================
        private static string GetLastVideoInfo(string folderPath, out string videoPath)
        {
            videoPath = null;
            if (!Directory.Exists(folderPath)) return "Folder not found";

            var videoFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(f => videoExtensions.Contains(Path.GetExtension(f).ToLower()))
                .Select(f => new FileInfo(f))
                .ToList();

            if (videoFiles.Count == 0) return "No videos found";

            var last = videoFiles.OrderByDescending(f => f.LastAccessTime).FirstOrDefault();
            if (last == null || last.LastAccessTime.Year < 2000) return "Never played";

            videoPath = last.FullName;
            string name = last.Name;
            const int maxLen = 35;
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

            // Play Next Video mode
            string targetVideoPath = GetNextVideoPath(loc.Path);
            if (targetVideoPath == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nNext video not found (maybe last video is the last one).");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nPlaying next video after last accessed: {Path.GetFileName(targetVideoPath)}");
            Console.ResetColor();
            try { Process.Start(targetVideoPath); }
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

            // Get all video files sorted naturally (alphabetical, numeric aware)
            var allVideos = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                .Where(f => videoExtensions.Contains(Path.GetExtension(f).ToLower()))
                .Select(f => new FileInfo(f))
                .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (allVideos.Count == 0) return null;

            // Find last accessed video
            var lastAccessed = allVideos.OrderByDescending(f => f.LastAccessTime).FirstOrDefault();
            if (lastAccessed == null) return null;

            int index = allVideos.FindIndex(f => f.FullName == lastAccessed.FullName);
            if (index == -1) return null;

            // If last video is the last one, return null (no next)
            if (index + 1 >= allVideos.Count) return null;

            return allVideos[index + 1].FullName;
        }

        // ==================== UI Helpers (unchanged except added mode indicator) ====================
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
                else if (key.Key == ConsoleKey.Escape) done = true;
            }
            return result;
        }

        private static void DrawMainMenu(int selectedIndex)
        {
            Console.Clear();

            colorPhase = (colorPhase + 1) % (waveColors.Length * 3);
            int colorIndex = (colorPhase / 3) % waveColors.Length;
            ConsoleColor currentColor = waveColors[colorIndex];

            Console.ForegroundColor = currentColor;
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

            // Show current play mode
            Console.ForegroundColor = ConsoleColor.DarkYellow;
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
                    int maxWidth = Console.WindowWidth - 5;
                    if (display.Length > maxWidth) display = display.Substring(0, maxWidth - 3) + "...";

                    if (i == selectedIndex)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkCyan;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("> ");
                    }
                    else Console.Write("  ");

                    int sep = display.IndexOf("||");
                    if (sep >= 0)
                    {
                        Console.Write(display.Substring(0, sep + 2));
                        if (i == selectedIndex) Console.Write(display.Substring(sep + 2));
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Write(display.Substring(sep + 2));
                            Console.ResetColor();
                        }
                        Console.WriteLine();
                    }
                    else Console.WriteLine(display);
                    Console.ResetColor();
                }
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("1. Add New Location");
            Console.WriteLine("2. Remove Location");
            Console.WriteLine("3. Details Location");
            Console.WriteLine("4. Reset All");
            Console.WriteLine("5. Play Mode");
            Console.WriteLine("Esc. Back/Exit");
            Console.ResetColor();

            if (locations.Count > 0)
                Console.WriteLine("\n↑/↓: Select location   Enter: Play   Number: Menu action");
            else
                Console.WriteLine("\nUse number keys to manage locations.");
        }
    }
}