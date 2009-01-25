/* 
 * SHGG
 * More info in SHGG.cs file 
 * 
*/

using System;
using System.Runtime.InteropServices;

namespace HAKGERSoft {

    public sealed partial class sHGG {

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        internal struct stHeader {
            internal uint Type;
            internal uint Size;
        }

        # region Output structures

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        internal struct stLogin60 {
            internal stHeader Header;
            internal uint Number;
            internal uint Hash;
            internal uint Status;
            internal uint Version;
            internal byte Unknown1;
            internal uint LocalIp;
            internal UInt16 LocalPort;
            internal uint ExternalIp;
            internal UInt16 ExternalPort;
            internal byte ImageSize;
            internal byte Unknown2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        internal struct stStatus {
            internal stHeader Header;
            internal uint Status;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_DESCRIPTIONS_SIZE + 1)]
            internal string Desc;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        internal struct stPing {
            internal stHeader Header;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        internal struct stMsg {
            internal stHeader Header;
            internal uint Recipient;
            internal uint Seq;
            internal uint Class;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_MESSAGE_SIZE + 1)]
            internal string Message;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        internal struct stMsgRecs {
            internal byte Flag; // 1
            internal uint RecipientsCount;
            //[MarshalAs(UnmanagedType.AsAny)]
            //internal uint[] Recipients; // wysyłane osobno
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        internal struct stMsgRich {
            internal byte Flag;	// 2
            internal short Length;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        internal struct stMsgRichFormat {
            internal short Position;
            internal byte Font;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            internal byte[] RGB; // nie musi wystąpić
            //internal struct sggOutImage; // nie z tekstem!
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        internal struct stMsgImage {
            internal short Unknown;
            internal int Size;
            internal uint CRC32;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        internal struct stMsgImageReply {
            internal byte Flag;
            internal int Size;
            internal uint CRC32;
            //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
            //internal byte[] FileName;
            //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
            //internal byte[] Image;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        internal struct stUsersNotify {
            internal stHeader Header;
            internal int GGNumber;
            internal byte Type;            
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        internal struct stUsersRequest {
            internal stHeader Header;
            internal int GGNumber;
            internal byte Type;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        internal struct stPubDir {
            internal stHeader Header;
            internal byte RequestType;
            internal int Seq;
            //internal byte[] Request; // wysyłane osobno
        }

        #endregion

        # region Input structures

        /*
        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        private struct sggInMessage {
            internal sggHeader Header;
            internal uint Sender;
            internal uint Seq;
            internal uint Time;
            internal uint Class;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_MESSAGE_SIZE + 1)]
            internal string Message;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        private struct sggInMessageAck {
            internal sggHeader Header;
            internal uint Status;
            internal uint Recipient;
            internal uint Seq;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        private struct sggInStatus {
            internal sggHeader Header;
            internal uint Number;
            internal uint Status;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_DESCRIPTIONS_SIZE + 1)]
            internal string Description;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        private struct sggInStatus60 {
            internal sggHeader Header;
            internal uint Number;
            internal byte Status;
            internal int RemoteIPAddress;
            short RemotePort;
            byte ClientVersion;
            byte MaxImageSize;
            byte Unknown; // 0x00
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_DESCRIPTIONS_SIZE + 1)]
            internal string Description;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        private struct sggInPubDirReply {
            internal sggHeader Header;
            internal byte RequestType;
            internal int Seq;
            //internal byte[] Reply;
        }
        */

        # endregion

    }
}