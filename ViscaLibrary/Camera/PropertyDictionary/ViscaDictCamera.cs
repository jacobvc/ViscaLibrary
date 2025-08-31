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
        private readonly byte _id;
        private readonly ViscaProtocolProcessor _visca;

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
        public ViscaCamera(ViscaCameraId id, ViscaProtocolProcessor visca)
            : this(id)
        {
            _id = (byte)id;
            _visca = visca;

            // Default poll is everything in InquiriesByPropertyName
            _pollList = new Dictionary<string, ViscaInquiry>(InquiriesByPropertyName); 
#if SSHARP
            _pollTimer = new CTimer((o) => 
#else
            _pollTimer = new Timer((o) =>
#endif
                {
                    Poll();
                }, null, Timeout.Infinite, Timeout.Infinite);

        }

        #region Polling interface
        private Dictionary<string, ViscaInquiry> _pollList = new Dictionary<string, ViscaInquiry>();

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
                foreach (ViscaInquiry inquiry in _pollList.Values)
                    _visca.EnqueueCommand(inquiry);
            }
        }

        public void PollListNew()
        {
            _pollList.Clear();
        }
        public void PollListAdd(string propertyName)
        {
            _pollList[propertyName] = InquiriesByPropertyName[propertyName];
        }
        public void PollListAddRange(string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                PollListAdd(propertyName);
            }
        }

        #endregion Polling Commands

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}: id = {1}\r\n", GetType().Name, _id);
            foreach (string propertyName in _pollList.Keys)
            {
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