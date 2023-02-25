using Library;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinFormsApp = System.Windows.Forms.Application;

namespace Library
{
    
    public class Notifier : Form
    {         
        private NotifyIcon  notifyIcon;
        
        public Notifier(string message)
        {            
            try
            {
                // this.trayMenu = new ContextMenu();                
                // MenuItem defaultExit = new MenuItem("E&xit", (o,e)=> Application.Exit());
                // defaultExit.DefaultItem = true;
                // this.defaultMenuItems = new List<MenuItem> {};
                // this.defaultMenuItems.Add(defaultExit);
                // this.sysTrayOptions = new List<SysTrayOption> {};
                // this.trayMenu.Popup += popupHandler;            

                this.notifyIcon = new NotifyIcon();                            
                this.notifyIcon.Icon = this.GetCurrentIcon();
                // this.notifyIcon.ContextMenu = trayMenu;
                this.notifyIcon.Visible = true;
                this.ShowInfoBalloon(message);
                this.Close();
            }
            catch (Exception ex)
            {
                ex.ToMessageBox("SysTrayIcon() -> constructor failed");
            }                                   
        }

        // public SysTrayIcon(EventHandler doubleClickHandler) : this()
        // {
        //     this.notifyIcon.Text = "double click for info";
        //     this.notifyIcon.DoubleClick += doubleClickHandler;
        // }                

        // public SysTrayIcon(List<SysTrayOption> customOptions, EventHandler doubleClickHandler) :this(doubleClickHandler)
        // {
        //     try
        //     {                
        //         this.sysTrayOptions.AddRange(customOptions);
        //         this.sysTrayOptions.AttachToParent(this);
        //     }
        //     catch (Exception ex)
        //     {
        //         ex.ToMessageBox("failed to instantiate SysTrayIcon()");
        //     }
        // }

        protected override void OnLoad(EventArgs e)
        {
            Visible       = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar. 
            base.OnLoad(e);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {                
                notifyIcon.Dispose();
            } 
            base.Dispose(isDisposing);
        }

        // private void popupHandler(object sender, EventArgs e)
        // {
        //     this.trayMenu.MenuItems.Clear();
        //     this.trayMenu.MenuItems.AddRange(this.defaultMenuItems.ToArray());            
        //     this.trayMenu.MenuItems.AddRange(this.sysTrayOptions.ToMenuItems().ToArray());
        // }

        public void ShowInfoBalloon(string message, int timeout = 8192)
        {            
            this.notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            this.notifyIcon.BalloonTipTitle = this.CurrentProgramName();               
            this.notifyIcon.BalloonTipText = message;
            this.notifyIcon.ShowBalloonTip(timeout);
        }
    }
}