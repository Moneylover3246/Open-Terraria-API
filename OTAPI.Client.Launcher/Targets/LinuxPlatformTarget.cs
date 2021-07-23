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
using OTAPI.Common;
using System;
using System.IO;

namespace OTAPI.Client.Launcher.Targets
{
    public class LinuxPlatformTarget : LinuxInstallDiscoverer, IPlatformTarget
    {
        public void OnUILoad(MainWindowViewModel vm)
        {
            vm.OtapiExe = Path.Combine(Environment.CurrentDirectory, "OTAPI.exe");
            vm.VanillaExe = Directory.Exists(vm.InstallPath.Path) ? Path.Combine(vm.InstallPath.Path, "Terraria.exe") : null;
        }

        public void Install(string installPath)
        {
            //var packagePaths = this.PublishHostGame();

            //if (packagePaths.Any())
            //{
            //    var otapiFolder = Path.Combine(installPath, "otapi");
            var sourceContentPath = Path.Combine(installPath, "Content");
            var destContentPath = Path.Combine(Environment.CurrentDirectory, "Content");

            //    if (!Directory.Exists(otapiFolder))
            //        Directory.CreateDirectory(otapiFolder);

            Console.WriteLine(Status = "Installing FNA libs...");
            this.InstallLibs(Environment.CurrentDirectory);

            //    Console.WriteLine(Status = "Installing LUA...");
            //    this.InstallLua(otapiFolder);

            //    Console.WriteLine(Status = "Installing ClearScript...");
            //    this.InstallClearScript(otapiFolder);

            //    Console.WriteLine(Status = "Installing extra files...");
            //    this.CopyInstallFiles(otapiFolder);

            Console.WriteLine(Status = "Installing Steamworks...");
            this.InstallSteamworks64(Environment.CurrentDirectory, installPath);

            Console.WriteLine(Status = "Copying Terraria Content files, this may take a while...");
            this.CopyFiles(sourceContentPath, destContentPath);

            //    Console.WriteLine(Status = "Patching launch scripts...");
            //    this.PatchLinuxLaunch(installPath);

            //    this.CopyOTAPI(otapiFolder, packagePaths);

            //    Console.WriteLine(Status = "Linux install finished");
            //}
            //else
            //{
            //    Console.Error.WriteLine(Status = "Failed to produce or find the appropriate package");
            //}
        }
    }
}