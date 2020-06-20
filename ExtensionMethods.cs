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

    public struct CmdResult
    {
        public string Output;
        public string[] OutputLines
        {
            get => Output.Split('\n');
        }
        public string[] OutputPaths
        {
            get => Output.Split(';');
        }
        public string Error;
        public int ExitCode;
        
        public CmdResult(string output, string error, int exitCode)
        {
            this.Output = output;
            this.Error = error;
            this.ExitCode = exitCode;            
        }

        public CmdResult(Process proc)
        {
            proc.Start();
            this.Output = proc.StandardOutput.ReadToEnd();
            this.Error = proc.StandardError.ReadToEnd();            
            proc.WaitForExit(); 
            this.ExitCode = proc.ExitCode;            
        } 
    }

    public static partial class ExtensionMethods
    {   
        public static string CurrentProgramName(this object program)
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        }

        private static Mutex mutex = null;

        public static void SingleInstance(this System.Windows.Forms.Form formApp)
        {
            bool createdNew;
            string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;    
            mutex = new Mutex(true, appName, out createdNew);    
            if (!createdNew)  
            {                  
                formApp.Close();
            }
        }

        public static Icon GetCurrentIcon(this System.Windows.Forms.Form formApp)
        {
            return Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        public static void Run(this System.Windows.Forms.Form app)
        {
            System.Windows.Forms.Application.Run(app);
        }        

        public static bool NullOrEmpty(this string s)
        {
            return s == null || s == "";
        }

        public static string Join(this IEnumerable<string> strCollection, string sepStr = null, char sepChar = '\n')
        {
            string separator = (sepStr == null)? $"{sepChar}" : sepStr;            
            return string.Join(separator, strCollection.ToArray());
        }

        public static string NoParens(this string str)
        {
            if (str.Length < 2) return str;            
            return str.Trim(new char[] { '"'});
        }
        
        public static bool Contains(this string[] strArray, string item)
        {
            foreach(string str in strArray)
            {   
                bool found = item.Equals(str) || item.NoParens().Equals(str);   
                if (found) return true;
            }
            return false;            
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (action == null) throw new ArgumentNullException("action");

            foreach (T item in source)
            {
                action(item);
            }
        }

        public static List<U> Map<T,U>(this IEnumerable<T> source, Func<T,U> mapFunction)
        {
            if (source == null) throw new ArgumentNullException("source");
            
            List<U> results = new List<U> ();

            source.ForEach( (item) => 
            {
                results.Add( (U)mapFunction(item) );
            });

            return results;            
        }

        public static U ReflectProperty<T,U>(this T obj, string propertyName)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            if (propertyName.NullOrEmpty()) throw new ArgumentException(propertyName);

            PropertyInfo prop = typeof(T).GetProperty(propertyName);
            if (prop == null) throw new TargetException(propertyName); 

            return (U)prop.GetValue(obj);
        }

        public static List<U> GetPropertyList<T,U>(this IEnumerable<T> source, string propertyName)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (propertyName.NullOrEmpty()) throw new ArgumentException(propertyName);
            
            return source.Map<T,U>( (item) => { return item.ReflectProperty<T,U>(propertyName); });            
        }

        public static List<Dictionary<string,object>> ToDictionaryList<T>(this IEnumerable<T> source, string[] properties)
        {
            if (source == null) throw new ArgumentNullException("source");            
                        
            return source.Map<T,Dictionary<string,object>>( (item) => 
            { 
                Dictionary<string, object> dict = new Dictionary<string,object> ();
                properties.ForEach( (name) => 
                {
                    dict[name] = item.ReflectProperty<T,object>(name);
                });
                return dict;
            });            
        }

        private static void showMessageWithOptions(string message, string title, bool exitOption = false)
        {
            if (Environment.UserInteractive)
            {
                Console.Error.WriteLine(message);
                return;                
            }

            if (exitOption)
            {
                DialogResult result = MessageBox.Show(message, title, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                if (result == DialogResult.Cancel)
                {
                    Environment.Exit(-1);
                }
            } else {
                MessageBox.Show(message, title);                
            }                  
        }

        public static void ToMessageBox(this Exception ex, string name = "", bool enableExit = true)
        {                       
            string title = ex.InnerException.ToString();
            string message = $"{ex.Message}\n{ex.StackTrace}\n{ex.Data}";
            try
            {
                string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                EventLog myLog = new EventLog($"{appName}");
                myLog.Source = $"{appName}_Source";
                myLog.WriteEntry(message, EventLogEntryType.Error);
            }
            catch {}             
            showMessageWithOptions(message, title, true); 
            throw ex;                        
        }

        public static void ToMessageBox(this string str, string name = "", bool enableExit = true)
        {
            showMessageWithOptions(str, name, enableExit);
        }

        public static void ToMessageBox(this string[] strArray, string name = "", bool enableExit = true)
        {
            List<string> strList = new List<string>{};            
            foreach(string s in strArray)
            {
                strList.Add(s);
                if (strList.Count >= 25)
                {
                    showMessageWithOptions(strList.ToArray().Join("\n"), name, enableExit);
                    strList.Clear();                    
                }
            }
            showMessageWithOptions(strList.ToArray().Join("\n"), name, enableExit);            
        }

        public static void ToMessageBox(this Dictionary<string,string> obj, string name = "", bool enableExit = true)        
        {
            Dictionary<string,string> myDict = (Dictionary<string,string>)obj;
            List<string> items = new List<string>{};
            foreach(var x in myDict) 
            {
                items.Add($"'{x.Key}:{x.Value}'\n");
            }
            items.ToArray().ToMessageBox(name, enableExit);            
        }  

        public static string[] ToNames(this ServiceController[] services)
        {
            List<string> names = new List<string>{};
            foreach(ServiceController sc in services)
            {
                names.Add(sc.ServiceName.Trim());
            }
            return names.ToArray();
        }

        public static string ToPrompt(this string message)
        {
            Console.Write(message);
            return Console.ReadLine();
        }

        public static List<string> ToList(this string[] strArray)
        {
            List<string> list = new List<string>();
            foreach(string s in strArray)
            {
                list.Add(s);
            }
            return list;
        }

        public static string ReadFileAsString(this string filename)
        {                           
            string fileContents = "";
            try
            {
                string localFilePath = Path.Combine(Environment.CurrentDirectory, filename);                

                bool fileExists = File.Exists(localFilePath);               

                using (StreamReader r = File.OpenText(localFilePath))
                {
                    fileContents = r.ReadToEnd();                    
                }                            
            }
            catch(Exception ex)
            {   
                try
                {
                    ex.ToMessageBox($"failed to read from {filename}");
                }
                catch
                {
                    Console.WriteLine($"failed to read from {filename}");
                }
                throw;                
            }
            return fileContents;        
        }

        private static string FindIndexedProcessName(int pid) {
            var processName = Process.GetProcessById(pid).ProcessName;
            var processesByName = Process.GetProcessesByName(processName);
            string processIndexdName = null;

            for (var index = 0; index < processesByName.Length; index++) {
                processIndexdName = index == 0 ? processName : processName + "#" + index;
                var processId = new PerformanceCounter("Process", "ID Process", processIndexdName);
                if ((int) processId.NextValue() == pid) {
                    return processIndexdName;
                }
            }

            return processIndexdName;
        }

        private static Process FindPidFromIndexedProcessName(string indexedProcessName) {
            var parentId = new PerformanceCounter("Process", "Creating Process ID", indexedProcessName);
            return Process.GetProcessById((int) parentId.NextValue());
        }

        public static Process Parent(this Process process) {
            return FindPidFromIndexedProcessName(FindIndexedProcessName(process.Id));
        }

        public static CmdResult GetCommandResult(this Process proc)
        {
            return new CmdResult(proc);            
        }

        public static (T,T[]) Pop<T>(this T[] arr)
        {
            if (arr.Length == 0)
                throw new ArgumentOutOfRangeException();
            if (arr.Length == 1)
            {
                return (arr[0], new T[0]);
            }
            return (arr[0], new ArraySegment<T>(arr, 1, arr.Length-1).ToArray());
        }

        public static void ToConsole(this string str)
        {
            Console.WriteLine(str);            
        }

        public static void ToConsole(this IEnumerable<string> strArray, bool indexed = false)
        {
            if (indexed)
            {
                for (int i=0; i < strArray.Count(); i++)
                {
                    Console.WriteLine($"{i}:{strArray.ElementAt(i)}");
                }                
            }
            else
            {
                strArray.ForEach( (str) => 
                {
                    if (str.Length > 0) str.ToConsole();
                });
            }            
        }

        public static List<string> ToLower(this IEnumerable<string> strArray)
        {
            List<string> lowerCase = new List<string>();
            strArray.ForEach( (str) => 
            {
                lowerCase.Append(str.ToLower());
            });
            return lowerCase;
        }

        public static string[] Normalize(this string[] paths, Func<string,string> normalize)
        {
            string[] normalized = new string[paths.Length];
            for (int i=0; i<paths.Length; i++)
            {
                normalized[i] = normalize(paths[i]);
            }
            return normalized;
        }

        public static string Name<T>(this T enumValue) where T : System.Enum
        {
            return Enum.GetName(typeof(T), enumValue);
        }
    }
}