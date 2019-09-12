using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CmakeShell {
    
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.Directory)]
    public class CmakeShell : SharpContextMenu {
        Dictionary<string, string> generators = new Dictionary<string, string> {
            ["Visual Studio 15 2017"] = "vs15",
            ["Visual Studio 14 2015"] = "vs14",
            ["Visual Studio 12 2013"] = "vs12",
            ["Visual Studio 11 2012"] = "vs11",
            ["Visual Studio 10 2010"] = "vs10",
            ["Visual Studio 9 2008"] = "vs9",
            ["Visual Studio 8 2005"] = "vs8",
            ["Visual Studio 7 .NET 2003"] = "vs7",
            ["Borland Makefiles"] = "borland",
            ["NMake Makefiles"] = "nmake",
            ["NMake Makefiles JOM"] = "nmake-jom",
            ["Green Hills MULTI"] = "greenhills",
            ["MSYS Makefiles"] = "msys",
            ["MinGW Makefiles"] = "mingw",
            ["Unix Makefiles"] = "unix",
            ["Ninja"] = "ninjq",
            ["Watcom WMake"] = "watcom",
            ["CodeBlocks - MinGW Makefiles"] = "cblocks-mingw",
            ["CodeBlocks - NMake Makefiles"] = "cblocks-nmake",
            ["CodeBlocks - NMake Makefiles JOM"] = "cblocks-nmake-jom",
            ["CodeBlocks - Ninja"] = "cblocks-ninja",
            ["CodeBlocks - Unix Makefiles"] = "cblocks-unix",
            ["CodeLite - MinGW Makefiles"] = "clite-mingw",
            ["CodeLite - NMake Makefiles"] = "clite-nmake",
            ["CodeLite - Ninja"] = "clite-ninja",
            ["CodeLite - Unix Makefiles"] = "clite-unix",
            ["Sublime Text 2 - MinGW Makefiles"] = "subl-mingw",
            ["Sublime Text 2 - NMake Makefiles"] = "subl-nmake",
            ["Sublime Text 2 - Ninja"] = "subl-ninja",
            ["Sublime Text 2 - Unix Makefiles"] = "subl-unix",
            ["Kate - MinGW Makefiles"] = "kate-mingw",
            ["Kate - NMake Makefiles"] = "kate-nmake",
            ["Kate - Ninja"] = "kate-ninja",
            ["Kate - Unix Makefiles"] = "kate-unix",
            ["Eclipse CDT4 - NMake Makefiles"] = "eclipse-nmake",
            ["Eclipse CDT4 - MinGW Makefiles"] = "eclipse-mingw",
            ["Eclipse CDT4 - Ninja"] = "eclipse-ninja",
            ["Eclipse CDT4 - Unix Makefiles"] = "eclipse-unix"
        };

        Dictionary<string, string> sourceFileExtentions = new Dictionary<string, string> {
            [".cpp"] = "c++",
            [".cxx"] = "c++",
            [".cpp"] = "c++",
            [".c++"] = "c++",
            [".cc"] = "c++",
            [".c"] = "c",
        };

        Dictionary<string, string> headerFileExtentions = new Dictionary<string, string> {
            [".hpp"] = "c++",
            [".h"] = "all"
        };

        private string GetCmakeVersion() {
            var cmakeVersionProcess = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = "cmake",
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            cmakeVersionProcess.Start();
            Regex rx = new Regex("cmake version ([0-9\\.]+)");
            while (!cmakeVersionProcess.StandardOutput.EndOfStream) {
                var match = rx.Match(cmakeVersionProcess.StandardOutput.ReadLine());
                if (match.Success) {
                    return match.Groups[1].Value;
                   
                }
            }
            return "3.1";
        }


        protected override bool CanShowMenu() {
            if (SelectedItemPaths.Count() != 1) {
                return false;
            }
            string path = SelectedItemPaths.First();
            return File.Exists(path + "\\CMakeLists.txt") 
                || File.Exists(path + "\\CMakeCache.txt")
                || Control.ModifierKeys == Keys.Shift;
        }

        protected override ContextMenuStrip CreateMenu() {
            var menu = new ContextMenuStrip();
            string path = SelectedItemPaths.First();

            var cmakeMenu = new ToolStripMenuItem("CMake") {
                Image = res.icon
            };

            bool listsFound = File.Exists(path + "\\CMakeLists.txt");
            bool cacheFound = File.Exists(path + "\\CMakeCache.txt");

            if (listsFound | cacheFound) {
                var runGui = new ToolStripMenuItem("Launch CMake GUI") {
                    Image = res.icon
                };
                runGui.Click += (sender, args) => {
                    var si = new ProcessStartInfo {
                        FileName = "cmake-gui",
                        Arguments = "\"" + path + "\""
                    };
                    Process.Start(si);
                };
                cmakeMenu.DropDownItems.Add(runGui);
            }

            if (cacheFound) {
                var cmakeBuild = new ToolStripMenuItem("Build") {
                    Image = res.icon
                };
                cmakeBuild.Click += (sender, args) => {
                    var si = new ProcessStartInfo {
                        FileName = "cmd",
                        Arguments = "/C cmake --build . & pause",
                        WorkingDirectory = path
                    };
                    Process.Start(si);
                };
                cmakeMenu.DropDownItems.Add(cmakeBuild);

                var cmakeRefresh = new ToolStripMenuItem("Refresh") {
                    Image = res.icon
                };
                cmakeRefresh.Click += (sender, args) => {
                    var si = new ProcessStartInfo {
                        FileName = "cmd",
                        Arguments = "/C cmake . & pause",
                        WorkingDirectory = path
                    };
                    Process.Start(si);
                };
                cmakeMenu.DropDownItems.Add(cmakeRefresh);

                var cmakeBuildType = new ToolStripMenuItem("Switch build type") {
                    Image = res.icon
                };
                cmakeMenu.DropDownItems.Add(cmakeBuildType);

                var cmakeSetDebug = new ToolStripMenuItem("Debug") {
                    Image = res.icon
                };
                cmakeSetDebug.Click += (sender, args) => {
                    var si = new ProcessStartInfo {
                        FileName = "cmd",
                        Arguments = "/C cmake -DCMAKE_BUILD_TYPE=Debug . & pause",
                        WorkingDirectory = path
                    };
                    Process.Start(si);
                };
                cmakeBuildType.DropDownItems.Add(cmakeSetDebug);

                var cmakeSetRelease = new ToolStripMenuItem("Release") {
                    Image = res.icon
                };
                cmakeSetRelease.Click += (sender, args) => {
                    var si = new ProcessStartInfo {
                        FileName = "cmd",
                        Arguments = "/C cmake -DCMAKE_BUILD_TYPE=Release . & pause",
                        WorkingDirectory = path
                    };
                    Process.Start(si);
                };
                cmakeBuildType.DropDownItems.Add(cmakeSetRelease);
            }

            if (listsFound) {
                var cmakeGenerate = new ToolStripMenuItem("Generate") {
                    Image = res.icon
                };
                                
                foreach(var generator in generators) {
                    var g = new ToolStripMenuItem(generator.Key) {
                        Image = res.icon
                    };
                    g.Click += (sender, args) => {
                        string targetPath = path + "-cmk-" + generator.Value;
                        Directory.CreateDirectory(targetPath);
                        var si = new ProcessStartInfo {
                            FileName = "cmd",
                            Arguments = String.Format("/C cmake -DCMAKE_INSTALL_PREFIX=\"./install\" -G \"{0}\" \"{1}\" & pause", generator.Key, path),
                            WorkingDirectory = targetPath
                        };
                        Process.Start(si);
                    };
                    cmakeGenerate.DropDownItems.Add(g);
                }                
                cmakeMenu.DropDownItems.Add(cmakeGenerate);
            }

            if(Control.ModifierKeys == Keys.Shift && !cacheFound && !listsFound) {
                var createProject = new ToolStripMenuItem("Create project") {
                    Image = res.icon
                };
                createProject.Click += (sender, args) => {
                    int cCount = 0;
                    int cppCount = 0;
                    string sourceFilesStr = "";
                    string project = Path.GetFileName(path).Replace(" ", "_");
                    string cmakeVersion = GetCmakeVersion();

                    foreach (string filePath in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)) {                        
                        FileInfo fi = new FileInfo(filePath);                            
                        string relative = filePath.Remove(0, path.Length + 1).Replace("\\", "/");
                        if (sourceFileExtentions.ContainsKey(fi.Extension)) {
                            string language = sourceFileExtentions[fi.Extension];
                            if (language == "c++") {
                                cppCount++;
                            }
                            if (language == "c") {
                                cCount++;
                            }
                            if(relative.Contains(" ")) {
                                relative = "\"" + relative + "\"";
                            }
                            sourceFilesStr += "    " + relative + "\n";
                        }   
                    }

                    string result = "cmake_minimum_required(VERSION " + cmakeVersion + ")\n\n";
                    result += "project(" + project + ")\n\n";//todo
                    if (cppCount > cCount) {
                        result += "set(CMAKE_CXX_STANDARD 11)\n\n";
                    }
                    result += "add_executable(" + project + "\n";
                    result += sourceFilesStr;
                    result += ")\n";

                    string listsPath = path + "\\CMakeLists.txt";
                    MessageBox.Show(listsPath + " created");
                    File.WriteAllText(listsPath, result);
                };
                cmakeMenu.DropDownItems.Add(createProject);
            }

            menu.Items.Add(cmakeMenu);
            return menu;
        }
    }
}
