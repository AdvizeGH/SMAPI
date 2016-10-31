﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
#if SMAPI_FOR_WINDOWS
using System.Windows.Forms;
#endif
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Inheritance;
using StardewValley;

namespace StardewModdingAPI
{
    public class Program
    {
        /// <summary>The full path to the Stardew Valley executable.</summary>
        private static readonly string GameExecutablePath = File.Exists(Path.Combine(Constants.ExecutionPath, "StardewValley.exe"))
            ? Path.Combine(Constants.ExecutionPath, "StardewValley.exe") // Linux or Mac
            : Path.Combine(Constants.ExecutionPath, "Stardew Valley.exe"); // Windows

        /// <summary>The full path to the folder containing mods.</summary>
        private static readonly string ModPath = Path.Combine(Constants.ExecutionPath, "Mods");

        public static SGame gamePtr;
        public static bool ready;

        public static Assembly StardewAssembly;
        public static Type StardewProgramType;
        public static FieldInfo StardewGameInfo;

        public static Thread gameThread;
        public static Thread consoleInputThread;

        public static Texture2D DebugPixel { get; private set; }

        // ReSharper disable once PossibleNullReferenceException
        public static int BuildType => (int) StardewProgramType.GetField("buildType", BindingFlags.Public | BindingFlags.Static).GetValue(null);

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        ///     Main method holding the API execution
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");

            try
            {
                Log.AsyncY("SDV Version: " + Game1.version);
                Log.AsyncY("SMAPI Version: " + Constants.Version.VersionString);
                ConfigureUI();
                CreateDirectories();
                StartGame();
            }
            catch (Exception e)
            {
                // Catch and display all exceptions. 
                Console.WriteLine(e);
                Console.ReadKey();
                Log.AsyncR("Critical error: " + e);
            }

            Log.AsyncY("The API will now terminate. Press any key to continue...");
            Console.ReadKey();
        }

        /// <summary>
        ///     Set up the console properties
        /// </summary>
        private static void ConfigureUI()
        {
            Console.Title = Constants.ConsoleTitle;
#if DEBUG
            Console.Title += " - DEBUG IS NOT FALSE, AUTHOUR NEEDS TO REUPLOAD THIS VERSION";
#endif
        }

        /// <summary>Create and verify the SMAPI directories.</summary>
        private static void CreateDirectories()
        {
            Log.AsyncY("Validating file paths...");
            VerifyPath(ModPath);
            VerifyPath(Constants.LogDir);
            if (!File.Exists(GameExecutablePath))
                throw new FileNotFoundException($"Could not find executable: {GameExecutablePath}");
        }

        /// <summary>
        ///     Load Stardev Valley and control features, and launch the game.
        /// </summary>
        private static void StartGame()
        {
            // Load in the assembly - ignores security
            Log.AsyncY("Initializing SDV Assembly...");
            StardewAssembly = Assembly.UnsafeLoadFrom(GameExecutablePath);
            StardewProgramType = StardewAssembly.GetType("StardewValley.Program", true);
            StardewGameInfo = StardewProgramType.GetField("gamePtr");

            // Change the game's version
            Log.AsyncY("Injecting New SDV Version...");
            Game1.version += $"-Z_MODDED | SMAPI {Constants.Version.VersionString}";

            // add error interceptors
#if SMAPI_FOR_WINDOWS
            Application.ThreadException += Log.Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
#endif
            AppDomain.CurrentDomain.UnhandledException += Log.CurrentDomain_UnhandledException;

            // initialise game
            try
            {
                Log.AsyncY("Initializing SDV...");
                gamePtr = new SGame();

                // hook events
                gamePtr.Exiting += (sender, e) => ready = false;
                gamePtr.Window.ClientSizeChanged += GraphicsEvents.InvokeResize;

                // patch graphics
                Log.AsyncY("Patching SDV Graphics Profile...");
                Game1.graphics.GraphicsProfile = GraphicsProfile.HiDef;

                // load mods
                LoadMods();

                // initialise
                StardewGameInfo.SetValue(StardewProgramType, gamePtr);
                Log.AsyncY("Applying Final SDV Tweaks...");
                gamePtr.IsMouseVisible = false;
                gamePtr.Window.Title = "Stardew Valley - Version " + Game1.version;
            }
            catch (Exception ex)
            {
                Log.AsyncR("Game failed to initialise: " + ex);
                return;
            }

            // initialise after game launches
            new Thread(() =>
            {
                // Wait for the game to load up
                while (!ready) Thread.Sleep(1000);

                // Create definition to listen for input
                Log.AsyncY("Initializing Console Input Thread...");
                consoleInputThread = new Thread(ConsoleInputThread);

                // The only command in the API (at least it should be, for now)
                Command.RegisterCommand("help", "Lists all commands | 'help <cmd>' returns command description").CommandFired += help_CommandFired;

                // Subscribe to events
                ControlEvents.KeyPressed += Events_KeyPressed;
                GameEvents.LoadContent += Events_LoadContent;

                // Game's in memory now, send the event
                Log.AsyncY("Game Loaded");
                GameEvents.InvokeGameLoaded();

                // Listen for command line input
                Log.AsyncY("Type 'help' for help, or 'help <cmd>' for a command's usage");
                consoleInputThread.Start();
                while (ready)
                    Thread.Sleep(1000 / 10); // Check if the game is still running 10 times a second

                // Abort the thread, we're closing
                if (consoleInputThread != null && consoleInputThread.ThreadState == ThreadState.Running)
                    consoleInputThread.Abort();

                Log.AsyncY("Game Execution Finished");
                Log.AsyncY("Shutting Down...");
                Thread.Sleep(100);
                Environment.Exit(0);
            }).Start();

            // Start game loop
            Log.AsyncY("Starting SDV...");
            try
            {
                ready = true;
                gamePtr.Run();
            }
            catch (Exception ex)
            {
                ready = false;
                Log.AsyncR("Game failed to start: " + ex);
            }
        }

        /// <summary>Create a directory path if it doesn't exist.</summary>
        /// <param name="path">The directory path.</param>
        private static void VerifyPath(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                Log.AsyncR("Could not create a path: " + path + "\n\n" + ex);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static void LoadMods()
        {
            Log.AsyncY("LOADING MODS");
            foreach (string directory in Directory.GetDirectories(ModPath))
            {
                foreach (string manifestFile in Directory.GetFiles(directory, "manifest.json"))
                {
                    if (manifestFile.Contains("StardewInjector"))
                        continue;
                    Log.AsyncG("Found Manifest: " + manifestFile);
                    var manifest = new Manifest();
                    try
                    {
                        string t = File.ReadAllText(manifestFile);
                        if (string.IsNullOrEmpty(t))
                        {
                            Log.AsyncR($"Failed to read mod manifest '{manifestFile}'. Manifest is empty!");
                            continue;
                        }

                        manifest = manifest.InitializeConfig(manifestFile);

                        if (string.IsNullOrEmpty(manifest.EntryDll))
                        {
                            Log.AsyncR($"Failed to read mod manifest '{manifestFile}'. EntryDll is empty!");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.AsyncR($"Failed to read mod manifest '{manifestFile}'. Exception details:\n" + ex);
                        continue;
                    }
                    string targDir = Path.GetDirectoryName(manifestFile);
                    string psDir = Path.Combine(targDir, "psconfigs");
                    Log.AsyncY($"Created psconfigs directory @{psDir}");
                    try
                    {
                        if (manifest.PerSaveConfigs)
                        {
                            if (!Directory.Exists(psDir))
                            {
                                Directory.CreateDirectory(psDir);
                                Log.AsyncY($"Created psconfigs directory @{psDir}");
                            }

                            if (!Directory.Exists(psDir))
                            {
                                Log.AsyncR($"Failed to create psconfigs directory '{psDir}'. No exception occured.");
                                continue;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.AsyncR($"Failed to create psconfigs directory '{targDir}'. Exception details:\n" + ex);
                        continue;
                    }
                    string targDll = string.Empty;
                    try
                    {
                        targDll = Path.Combine(targDir, manifest.EntryDll);
                        if (!File.Exists(targDll))
                        {
                            Log.AsyncR($"Failed to load mod '{manifest.EntryDll}'. File {targDll} does not exist!");
                            continue;
                        }

                        Assembly modAssembly = Assembly.UnsafeLoadFrom(targDll);
                        if (modAssembly.DefinedTypes.Count(x => x.BaseType == typeof(Mod)) > 0)
                        {
                            Log.AsyncY("Loading Mod DLL...");
                            TypeInfo tar = modAssembly.DefinedTypes.First(x => x.BaseType == typeof(Mod));
                            Mod modEntry = (Mod)modAssembly.CreateInstance(tar.ToString());
                            if (modEntry != null)
                            {
                                modEntry.PathOnDisk = targDir;
                                modEntry.Manifest = manifest;
                                Log.AsyncG($"LOADED MOD: {modEntry.Manifest.Name} by {modEntry.Manifest.Authour} - Version {modEntry.Manifest.Version} | Description: {modEntry.Manifest.Description} (@ {targDll})");
                                Constants.ModsLoaded += 1;
                                modEntry.Entry();
                            }
                        }
                        else
                            Log.AsyncR("Invalid Mod DLL");
                    }
                    catch (Exception ex)
                    {
                        Log.AsyncR($"Failed to load mod '{targDll}'. Exception details:\n" + ex);
                    }
                }
            }

            Log.AsyncG($"LOADED {Constants.ModsLoaded} MODS");
            Console.Title = Constants.ConsoleTitle;
        }

        public static void ConsoleInputThread()
        {
            while (true)
            {
                Command.CallCommand(Console.ReadLine());
            }
        }

        private static void Events_LoadContent(object o, EventArgs e)
        {
            Log.AsyncY("Initializing Debug Assets...");
            DebugPixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            DebugPixel.SetData(new[] { Color.White });
        }

        private static void Events_KeyPressed(object o, EventArgsKeyPressed e)
        {
        }

        private static void help_CommandFired(object o, EventArgsCommand e)
        {
            if (e.Command.CalledArgs.Length > 0)
            {
                var fnd = Command.FindCommand(e.Command.CalledArgs[0]);
                if (fnd == null)
                    Log.AsyncR("The command specified could not be found");
                else
                {
                    Log.AsyncY(fnd.CommandArgs.Length > 0 ? $"{fnd.CommandName}: {fnd.CommandDesc} - {fnd.CommandArgs.ToSingular()}" : $"{fnd.CommandName}: {fnd.CommandDesc}");
                }
            }
            else
                Log.AsyncY("Commands: " + Command.RegisteredCommands.Select(x => x.CommandName).ToSingular());
        }
    }
}
