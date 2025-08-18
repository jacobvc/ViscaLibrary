using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
#if SSHARP
using Crestron.SimplSharp;
#else
using System.Threading;
#endif

namespace Visca
{
    public partial class ViscaCamera
    {
        public class GenericEventArgs<T> : EventArgs
        {
            public T EventData { get; private set; }

            public GenericEventArgs(T EventData)
            {
                this.EventData = EventData;
            }
        }
        public class OnOffEventArgs : EventArgs
        {
            private readonly bool _value;
            public bool On { get { return _value; } }
            public bool Off { get { return !_value; } }
            public OnOffEventArgs(bool value) : base() { _value = value; }
        }
        public class PositionEventArgs : EventArgs
        {
            public int Position;
            public PositionEventArgs(int position) : base() { this.Position = position; }
        }

        private readonly ViscaCameraId _id;
        protected readonly ViscaCameraParameters _parameters;
        protected readonly ViscaProtocolProcessor _visca;
        protected readonly List<ViscaInquiry> _pollCommands;

        /// <summary>
        /// Poll timer controls how often poll camera
        /// </summary>
#if SSHARP
        private readonly CTimer _pollTimer;
#else
        private readonly Timer _pollTimer;
#endif
        public ViscaCamera(ViscaCameraId id, ViscaCameraParameters parameters, ViscaProtocolProcessor visca)
            : this(id, parameters)
        {
            _id = id;
            _visca = visca;
            _pollCommands = new List<ViscaInquiry>();
#if SSHARP
            _pollTimer = new CTimer((o) => 
#else
            _pollTimer = new Timer((o) =>
#endif
                {
                    Poll();
                }, null, Timeout.Infinite, Timeout.Infinite);


            _pollCommands.Add(_aeInquiry);
            _pollCommands.Add(_apertureInquiry);
            _pollCommands.Add(_backLightInquiry);
            _pollCommands.Add(_bGainInquiry);
            _pollCommands.Add(_expCompInquiry);
            _pollCommands.Add(_gainInquiry);
            _pollCommands.Add(_focusAutoInquiry);
            _pollCommands.Add(_focusPositionInquiry);
            _pollCommands.Add(_irisInquiry);
            _pollCommands.Add(_muteInquiry);
            _pollCommands.Add(_powerInquiry);
            _pollCommands.Add(_ptzPositionInquiry);
            _pollCommands.Add(_rGainInquiry);
            _pollCommands.Add(_shutterInquiry);
            _pollCommands.Add(_titleInquiry);
            _pollCommands.Add(_wbInquiry);
            _pollCommands.Add(_wideDynamicInquiry);
            _pollCommands.Add(_zoomPositionInquiry);
        }

        #region Polling commands

        /// <summary>
        /// Enable or Disable Poll option
        /// </summary>
        public bool PollEnabled { get; set; }

        private int _pollTime = Timeout.Infinite;
        /// <summary>
        /// Poll interval for automatic polling
        /// </summary>
        public int PollTime
        {
            get { return _pollTime; }
            set
            {
                if (_pollTime != value)
                {
                    _pollTime = value;
#if SSHARP
                    _pollTimer.Reset(_pollTime, _pollTime);
#else
                    _pollTimer.Change(_pollTime, _pollTime);
#endif
                }
            }
        }

        /// <summary>
        /// Manual Poll, have effect only if Polling Enabled
        /// </summary>
        public void Poll()
        {
            if (PollEnabled)
            {
                foreach (var command in _pollCommands)
                    _visca.EnqueueCommand(command);
            }
        }

        #endregion Polling Commands

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}: {1}\r\n", GetType().Name, (byte)_id);
            sb.AppendFormat("\tPower:\t\t{0}\r\n", Power ? "ON" : "OFF");
            sb.AppendFormat("\tPan:\t\t{0}\r\n", PanPosition);
            sb.AppendFormat("\tPan Speed:\t\t{0}\r\n", PanSpeed);
            sb.AppendFormat("\tTilt:\t\t{0}\r\n", TiltPosition);
            sb.AppendFormat("\tTilt Speed:\t\t{0}\r\n", TiltSpeed);
            sb.AppendFormat("\tAE:\t\t{0}\r\n", AE);
            sb.AppendFormat("\tAperture:\t\t{0}\r\n", Aperture);
            sb.AppendFormat("\tBackLight:\t\t{0}\r\n", BackLight ? "ON" : "OFF");
            sb.AppendFormat("\tExpComp:\t\t{0}\r\n", ExpComp);
            sb.AppendFormat("\tFocusMode:\t\t{0}\r\n", FocusAuto ? "ON" : "OFF");
            sb.AppendFormat("\tFocus:\t\t{0}\r\n", FocusPosition);
            sb.AppendFormat("\tGain:\t\t{0}\r\n", Gain);
            sb.AppendFormat("\tBGain:\t\t{0}\r\n", BGain);
            sb.AppendFormat("\tRGain:\t\t{0}\r\n", RGain);
            sb.AppendFormat("\tRGain: \t{0}\r\n", Iris);
            sb.AppendFormat("\tMute:\t\t{0}\r\n", Mute ? "ON" : "OFF");
            sb.AppendFormat("\tShutter:\t\t{0}\r\n", Shutter);
            sb.AppendFormat("\tTitle:\t\t{0}\r\n", Title ? "ON" : "OFF");
            sb.AppendFormat("\tWB:\t\t{0}\r\n", WB);
            sb.AppendFormat("\tWideDynamic:\t\t{0}\r\n", WideDynamicMode);
            sb.AppendFormat("\tZoom:\t\t{0}\r\n", ZoomPosition);
            sb.AppendFormat("\tZoom Speed:\t\t{0}\r\n", ZoomSpeed);

            return sb.ToString();
        }

        /// <summary>
        /// Connects the camera. Override in derived classes for specific connection logic.
        /// </summary>
        public virtual void Connect()
        {
            // Default implementation does nothing.
            // Derived classes should override for actual connection logic.
        }

        /// <summary>
        /// Disposes the camera resources. Override in derived classes for cleanup.
        /// </summary>
        public virtual void Dispose()
        {
            // Default implementation does nothing.
            // Derived classes should override for actual cleanup logic.
        }
    }
}