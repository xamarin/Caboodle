using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Email
    {
        internal static bool IsComposeSupported => true;

        static Task PlatformComposeAsync(EmailMessage message)
        {
            var msg = default(NativeMethods.MapiMessage);

            try
            {
                var recipCount = (message.To is object ? message.To.Count : 0) + (message.Cc is object ? message.Cc.Count : 0) + (message.Bcc is object ? message.Bcc.Count : 0);

                if (!string.IsNullOrEmpty(message.Subject))
                    msg.lpszSubject = message.Subject;

                if (!string.IsNullOrEmpty(message.Body))
                    msg.lpszNoteText = message.Body;

                // recipients
                if (recipCount > 0)
                {
                    var recipdesclen = Marshal.SizeOf<NativeMethods.MapiRecipDescW>();
                    msg.lpRecips = Marshal.AllocHGlobal(recipdesclen * recipCount);
                    msg.nRecipCount = recipCount;
                    var currentRecip = 0;

                    if (message.To is object)
                    {
                        foreach (var r in message.To)
                        {
                            var rd = new NativeMethods.MapiRecipDescW(r, r, NativeMethods.RecipientClass.MAPI_TO);
                            Marshal.StructureToPtr(rd, IntPtr.Add(msg.lpRecips, recipdesclen * currentRecip), false);

                            currentRecip++;
                        }
                    }

                    if (message.Cc is object)
                    {
                        foreach (var r in message.Cc)
                        {
                            var rd = new NativeMethods.MapiRecipDescW(r, r, NativeMethods.RecipientClass.MAPI_CC);
                            Marshal.StructureToPtr(rd, IntPtr.Add(msg.lpRecips, recipdesclen * currentRecip), false);

                            currentRecip++;
                        }
                    }

                    if (message.Bcc is object)
                    {
                        foreach (var r in message.Bcc)
                        {
                            var rd = new NativeMethods.MapiRecipDescW(r, r, NativeMethods.RecipientClass.MAPI_BCC);
                            Marshal.StructureToPtr(rd, IntPtr.Add(msg.lpRecips, recipdesclen * currentRecip), false);

                            currentRecip++;
                        }
                    }
                }

                // attachments
                if (message.Attachments is object)
                {
                    if (message.Attachments.Count > 0)
                    {
                        var fileDescLen = Marshal.SizeOf<NativeMethods.MapiFileDescW>();
                        msg.lpFiles = Marshal.AllocHGlobal(fileDescLen * message.Attachments.Count);
                        msg.nFileCount = message.Attachments.Count;

                        for (var i = 0; i < message.Attachments.Count; i++)
                        {
                            var f = new NativeMethods.MapiFileDescW(message.Attachments[i].FullPath, message.Attachments[i].FileName);
                            Marshal.StructureToPtr(f, IntPtr.Add(msg.lpFiles, fileDescLen * i), false);
                        }
                    }
                }

                var result = NativeMethods.MAPISendMailW(IntPtr.Zero, IntPtr.Zero, ref msg, 0xd, 0);
            }
            finally
            {
                FreeMapiMessage(msg);
            }

            return Task.CompletedTask;
        }

        static void FreeMapiMessage(NativeMethods.MapiMessage msg)
        {
            if (msg.lpFiles != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(msg.lpFiles);
                msg.lpFiles = IntPtr.Zero;
                msg.nFileCount = 0;
            }

            if (msg.lpRecips != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(msg.lpRecips);
                msg.lpRecips = IntPtr.Zero;
                msg.nRecipCount = 0;
            }
        }

        static class NativeMethods
        {
            [DllImport("Mapi32.dll", CharSet = CharSet.Unicode)]
            internal static extern uint MAPISendMailW(IntPtr lhSession, IntPtr ulUIParam, ref MapiMessage lpMessage, int flFlags, uint ulReserved);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct MapiMessage
            {
                uint ulReserved;
                internal string lpszSubject;
                internal string lpszNoteText;
                IntPtr lpszMessageType;
                IntPtr lpszDateReceived;
                IntPtr lpszConversationID;
                int flFlags;
                IntPtr lpOriginator;
                internal int nRecipCount;
                internal IntPtr lpRecips;
                internal int nFileCount;
                internal IntPtr lpFiles;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct MapiFileDescW
            {
                uint ulReserved;
                uint flFlags;
                int nPosition;
                string lpszPathName;
                string lpszFileName;
                uint lpFileType;

                public MapiFileDescW(string path, string filename)
                {
                    ulReserved = 0;
                    flFlags = 0;
                    nPosition = -1;
                    lpszPathName = path;
                    lpszFileName = filename;
                    lpFileType = 0;
                }
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct MapiRecipDescW
            {
                uint ulReserved;
                RecipientClass ulRecipClass;
                string lpszName;
                string lpszAddress;
                uint ulEIDSize;
                uint lpEntryID;

                public MapiRecipDescW(string address, string name, RecipientClass rc)
                {
                    ulReserved = 0;
                    ulRecipClass = rc;
                    lpszName = name;
                    lpszAddress = address;
                    ulEIDSize = 0;
                    lpEntryID = 0;
                }
            }

            internal enum RecipientClass
            {
                MAPI_ORIG = 0,
                MAPI_TO = 1,
                MAPI_CC = 2,
                MAPI_BCC = 3,
            }
        }
    }
}
