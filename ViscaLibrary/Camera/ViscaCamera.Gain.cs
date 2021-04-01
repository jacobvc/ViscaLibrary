﻿using System;

namespace Visca
{
    public partial class ViscaCamera
    {
        #region Gain Commands Definition

        private readonly ViscaGain _gainCmd;
        private readonly ViscaGainValue _gainValueCmd;
        private readonly ViscaGainInquiry _gainInquiry;

        #endregion Gain Commands Definition

        #region Gain Commands Implementations

        public void GainUp() { _visca.EnqueueCommand(_gainCmd.SetMode(UpDownMode.Up)); }
        public void GainDown() { _visca.EnqueueCommand(_gainCmd.SetMode(UpDownMode.Down)); }

        public event EventHandler<PositionEventArgs> GainChanged;

        protected virtual void OnGainChanged(PositionEventArgs e)
        {
            EventHandler<PositionEventArgs> handler = GainChanged;
#if SSHARP
            if (handler != null)
                handler(this, e);
#else
            handler?.Invoke(this, e);
#endif
        }

        private int _gain;

        public int Gain
        {
            get { return _gain; }
            set { _visca.EnqueueCommand(_gainValueCmd.SetPosition(value).OnCompletion(() => { _gain = value; })); }
        }

        #endregion Gain Commands Implementations

    }
}
