using System;
using System.IO;

namespace Library
{
    public sealed class TempFile : IDisposable
    {
        private string path;

        public string Path
        {
            get
            {
                return this.path;
            }
        }

        public TempFile()
        {
            this.path = System.IO.Path.GetTempFileName();
        }

        public TempFile(Action<string> somethingToDo)
        {
            this.path = System.IO.Path.GetTempFileName();
            using(var disposable = this)
            {
                somethingToDo(this.path);
            }
        }

        ~TempFile() 
        { 
            Dispose(false); 
        }
        
        public void Dispose() 
        { 
            Dispose(true); 
        }
        
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);                
            }
            if (path != null)
            {
                try { File.Delete(path); }
                catch { } // best effort
                path = null;
            }
        }

        public string GetContents()
        {
            string fileContents = "";
            try
            {
                bool fileExists = File.Exists(this.path);               
                using (StreamReader r = File.OpenText(this.path))
                {
                    fileContents = r.ReadToEnd();                    
                }                            
            }
            catch(Exception ex)
            {                  
                ex.ToMessageBox($"failed to read from {this.path}");
            }
            return fileContents; 
        }
    }
}

