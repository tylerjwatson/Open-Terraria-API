﻿

using System;
using System.Reflection;

namespace OTA.Patcher
{
    [Serializable]
    /// <summary>
    /// AppDomain proxy for accessing OTA
    /// </summary>
    public class Proxy : MarshalByRefObject
    {
        Assembly _api;

//        /// <summary>
//        /// Gets the build for OTA.
//        /// </summary>
//        /// <value>The build.</value>
//        public int Build
//        {
//            get
//            {
//                return (int)_api.GetType("OTA.Globals").GetField("Build").GetValue(null);
//
//            }
//        }

        /// <summary>
        /// Get the supported Terraria version
        /// </summary>
        /// <value>The terraria version.</value>
        public string TerrariaVersion
        {
            get
            {
                return (string)_api.GetType("OTA.Globals").GetField("TerrariaVersion").GetValue(null);
            }
        }


//        public string LibrariesPath
//        {
//            get
//            {
//                return (string)_api.GetType("OTA.Globals").GetProperty("LibrariesPath").GetValue(null, null);
//            }
//        }
//
//        public string PluginPath
//        {
//            get
//            {
//                return (string)_api.GetType("OTA.Globals").GetProperty("PluginPath").GetValue(null, null);
//            }
//        }
//
//        public bool IsPatching
//        {
//            get
//            {
//                return (bool)_api.GetType("OTA.Globals").GetProperty("IsPatching").GetValue(null, null);
//            }
//            set
//            {
//                _api.GetType("OTA.Globals").GetProperty("IsPatching").SetValue(null, value, null);
//            }
//        }
//
//        public void Initialise()
//        {
//            IsPatching = true;
//
//            _api.GetType("OTA.Globals").GetMethod("Touch").Invoke(null, null);
//
//            var pm = _api.GetType("OTA.PluginManager");
//
//            pm.GetMethod("SetHookSource").Invoke(null, new object[] { _api.GetType("TDSM.API.Plugin.HookPoints") });
//            pm.GetMethod("Initialize").Invoke(null, new object[] { PluginPath, LibrariesPath });
//            pm.GetMethod("LoadPlugins").Invoke(null, null);
//        }
//
//        public void InvokeEvent(byte[] terraria, bool isServer)
//        {
//            var hct = _api.GetType("OTA.Plugin.HookContext");
//            var hap = _api.GetType("OTA.Plugin.HookArgs").GetNestedType("PatchServer");
//
//            var ctx = Activator.CreateInstance(hct);
//            var args = Activator.CreateInstance(hap);
//
//            hap.GetProperty("Terraria").SetValue(args, terraria, null);
//            hap.GetProperty("IsServer").SetValue(args, isServer, null);
//            hap.GetProperty("IsClient").SetValue(args, !isServer, null);
//
//            var pst = _api.GetType("OTA.Plugin.HookPoints")
//                .GetField("PatchServer");
//            var pse = pst.GetValue(null);
//
//            var arguments = new object[] { ctx, args };
//            pst.FieldType.GetMethod("Invoke").Invoke(pse, arguments);
//
//            //return hap.GetProperty("Terraria").GetValue(arguments[1], null) as byte[];
//        }

        /// <summary>
        /// Load an assembly into the domain
        /// </summary>
        /// <param name="path">Path.</param>
        public void Load(string path)
        {
            _api = Assembly.LoadFile(path);
        }
    }

    /// <summary>
    /// This class is to isolate and manage the tdsm.exe referenced by the api dll.
    /// Previously windows would lock the tdsm.exe that was referenced by TDSM.API.dll, which itself was loaded by the patcher.
    /// The locking would cause the patcher (if ran a second time) to fail when saving tdsm.exe
    /// </summary>

    public static class APIWrapper
    {
        //static Assembly _api;
        static Proxy _api;
        static AppDomain _domain;

        static APIWrapper()
        {
            //_domain = AppDomain.CreateDomain("TDSM_API_WRAPPER", AppDomain.CurrentDomain.Evidence, new AppDomainSetup()
            //{
            //    //ShadowCopyFiles = "false",
            //    ApplicationBase = Environment.CurrentDirectory
            //});
            _domain = AppDomain.CreateDomain("OPEN_TERRARIA_API_WRAPPER", null /*AppDomain.CurrentDomain.Evidence*/, new AppDomainSetup()
                {
                    //ShadowCopyFiles = "false",
                    ApplicationBase = Environment.CurrentDirectory/*, Commented out as OSX does not have this?
                AppDomainManagerAssembly = String.Empty*/
                });

            //Console.WriteLine("Domain: " + ((_domain == null) ? "null" : "not null"));

            //_domain.AssemblyResolve += (s, a) =>
            //{
            //    try
            //    {
            //        //return Assembly.LoadFrom(Path.Combine(Globals.PluginPath, a.Name + ".dll"));
            //    }
            //    catch { }
            //    return null;
            //};

            var type = typeof(Proxy);
            foreach (var file in new string[] { "Patcher.exe", "OTA.dll" })
            {
                if (!System.IO.File.Exists(file))
                {
                    var bin = System.IO.Path.Combine(Environment.CurrentDirectory, "bin", "x86", "Debug", file);
                    if (System.IO.File.Exists(bin))
                    {
                        System.IO.File.Copy(bin, file);
                        Console.WriteLine("Copied: " + file);
                    }
                }
            }
            var plugin = _domain.CreateInstance(type.Assembly.FullName, type.FullName);
//            var r = plugin.CreateObjRef(typeof(MarshalByRefObject));
            ////var p = r.GetRealObject(new System.Runtime.Serialization.StreamingContext( System.Runtime.Serialization.StreamingContextStates.CrossAppDomain));
            _api = plugin.Unwrap() as Proxy;

            _api.Load(System.IO.Path.Combine(Environment.CurrentDirectory, "OTA.dll"));

            //var has = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.GetName().Name == "TDSM.API").Count() > 0;
            //var asm = _domain.GetAssemblies();
        }

        //public static int Build
        //{
        //    get
        //    {
        //        return _api.Build;
        //    }
        //}

        /// <summary>
        /// Gets the supported Terraria version for OTA.
        /// </summary>
        /// <value>The terraria version.</value>
        public static string TerrariaVersion
        {
            get
            {
                return _api.TerrariaVersion;
            }
        }

        //public static string LibrariesPath
        //{
        //    get
        //    {
        //        return _api.LibrariesPath;
        //    }
        //}

        //public static string PluginPath
        //{
        //    get
        //    {
        //        return _api.PluginPath;
        //    }
        //}

        //public static bool IsPatching
        //{
        //    get
        //    {
        //        return _api.IsPatching;
        //    }
        //    set
        //    {
        //        _api.IsPatching = value;
        //    }
        //}

        //public static void Initialise()
        //{
        //    _api.Initialise();
        //}

        //public static byte[] InvokeEvent(byte[] terraria, bool isServer)
        //{
        //    _api.InvokeEvent(terraria, isServer);
        //    return terraria; // _api.InvokeEvent(terraria, isServer);
        //}

        //public static void Finish()
        //{
        //    _domain.DomainUnload += (a, b) =>
        //    {
        //        System.Diagnostics.Debug.Print("DOMAIN UNLOADED =============");
        //    };
        //    AppDomain.Unload(_domain);
        //    _domain = null;
        //}

        //public static bool IsDotNet()
        //{
        //    return true;
        //}
    }
}

