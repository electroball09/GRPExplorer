using System;
using System.Runtime.InteropServices;

namespace GRPExplorerLib.Util
{
    /// <summary>
    /// A class containing methods for showing a Windows Message Box. <para/>
    /// For more information, see the MSDN docs on the MessageBox function (https://msdn.microsoft.com/en-us/library/ms645505).
    /// </summary>
    public class WinMessageBox
    {
        const bool DEFAULT_CAPTURE_FOCUS = false;

        [DllImport("user32.dll")]
        static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType); //shows windows messagebox
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow(); //returns HWND of the foreground window

        /// <summary>
        /// Shows a Windows MessageBox.  NOTE: this will pause the current window until it returns, whether it be in editor or player.
        /// </summary>
        /// <param name="messageText">The text to display in the MessageBox body.  Use standard carriage returns to delimit line endings.</param>
        /// <param name="messageCaption">The text to display in the header of the MessageBox.  Note: if you want to specify no caption, use 
        ///     either <c>""</c> or <c>string.Empty</c> - specifying <c>null</c> results in the caption displaying as "Error."</param>
        /// <param name="flags">Flags that determine the appearance and behavior of the MessageBox. See <see cref="WinMessageBoxFlags"/> for more information.</param>
        /// <param name="captureFocus">Whether or not the MessageBox will capture focus from the window or not.</param>
        /// <returns>The result of the MessageBox. <see cref="WinMessageBoxResult"/></returns>
        public static WinMessageBoxResult Show(string messageText, string messageCaption, WinMessageBoxFlags flags, bool captureFocus)
        {
            IntPtr hWnd = GetForegroundWindow();
            if (!captureFocus)
                hWnd = IntPtr.Zero;
            int result = MessageBox(hWnd, messageText, messageCaption, (uint)flags);
            return (WinMessageBoxResult)result;
        }

        /// <summary>
        /// Shows a Windows MessageBox.
        /// </summary>
        /// <param name="messageText">The text to display in the MessageBox body.  Use standard carriage returns to delimit line endings.</param>
        /// <param name="messageCaption">The text to display in the header of the MessageBox.  Note: if you want to specify no caption, use 
        ///     either <c>""</c> or <c>string.Empty</c> - specifying <c>null</c> results in the caption displaying as "Error."</param>
        /// <param name="flags">Flags that determine the appearance and behavior of the MessageBox. See <see cref="WinMessageBoxFlags"/> for more information.</param>
        /// <returns>The result of the MessageBox. <see cref="WinMessageBoxResult"/></returns>
        public static WinMessageBoxResult Show(string messageText, string messageCaption, WinMessageBoxFlags flags)
        {
            return Show(messageText, messageCaption, flags, DEFAULT_CAPTURE_FOCUS);
        }

        /// <summary>
        /// Shows a WindowsMessageBox with no caption.
        /// </summary>
        /// <param name="messageText">The text to display in the MessageBox body.  Use standard carriage returns to delimit line endings.</param>
        /// <param name="flags">Flags that determine the appearance and behavior of the MessageBox. See <see cref="WinMessageBoxFlags"/> for more information.</param>
        /// <returns>The result of the MessageBox. <see cref="WinMessageBoxResult"/></returns>
        public static WinMessageBoxResult Show(string messageText, WinMessageBoxFlags flags)
        {
            return Show(messageText, string.Empty, flags, DEFAULT_CAPTURE_FOCUS);
        }

        /// <summary>
        /// Shows a Windows MessageBox with default appearance.
        /// </summary>
        /// <param name="messageText">The text to display in the MessageBox body.  Use standard carriage returns to delimit line endings.</param>
        /// <param name="messageCaption">The text to display in the header of the MessageBox.  Note: if you want to specify no caption, use 
        ///     either <c>""</c> or <c>string.Empty</c> - specifying <c>null</c> results in the caption displaying as "Error."</param>
        /// <returns>The result of the MessageBox. <see cref="WinMessageBoxResult"/></returns>
        public static WinMessageBoxResult Show(string messageText, string messageCaption)
        {
            return Show(messageText, messageCaption, WinMessageBoxFlags.Default, DEFAULT_CAPTURE_FOCUS);
        }

        /// <summary>
        /// Shows a Windows MessageBox with default appearance and no caption.
        /// </summary>
        /// <param name="messageText">The text to display in the MessageBox body.  Use standard carriage returns to delimit line endings.</param>
        /// <returns>The result of the MessageBox. <see cref="WinMessageBoxResult"/></returns>
        public static WinMessageBoxResult Show(string messageText)
        {
            return Show(messageText, string.Empty, WinMessageBoxFlags.Default, DEFAULT_CAPTURE_FOCUS);
        }
    }

    /// <summary>
    /// Flags used to modify the appearance and behavior of a WinMessageBox <para/>
    /// Prefixes: Specifying one or more of the same prefix type can have unexpected results. <para/>
    /// btn Prefix: specify one of these to modify the buttons displayed on the message box [default is btnOk]. <para/>
    /// icon Prefix: specify one of these to modify the displayed icon [default is iconNone].  Also plays chime based on icon type.  Some icon types have the same display results. <para/>
    /// def Prefix: specify one of these to change the default selected button [default is defButton1]. <para/>
    /// modal Prefix: specify one of these to change the display behavior of the window [default is modalApplModal]. <para/>
    /// For more information, see the MSDN docs on the MessageBox function (https://msdn.microsoft.com/en-us/library/ms645505).
    /// </summary>
    [Flags]
    public enum WinMessageBoxFlags : uint
    {
        Default = 0x00000000, //dummy that specifies no flags set, AKA btnOkay | iconNone | defButton1 | modalApplModal

        btnOkay = 0x00000000, //MB_OKAY
        btnOkayCancel = 0x00000001, //MB_OKAYCANCEL
        btnAbortRetryIgnore = 0x00000002, //MB_ABORTRETRYIGNORE
        btnYesNoCancel = 0x00000003, //MB_YESNOCANCEL
        btnYesNo = 0x00000004, //MB_YESNO
        btnRetryCancel = 0x00000005, //MB_RETRYCANCEL
        btnCancelTryContinue = 0x00000006, //MB_CANCELTRYCONTINUE
        btnHelp = 0x00004000, //MB_HELP

        iconNone = 0x00000000, //dummy - shows no icon and does not play a charm
        iconExclamation = 0x00000030, //MB_ICONEXCLAMATION - shows yellow "!" icon and plays a chime
        iconWarning = 0x00000030, //MB_ICONWARNING - shows yellow "!" icon and plays a chime
        iconInformation = 0x00000040, //MB_ICONINFORMATION - shows blue "i" icon and plays a chime
        iconAsterisk = 0x00000040, //MB_ICONASTERISK - shows blue "i" icon and plays a chime
        iconQuestion = 0x00000020, //MB_ICONQUESTION - shows blue "?" icon and does not play a chime
        iconStop = 0x00000010, //MB_ICONSTOP - shows red "X" icon and plays a chime
        iconError = 0x00000010, //MB_ICONERROR - shows red "X" icon and plays a chime
        iconHand = 0x00000010, //MB_ICONHAND - shows red "X" icon and plays a chime

        defButton1 = 0x00000000, //MB_DEFBUTTON1
        defButton2 = 0x00000100, //MB_DEFBUTTON2
        defButton3 = 0x00000200, //MB_DEFBUTTON3
        defButton4 = 0x00000300, //MB_DEFBUTTON4

        modalAppl = 0x00000000, //MB_APPLMODAL
        modalSystem = 0x00001000, //MB_SYSTEMMODAL
        modalTask = 0x00002000 //MB_TASKMODAL
    }

    /// <summary>
    /// The result/button press that closed the message box.  Some results can only be obtained through specific button flags. <para />
    /// If the result is Error, then an error occurred in the Win32 call. <para/>
    /// If the MessageBox had a Cancel button, then the result will be Cancel if the Cancel button was pressed, the Escape key was pressed <para/>
    /// If the user pressed the "Help" button then Windows will send a WM_HELP message to the application. <para/>
    /// For more information, see the MSDN docs on the MessageBox function (https://msdn.microsoft.com/en-us/library/ms645505).
    /// </summary>
    [Flags]
    public enum WinMessageBoxResult : int
    {
        Error = 0, //an error occured showing the message box
        Ok = 1, //IDOK
        Cancel = 2, //IDCANCEL
        Abort = 3, //IDABORT
        Retry = 4, //IDRETRY
        Ignore = 5, //IDIGNORE
        Yes = 6, //IDYES
        No = 7, //IDNO
        TryAgain = 10, //IDTRYAGAIN
        Continue = 11 //IDCONTINUE
    }
}