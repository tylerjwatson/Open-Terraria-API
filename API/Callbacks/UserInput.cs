﻿using System;
using OTA.Command;
using OTA.Plugin;
using OTA.Misc;
using OTA.Logging;

namespace OTA.Callbacks
{
    /// <summary>
    /// Callbacks from vanilla code for miscellaneous patches
    /// </summary>
    public static class Patches
    {
        /// <summary>
        /// Used in vanilla code where there was fixed windows paths
        /// </summary>
        /// <returns>The current directory.</returns>
        public static string GetCurrentDirectory()
        {
            return Environment.CurrentDirectory;
        }
    }

    /// <summary>
    /// Input specific callbacks from Terraria.Main
    /// </summary>
    public static class UserInput
    {
        //        static readonly List<String> _officialCommands = new List<String>()
        //        {
        //            "help",
        //            "playing",
        //            "clear",
        //            "exit",
        //            "exit-nosave",
        //            "save",
        //            "kick",
        //            "ban",
        //            "password",
        //            "version",
        //            "time",
        //            "port",
        //            "maxplayers",
        //            "say",
        //            "motd",
        //            "dawn",
        //            "noon",
        //            "dusk",
        //            "midnight",
        //            "settle"
        //        };


        /// <summary>
        /// The request from vanilla code to start listening for commands
        /// </summary>
        public static void ListenForCommands()
        {
            var ctx = new HookContext()
            {
                Sender = HookContext.ConsoleSender
            };
            var args = new HookArgs.StartCommandProcessing();
            HookPoints.StartCommandProcessing.Invoke(ref ctx, ref args);

            if (ctx.Result == HookResult.DEFAULT)
            {
                //This might change to the vanilla code (allowing for pure vanilla functionalities). but we'll see.
                System.Threading.ThreadPool.QueueUserWorkItem(ListenForCommands); 
            }
        }

        static readonly CommandParser _cmdParser = new CommandParser();

        /// <summary>
        /// The active server instance of the command parser
        /// </summary>
        /// <value>The command parser.</value>
        public static CommandParser CommandParser
        {
            get
            { return _cmdParser; }
        }

        /// <summary>
        /// The call to start OTA command
        /// </summary>
        /// <param name="threadContext">Thread context.</param>
        private static void ListenForCommands(object threadContext)
        {
            System.Threading.Thread.CurrentThread.Name = "APC";

            ProgramLog.Console.Print("Ready for commands.");
#if Full_API
            while (!Terraria.Netplay.disconnect)
            {
                try
                {
                    Console.Write(": ");
                    var input = Console.ReadLine();
                    _cmdParser.ParseConsoleCommand(input);
                }
                catch (ExitException e)
                {
                    ProgramLog.Log(e.Message);
                    break;
                }
                catch (Exception e)
                {
                    ProgramLog.Log(e, "Exception from command");
                }
            }
#endif
        }

        //public static int X = 0;
        //public static Tile[,] Tiles;

        public static string ProcessInput(string value)
        {
            return value;
            //            var t = String.Empty;
            //            //int x = 0, y = 0;
            //            //if (Tiles[x, y] == DefaultTile)
            //            //{
            //            //    Tiles[x, y] = DefaultTile;
            //            //}
            //
            //            //Main.tile[0, 0] = default(Tile);
            //            //if (Main.tile[0, 0] == DefaultTile)
            //            //{
            //            //    return "";
            //            //}
            //
            //            //if (Main.netMode == 1 || X > Main.maxTilesX || X > Main.maxTilesY)
            //            //{
            //            //    for (int k = 0; k < X; k++)
            //            //    {
            //            //        float num2 = (float)k / (float)X;
            //            //        for (int l = 0; l < X; l++)
            //            //        {
            //            //            Main.tile[k, l] = default(Tile);
            //            //        }
            //            //    }
            //            //}
            //
            //            if (!String.IsNullOrEmpty(value))
            //            {
            //                var command = value.Split(' ').First().ToLower();
            //                if (_officialCommands.Contains(command) && command != "help") return value;
            //
            //                if (command == "spawnnpc")
            //                {
            //
            //                }
            //                else if (command == "test")
            //                {
            //                    //Tools.WriteLine("Active player count: " + Terraria.Main.player.Where(x => x != null && x.active).Count());
            //                }
            //                else if (command == "help")
            //                {
            //                    Tools.WriteLine("spawnnpc " + '\t' + " Spawn an npc by name or id");
            //                    return command; //Allow printout of defaults
            //                }
            //                else
            //                {
            //                    Tools.WriteLine("Server command does not exist.");
            //                }
            //            }
            //            return t;
        }

        #if Full_API
        public static readonly Terraria.Tile DefaultTile = default(Terraria.Tile);

        public static Terraria.Tile GetTile()
        {
            return DefaultTile;
        }

        public static bool Tile_Equality(Terraria.Tile t1, Terraria.Tile t2)
        {
            return t1.isTheSameAs(t2);
        }

        public static bool Tile_Inequality(Terraria.Tile t1, Terraria.Tile t2)
        {
            return !t1.isTheSameAs(t2);
        }
        #endif

        //public static bool TileEquals2(TileData t1, TileData t2)
        //{
        //    for (var x = 0; x < 1; x++)
        //    {
        //        for (var y = 0; y < 1; y++)
        //        {
        //            TestAA[x, y] = new TestA();
        //        }
        //    }
        //    for (var x = 0; x < 1; x++)
        //    {
        //        for (var y = 0; y < 1; y++)
        //        {
        //            TestBB[x, y] = new TestB();
        //        }
        //    }

        //    return false;
        //}

        //class TestA {}
        //struct TestB {}

        //static TestA[,] TestAA;
        //static TestB[,] TestBB;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    public struct TileData
    {
        //public static bool operator !=(TileData t1, TileData t2)
        //{

        //    return false;
        //}

        //public static bool operator ==(TileData t1, TileData t2)
        //{
        //    return UserInput.TileEquals2(t1, t2);
        //}

        //public override bool Equals(object obj)
        //{
        //    return base.Equals(obj);
        //}

        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}
    }
}
