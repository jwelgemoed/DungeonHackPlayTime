using SlimDX.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DungeonHack.Engine
{
    public class RenderPanel : Panel
    {
        public RenderPanel() : base()
        {
            ClientSize = new System.Drawing.Size(800, 600);
            MinimumSize = new System.Drawing.Size(200, 200);
            ResizeRedraw = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }

        public DisplayMonitor Monitor { get; }

        public event EventHandler AppActivated;
        public event EventHandler AppDeactivated;
        public event EventHandler MonitorChanged;
        public event EventHandler PauseRendering;
        public event EventHandler ResumeRendering;
        public event CancelEventHandler Screensaver;
        public event EventHandler SystemResume;
        public event EventHandler SystemSuspend;
        public event EventHandler UserResized;

        protected virtual void OnAppActivated(EventArgs e)
        {
        }
        protected virtual void OnAppDeactivated(EventArgs e)
        { }
        protected virtual void OnMonitorChanged(EventArgs e)
        { }
        protected override void OnPaintBackground(PaintEventArgs e)
        { }
        protected virtual void OnPauseRendering(EventArgs e)
        { }
        protected override void OnResize(EventArgs e)
        { }
        protected virtual void OnResumeRendering(EventArgs e)
        { }
        protected virtual void OnScreensaver(CancelEventArgs e)
        { }
        protected virtual void OnSystemResume(EventArgs e)
        { }
        protected virtual void OnSystemSuspend(EventArgs e)
        { }
        protected virtual void OnUserResized(EventArgs e)
        { }
        protected void raise_AppActivated(object value0, EventArgs value1)
        { }
        protected void raise_AppDeactivated(object value0, EventArgs value1)
        { }
        protected void raise_MonitorChanged(object value0, EventArgs value1)
        { }
        protected void raise_PauseRendering(object value0, EventArgs value1)
        { }
        protected void raise_ResumeRendering(object value0, EventArgs value1)
        { }
        protected void raise_Screensaver(object value0, CancelEventArgs value1)
        { }
        protected void raise_SystemResume(object value0, EventArgs value1)
        { }
        protected void raise_SystemSuspend(object value0, EventArgs value1)
        { }
        protected void raise_UserResized(object value0, EventArgs value1)
        { }
        protected override void WndProc(ref Message m)
        { }
    }
}
