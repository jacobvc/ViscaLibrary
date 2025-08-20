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
         /* 
         * Implement args required for library commands
         */
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ViscaCamera"/> class with the specified camera ID and parameters.
        /// Sets up all VISCA command and inquiry objects for camera control, using either the provided parameters or default parameters if null.
        /// Each command and inquiry is associated with a property name in <see cref="InquiriesByPropertyName"/>.
        /// </summary>
        /// <param name="id">The camera ID.</param>
        /// <param name="parameters">The camera parameters. If null, default parameters are used.</param>
        public ViscaCamera(ViscaCameraId id)
        {
            // Add all of the builtin Visca Limits to the dictionary
            limitsByPropertyName.Add(typeof(ViscaDefaults));

            // TODO replace _parameters with LimitsByPropertyName below and remove _parameters from all commands constructors
            ViscaCameraParameters _parameters = new ViscaCameraDefaultParameters();


            #region AE Commands Constructors

            _aeCmd = new ViscaAEMode((byte)id, AEMode.FullAuto);
            _aeInquiry = new ViscaAEInquiry((byte)id, new Action<AEMode>(mode => { updateAE(mode); }));
            InquiriesByPropertyName.Add("AE", _aeInquiry);

            #endregion AE Commands Constructors

            #region Aperture Commands Constructors

            _apertureCmd = new ViscaAperture((byte)id, UpDownMode.Up);
            _apertureValueCmd = new ViscaApertureValue((byte)id, 0);
            _apertureInquiry = new ViscaApertureInquiry((byte)id, new Action<int>(position => { updateAperture(position); }));
            InquiriesByPropertyName.Add("Aperture", _apertureInquiry);

            #endregion Aperture Commands Constructors

            #region BackLight Commands Constructors

            _backLightCmd = new ViscaBackLight((byte)id, OnOffMode.On);
            _backLightInquiry = new ViscaBackLightInquiry((byte)id, new Action<OnOffMode>(mode => { updateBackLight(mode); }));
            InquiriesByPropertyName.Add("BackLight", _backLightInquiry);

            #endregion BackLight Commands Constructors

            #region BGain Commands Constructors

            _bGainCmd = new ViscaBGain((byte)id, UpDownMode.Up);
            _bGainValueCmd = new ViscaBGainValue((byte)id, 0);
            _bGainInquiry = new ViscaBGainInquiry((byte)id, new Action<int>(position => { updateBGain(position); }));
            InquiriesByPropertyName.Add("BGain", _bGainInquiry);

            #endregion Gain Commands Constructors

            #region ExpComp Commands Constructors

            _expCompCmd = new ViscaExpComp((byte)id, UpDownMode.Up);
            _expCompValueCmd = new ViscaExpCompValue((byte)id, 0);
            _expCompInquiry = new ViscaExpCompInquiry((byte)id, new Action<int>(position => { updateExpComp(position); }));
            InquiriesByPropertyName.Add("ExpComp", _expCompInquiry);

            #endregion ExpComp Commands Constructors

            #region Focus Commands Constructors

            _focusStopCmd = new ViscaFocusStop((byte)id);
            _focusFarCmd = new ViscaFocusFar((byte)id);
            _focusNearCmd = new ViscaFocusNear((byte)id);
            _focusSpeed = new ViscaFocusSpeed(_parameters.FocusSpeedLimits);
            _focusFarWithSpeedCmd = new ViscaFocusFarWithSpeed((byte)id, _focusSpeed);
            _focusNearWithSpeedCmd = new ViscaFocusNearWithSpeed((byte)id, _focusSpeed);
            _focusTriggerCmd = new ViscaFocusTrigger((byte)id);
            _focusInfinityCmd = new ViscaFocusInfinity((byte)id);

            _focusNearLimitCmd = new ViscaFocusNearLimit((byte)id, 0x1000);

            _focusAutoCmd = new ViscaFocusAuto((byte)id, OnOffMode.On);
            _focusAutoToggleCmd = new ViscaFocusAutoToggle((byte)id);
            _focusAutoInquiry = new ViscaFocusAutoInquiry((byte)id, new Action<OnOffMode>(mode => { updateFocusAuto(mode); }));
            InquiriesByPropertyName.Add("FocusAuto", _focusAutoInquiry);

            _focusPositionCmd = new ViscaFocusPosition((byte)id, 0);
            _focusPositionInquiry = new ViscaFocusPositionInquiry((byte)id, new Action<int>(position => { updateFocusPosition(position); }));
            InquiriesByPropertyName.Add("FocusPosition", _focusPositionInquiry);

            #endregion Focus Commands Constructors

            #region Gain Commands Constructors

            _gainCmd = new ViscaGain((byte)id, UpDownMode.Up);
            _gainValueCmd = new ViscaGainValue((byte)id, 0);
            _gainInquiry = new ViscaGainInquiry((byte)id, new Action<int>(position => { updateGain(position); }));
            InquiriesByPropertyName.Add("Gain", _gainInquiry);

            #endregion Gain Commands Constructors

            #region Iris Commands Constructors

            _irisCmd = new ViscaIris((byte)id, UpDownMode.Up);
            _irisValueCmd = new ViscaIrisValue((byte)id, 0);
            _irisInquiry = new ViscaIrisInquiry((byte)id, new Action<int>(position => { updateIris(position); }));
            InquiriesByPropertyName.Add("Iris", _irisInquiry);

            #endregion Gain Commands Constructors

            #region Mute Commands Constructors

            _muteCmd = new ViscaMute((byte)id, OnOffMode.On);
            _muteInquiry = new ViscaMuteInquiry((byte)id, new Action<OnOffMode>(mode => { updateMute(mode); }));
            InquiriesByPropertyName.Add("Mute", _muteInquiry);

            #endregion Mute Commands Constructors

            #region Power Commands Constructors

            _powerCmd = new ViscaPower((byte)id, OnOffMode.On);
            _powerInquiry = new ViscaPowerInquiry((byte)id, new Action<OnOffMode>(mode => { updatePower(mode); }));
            InquiriesByPropertyName.Add("Power", _powerInquiry);

            #endregion Power Commands Constructors

            #region PTZ Commands Constructors

            _ptzHome = new ViscaPTZHome((byte)id);
            _ptzPanSpeed = new ViscaPanSpeed(_parameters.PanSpeedLimits);
            _ptzTiltSpeed = new ViscaTiltSpeed(_parameters.TiltSpeedLimits);
            _ptzStop = new ViscaPTZStop((byte)id, _ptzPanSpeed, _ptzTiltSpeed);
            _ptzUp = new ViscaPTZUp((byte)id, _ptzPanSpeed, _ptzTiltSpeed);
            _ptzDown = new ViscaPTZDown((byte)id, _ptzPanSpeed, _ptzTiltSpeed);
            _ptzLeft = new ViscaPTZLeft((byte)id, _ptzPanSpeed, _ptzTiltSpeed);
            _ptzRight = new ViscaPTZRight((byte)id, _ptzPanSpeed, _ptzTiltSpeed);
            _ptzUpLeft = new ViscaPTZUpLeft((byte)id, _ptzPanSpeed, _ptzTiltSpeed);
            _ptzUpRight = new ViscaPTZUpRight((byte)id, _ptzPanSpeed, _ptzTiltSpeed);
            _ptzDownLeft = new ViscaPTZDownLeft((byte)id, _ptzPanSpeed, _ptzTiltSpeed);
            _ptzDownRight = new ViscaPTZDownRight((byte)id, _ptzPanSpeed, _ptzTiltSpeed);
            _ptzAbsolute = new ViscaPTZPosition((byte)id, false, _ptzPanSpeed, _ptzTiltSpeed, 0, 0);
            _ptzRelative = new ViscaPTZPosition((byte)id, true, _ptzPanSpeed, _ptzTiltSpeed, 0, 0);
            _ptzPositionInquiry = new ViscaPTZPositionInquiry((byte)id, new Action<int, int>((panPosition, tiltPosition) => { updatePTZPosition(panPosition, tiltPosition); }));
            InquiriesByPropertyName.Add("PTZPosition", _ptzPositionInquiry);

            #endregion PTZ Commands Constructors

            #region RGain Commands Constructors

            _rGainCmd = new ViscaRGain((byte)id, UpDownMode.Up);
            _rGainValueCmd = new ViscaRGainValue((byte)id, 0);
            _rGainInquiry = new ViscaRGainInquiry((byte)id, new Action<int>(position => { updateRGain(position); }));
            InquiriesByPropertyName.Add("RGain", _rGainInquiry);

            #endregion RGain Commands Constructors

            #region Shutter Commands Constructors

            _shutterCmd = new ViscaShutter((byte)id, UpDownMode.Up);
            _shutterValueCmd = new ViscaShutterValue((byte)id, 0);
            _shutterInquiry = new ViscaShutterInquiry((byte)id, new Action<int>(position => { updateShutter(position); }));
            InquiriesByPropertyName.Add("Shutter", _shutterInquiry);

            #endregion Gain Commands Constructors

            #region Title Commands Constructors

            _titleCmd = new ViscaTitle((byte)id, OnOffMode.On);
            _titleInquiry = new ViscaTitleInquiry((byte)id, new Action<OnOffMode>(mode => { updateTitle(mode); }));
            InquiriesByPropertyName.Add("Title", _titleInquiry);

            #endregion Power Commands Constructors

            #region WB Commands Constructors

            _wbCmd = new ViscaWBMode((byte)id, WBMode.Auto);
            _wbInquiry = new ViscaWBInquiry((byte)id, new Action<WBMode>(mode => { updateWB(mode); }));
            InquiriesByPropertyName.Add("WB", _wbInquiry);

            #endregion WB Commands Constructors

            #region WideDynamic Commands Constructors

            _wideDynamicCmd = new ViscaWideDynamicMode((byte)id, OnOffMode.On);
            _wideDynamicInquiry = new ViscaWideDynamicInquiry((byte)id, new Action<OnOffMode>(mode => { updateWideDynamicMode(mode); }));
            InquiriesByPropertyName.Add("WideDynamic", _wideDynamicInquiry);

            #endregion WideDynamic Commands Constructors

            #region Zoom Commands Constructors

            _zoomStopCmd = new ViscaZoomStop((byte)id);
            _zoomTeleCmd = new ViscaZoomTele((byte)id);
            _zoomWideCmd = new ViscaZoomWide((byte)id);
            _zoomSpeed = new ViscaZoomSpeed(_parameters.ZoomSpeedLimits);
            _zoomTeleWithSpeedCmd = new ViscaZoomTeleWithSpeed((byte)id, _zoomSpeed);
            _zoomWideWithSpeedCmd = new ViscaZoomWideWithSpeed((byte)id, _zoomSpeed);
            _zoomPositionCmd = new ViscaZoomPosition((byte)id, 0);
            _zoomPositionInquiry = new ViscaZoomPositionInquiry((byte)id, new Action<int>(position => { updateZoomPosition(position); }));
            InquiriesByPropertyName.Add("ZoomPosition", _zoomPositionInquiry);

            #endregion Zoom Commands Constructors
        }
    }
}
