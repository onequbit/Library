using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Library
{

    

    public static partial class Lib
    {
        public static string GetPseudoCmdOutput(string command)
        {
            string output;
            using (Process pseudoCall = new Process())
            {
                pseudoCall.StartInfo = new ProcessStartInfo()
                {
                    FileName = "pseudo.exe",
                    Arguments = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                    // RedirectStandardError = true
                };
                pseudoCall.Start();
                // CmdResult result = new CmdResult(pseudoCall);
                output = pseudoCall.StandardOutput.ReadToEnd();
                pseudoCall.WaitForExit();
                if (pseudoCall.ExitCode != 0) output = "";                
            }            
            return output;
        }

        public static string GetCmdOutput(string command)
        {
            string output;            
            using (Process cmdCall = new Process())
            {
                cmdCall.StartInfo = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    // RedirectStandardError = true,
                    CreateNoWindow = true
                };
                // return cmdCall.GetCommandResult();
                cmdCall.Start();
                output = cmdCall.StandardOutput.ReadToEnd();
                cmdCall.WaitForExit();                
            }
            return output;         
        }

        public static string[] CmdWhere(string dir, string filename)
        {
            List<string> paths = new List<string>();
            string searchCmd = $"where /r {dir} {filename}";            
            string[] whereOutput = Lib.GetCmdOutput(searchCmd).Split('\n');                        
            foreach(string path in whereOutput)
            {
                if (path.Trim().Length > 3)
                {                                        
                    paths.Add(path.Trim());
                }               
            }            
            return paths.ToArray();
        }

        public static string[] GetEnvPaths()
        {
            string[] envPaths = Lib.GetCmdOutput("echo %path%").Split(';');
            List<string> paths = new List<string>();            
            foreach(string path in envPaths)
            {   
                if (path.Trim().Length > 3)
                {                    
                    paths.Add(path);
                }                    
            }
            string thisDir = Environment.CurrentDirectory;
            string parentDir = System.IO.Directory.GetParent(thisDir).FullName;
            paths.Add(parentDir);
            paths.Add(thisDir);
            envPaths = paths.ToArray();
            Array.Reverse(envPaths);
            return envPaths;             
        }

        public static string SearchEnvPathsFor(string filename)
        {   
            string[] paths = Lib.GetEnvPaths();            
            foreach(string path in paths)
            {
                try
                {                    
                    string[] found = Lib.CmdWhere(path, filename);
                    if (found.Length > 0)                    
                    { 
                        string location = found[0].Replace(path, @".\");                   
                        return $"{path} -> {location}";
                    }                                        
                }                
                catch {}
            }            
            return "";
        }
    }

    public class IfDebug
    {
        public IfDebug(Action debugAction)
        {
            #if DEBUG
            debugAction();
            #endif
        }

        public IfDebug(object something)
        {
            #if DEBUG   
            new IfDebugMsg(something.ToString());
            #endif
        }
    }

    public class IfDebugMsg
    {
        public IfDebugMsg(string message)
        {
            #if DEBUG            
            DialogResult result = MessageBox.Show(message, "DEBUG", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            if (result == DialogResult.Cancel)
            {
                Environment.Exit(-1);
            }
            #endif
        }

        public IfDebugMsg(string[] messageArray)
        {
            #if DEBUG
            List<string> messageBlock = new List<string>{};
            string newMessage = "";
            foreach(string message in messageArray)
            {                    
                messageBlock.Add(message);
                if (messageBlock.Count >= 25)
                {
                    newMessage = messageBlock.ToArray().Join("\n");
                    new IfDebugMsg(newMessage);                    
                    messageBlock.Clear();
                }
            }
            newMessage = messageBlock.ToArray().Join("\n");
            new IfDebugMsg(newMessage);
            #endif
        }
    }

}