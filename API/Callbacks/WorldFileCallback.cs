﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OTA.Logging;
using OTA.Plugin;
using OTA.Command;

#if Full_API
using Terraria;
using Terraria.IO;
using Terraria.Utilities;
using Terraria.Social;
#endif

namespace OTA.Callbacks
{
    public static class WorldFileCallback
    {
        private static string _savePath;
        public static readonly object SavePathLock = new object();

        public static string SavePath
        {
            #if Full_API
            get { return _savePath ?? Terraria.Main.worldPathName; }
            #else
            get { return null; }
            #endif
            set
            { _savePath = value; }
        }

        public static bool OnAutoSave()
        {
            var args = new HookArgs.WorldAutoSave();
            var ctx = HookContext.Empty;

            HookPoints.WorldAutoSave.Invoke(ref ctx, ref args);

            return ctx.Result == HookResult.DEFAULT;
        }

        /// <summary>
        /// Clears the world, and is a direct replacement of Terraria.WorldGen.clearWorld.
        /// </summary>
        public static void ClearWorld()
        {
//            Terraria.Main.tile = new OTA.Memory.TileCollection(Terraria.Main.maxTilesX, Terraria.Main.maxTilesY); //TODO when generating a small Terraria doesn't obey the size (that's why it's always large i guess. *siiiiiigh*)
            MainCallback.ResetTileArray();
            #if Full_API
            WorldGen.clearWorld();
            #endif
            GC.Collect();
        }

        //        public static void loadWorld(bool loadFromCloud)
        //        {
        //#if Full_API
        //            MainCallback.WorldLoadBegin();
        //            WorldFile.IsWorldOnCloud = loadFromCloud;
        //            Main.checkXMas();
        //            Main.checkHalloween();
        //            bool flag = loadFromCloud && SocialAPI.Cloud != null;
        //            if (!FileUtilities.Exists(Main.worldPathName, flag) && Main.autoGen)
        //            {
        //                if (!flag)
        //                {
        //                    for (int i = Main.worldPathName.Length - 1; i >= 0; i--)
        //                    {
        //                        if (Main.worldPathName.Substring(i, 1) == string.Concat(Path.DirectorySeparatorChar))
        //                        {
        //                            string path = Main.worldPathName.Substring(0, i);
        //                            Directory.CreateDirectory(path);
        //                            break;
        //                        }
        //                    }
        //                }
        //                WorldFileCallback.ClearWorld();
        //                WorldGen.generateWorld(-1, Main.AutogenProgress);
        //                WorldFile.saveWorld();
        //            }
        //            if (WorldGen.genRand == null)
        //            {
        //                WorldGen.genRand = new Random((int)DateTime.Now.Ticks);
        //            }
        //            byte[] buffer = FileUtilities.ReadAllBytes(Main.worldPathName, flag);
        //            using (MemoryStream memoryStream = new MemoryStream(buffer))
        //            {
        //                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
        //                {
        //                    try
        //                    {
        //                        WorldGen.loadFailed = false;
        //                        WorldGen.loadSuccess = false;
        //                        int num = binaryReader.ReadInt32();
        //                        WorldFile.versionNumber = num;
        //                        int num2;
        //                        if (num <= 87)
        //                        {
        //                            num2 = WorldFile.LoadWorld_Version1(binaryReader);
        //                        }
        //                        else
        //                        {
        //                            num2 = WorldFile.LoadWorld_Version2(binaryReader);
        //                        }
        //                        if (num < 141)
        //                        {
        //                            if (!loadFromCloud)
        //                            {
        //                                Main.ActiveWorldFileData.CreationTime = File.GetCreationTime(Main.worldPathName);
        //                            }
        //                            else
        //                            {
        //                                Main.ActiveWorldFileData.CreationTime = DateTime.Now;
        //                            }
        //                        }
        //                        binaryReader.Close();
        //                        memoryStream.Close();
        //                        if (num2 != 0)
        //                        {
        //                            WorldGen.loadFailed = true;
        //                        }
        //                        else
        //                        {
        //                            WorldGen.loadSuccess = true;
        //                        }
        //                        if (WorldGen.loadFailed || !WorldGen.loadSuccess)
        //                        {
        //                            return;
        //                        }
        //                        WorldGen.gen = true;
        //                        WorldGen.waterLine = Main.maxTilesY;
        //                        Liquid.QuickWater(2, -1, -1);
        //                        WorldGen.WaterCheck();
        //                        int num3 = 0;
        //                        Liquid.quickSettle = true;
        //                        int num4 = Liquid.numLiquid + LiquidBuffer.numLiquidBuffer;
        //                        float num5 = 0;
        //                        while (Liquid.numLiquid > 0 && num3 < 100000)
        //                        {
        //                            num3++;
        //                            float num6 = (float)(num4 - (Liquid.numLiquid + LiquidBuffer.numLiquidBuffer)) / (float)num4;
        //                            if (Liquid.numLiquid + LiquidBuffer.numLiquidBuffer > num4)
        //                            {
        //                                num4 = Liquid.numLiquid + LiquidBuffer.numLiquidBuffer;
        //                            }
        //                            if (num6 > num5)
        //                            {
        //                                num5 = num6;
        //                            }
        //                            else
        //                            {
        //                                num6 = num5;
        //                            }
        //                            Main.statusText = string.Concat(new object[]
        //                                {
        //                                    Lang.gen[27],
        //                                    " ",
        //                                    (int)(num6 * 100 / 2 + 50),
        //                                    "%"
        //                                });
        //                            Liquid.UpdateLiquid();
        //                        }
        //                        Liquid.quickSettle = false;
        //                        Main.weatherCounter = WorldGen.genRand.Next(3600, 18000);
        //                        Cloud.resetClouds();
        //                        WorldGen.WaterCheck();
        //                        WorldGen.gen = false;
        //                        NPC.setFireFlyChance();
        //                        MainCallback.WorldLoadEnd();
        //                        Main.InitLifeBytes();
        //                        if (Main.slimeRainTime > 0)
        //                        {
        //                            Main.StartSlimeRain(false);
        //                        }
        //                        NPC.setWorldMonsters();
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        ProgramLog.Log(e);
        //                        WorldGen.loadFailed = true;
        //                        WorldGen.loadSuccess = false;
        //                        try
        //                        {
        //                            binaryReader.Close();
        //                            memoryStream.Close();
        //                        }
        //                        catch
        //                        {
        //                        }
        //                        return;
        //                    }
        //                }
        //            }
        //            //            if (WorldFile.OnWorldLoad != null)
        //            //            {
        //            //                WorldFile.OnWorldLoad.Invoke();
        //            //            }
        //#endif
        //        }
    }
}
