﻿/*
Copyright (C) 2020 DeathCradle

This file is part of Open Terraria API v3 (OTAPI)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
*/
using Mono.Cecil;
using MonoMod;
using MonoMod.RuntimeDetour.HookGen;
using OTAPI.Common;
using System;
using System.IO;
using System.Linq;

namespace OTAPI.Setup
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = Remote.DownloadServer();

            Console.WriteLine($"[OTAPI] Extracting embedded binaries and packing into one binary...");

            // allow for refs to the embedded resources, such as ReLogic.dll
            var extractor = new ResourceExtractor();
            var embeddedResourcesDir = extractor.Extract(input);
            var inputName = Path.GetFileNameWithoutExtension(input);

            var output = $"MMHOOK_{inputName}.dll";
            using (MonoModder mm = new MonoModder()
            {
                InputPath = input,
                OutputPath = output,
                ReadingMode = ReadingMode.Deferred,
                MissingDependencyThrow = false,
                PublicEverything = true, // we want all of terraria exposed

                GACPaths = new string[] { } // avoid MonoMod looking up the GAC, which causes an exception on .netcore
            })
            {
                (mm.AssemblyResolver as DefaultAssemblyResolver).AddSearchDirectory(embeddedResourcesDir);
                mm.Read();

                // prechange the assembly name to a dll
                // monomod will also reference this when relinking so it must be correct
                // in order for shims within this dll to work (relogic)
                mm.Module.Name = Path.ChangeExtension(mm.Module.Name, ".dll");

                foreach (var path in new[] {
                    Path.Combine(System.Environment.CurrentDirectory, "TerrariaServer.OTAPI.Shims.mm.dll"),
                    Directory.GetFiles(embeddedResourcesDir).Single(x => Path.GetFileName(x).Equals("ReLogic.dll", StringComparison.CurrentCultureIgnoreCase)),
                })
                {
                    mm.Log($"[MonoMod] Reading mod or directory: {path}");
                    mm.ReadMod(path);
                }

                mm.MapDependencies();
                mm.AutoPatch();

                if (File.Exists(output))
                {
                    mm.Log($"[HookGen] Clearing {output}");
                    File.Delete(output);
                }

                mm.Log("[HookGen] Starting HookGenerator");
                var gen = new HookGenerator(mm, Path.GetFileName(output));
                using (ModuleDefinition mOut = gen.OutputModule)
                {
                    gen.Generate();

                    mOut.Write(output);
                }

                mm.OutputPath = mm.Module.Name; // the merged TerrariaServer + ReLogic (so we can apply patches)

                // switch to any cpu so that we can compile and use types in mods
                // this is usually in a modification otherwise
                mm.Module.Architecture = TargetArchitecture.I386;
                mm.Module.Attributes = ModuleAttributes.ILOnly;

                mm.Write();

                mm.Log("[HookGen] Done.");
            }
        }
    }
}
