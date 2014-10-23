using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using ScreenShotCapture;
using System.Windows.Media;

namespace Delay
{
    /// <summary>
    /// Class implementing support for "minimize to tray" functionality.
    /// </summary>
    public static class MinimizeToTray
    {
        /// <summary>
        /// Enables "minimize to tray" behavior for the specified Window.
        /// </summary>
        /// <param name="window">Window to enable the behavior for.</param>
        public static MinimizeToTrayInstance _tray; 
        public static void Enable(Window window)
        {
            // No need to track this instance; its event handlers will keep it alive
           _tray =  new MinimizeToTrayInstance(window);
        }

        public static void RecodeMode(Icon _icon)
        {
            _tray.ChangeIcon(_icon);
        }

        /// <summary>
        /// Class implementing "minimize to tray" functionality for a Window instance.
        /// </summary>
        public class MinimizeToTrayInstance
        {
            private Window _window;
            private NotifyIcon _notifyIcon;
            private static Icon _iconSource;
            private bool _balloonShown;

            /// <summary>
            /// Initializes a new instance of the MinimizeToTrayInstance class.
            /// </summary>
            /// <param name="window">Window instance to attach to.</param>
            public MinimizeToTrayInstance(Window window)
            {
                Debug.Assert(window != null, "window parameter is null.");
                _window = window;
                _window.StateChanged += new EventHandler(HandleStateChanged);
            }
            public void ChangeIcon(Icon _icon)
            {
                if (_notifyIcon != null)
                {
                    this._notifyIcon.Icon = _icon;
                }
                else
                {
                    _iconSource = _icon;
                }

            }

            /// <summary>
            /// Handles the Window's StateChanged event.
            /// </summary>
            /// <param name="sender">Event source.</param>
            /// <param name="e">Event arguments.</param>
            private void HandleStateChanged(object sender, EventArgs e)
            {
                if (_notifyIcon == null)
                {
                    // Initialize NotifyIcon instance "on demand"
                    _notifyIcon = new NotifyIcon();
                    _notifyIcon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetEntryAssembly().Location);
                    //_notifyIcon.Icon = _iconSource;
                    _notifyIcon.MouseClick += new MouseEventHandler(HandleNotifyIconOrBalloonClicked);
                    _notifyIcon.BalloonTipClicked += new EventHandler(HandleNotifyIconOrBalloonClicked);
                }
                // Update copy of Window Title in case it has changed
                _notifyIcon.Text = _window.Title;

                // Show/hide Window and NotifyIcon
                var minimized = (_window.WindowState == WindowState.Minimized);
                _window.ShowInTaskbar = !minimized;
                _notifyIcon.Visible = minimized;
                if (minimized && !_balloonShown)
                {
                    // If this is the first time minimizing to the tray, show the user what happened
                    _notifyIcon.ShowBalloonTip(1000, null, _window.Title, ToolTipIcon.None);
                    _balloonShown = true;
                }
            }

            /// <summary>
            /// Handles a click on the notify icon or its balloon.
            /// </summary>
            /// <param name="sender">Event source.</param>
            /// <param name="e">Event arguments.</param>
            private void HandleNotifyIconOrBalloonClicked(object sender, EventArgs e)
            {
                // Restore the Window
                _window.WindowState = WindowState.Normal;
            }
        }

        
    }
}
