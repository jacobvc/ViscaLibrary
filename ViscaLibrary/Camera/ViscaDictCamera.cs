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
        private readonly ViscaProtocolProcessor _visca;
        private readonly List<ViscaInquiry> _pollCommands;

        public ViscaRangeDictionary limitsByPropertyName = new ViscaRangeDictionary();
        public Dictionary<string, ViscaInquiry> InquiriesByPropertyName = new Dictionary<string, ViscaInquiry>();

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

            foreach (ViscaInquiry inquiry in InquiriesByPropertyName.Values)
            {
                if (inquiry != null)
                    _pollCommands.Add(inquiry);
            }
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
                foreach (ViscaInquiry inquiry in InquiriesByPropertyName.Values)
                    _visca.EnqueueCommand(inquiry);
            }
        }

        #endregion Polling Commands

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}: {1}\r\n", GetType().Name, (byte)_id);
            foreach (string propertyName in InquiriesByPropertyName.Keys)
            {
                PropertyInfo pinfo = typeof(int).GetProperty(propertyName);
                // Fix: GetValue requires an object instance as the first argument.
                // Since typeof(int) is used, but int does not have properties, this code is likely incorrect.
                // If you intend to get the value of a property from this instance, use:
                PropertyInfo instanceProp = this.GetType().GetProperty(propertyName);
                object value = instanceProp != null ? instanceProp.GetValue(this, null) : null;
                sb.AppendFormat("\t{0}:\t\t{1}\r\n", propertyName, value);
            }
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