/* 
 * SHGG
 * More info in SHGG.cs file 
 * 
*/

using System;
using System.Collections;

namespace HAKGERSoft {

    /// <summary>
    /// Określa status gadu-gadu
    /// </summary>
    public enum GGStatusType { 
        /// <summary>
        /// niedostępny
        /// </summary>
        NotAvailable, 
        /// <summary>
        /// dostępny
        /// </summary>
        Available, 
        /// <summary>
        /// zaraz wracam
        /// </summary>
        Busy, 
        /// <summary>
        /// niewidoczny
        /// </summary>
        Invisible,
        /// <summary>
        /// zablokowany
        /// </summary>
        Blocked 
    }

    /// <summary>
    /// Określa płeć osoby
    /// </summary>
    public enum GGGender{
        /// <summary>
        /// mężczyzna
        /// </summary>
        Male,
        /// <summary>
        /// kobieta
        /// </summary>
        Female,
        /// <summary>
        /// obojętnie
        /// </summary>
        All
    }

    public sealed partial class sHGG {
        //private const string DEFAULT_GG_HOST = "m1.gadu-gadu.pl";
        internal const string DEFAULT_GG_HOST = "217.17.45.147";
        internal const int DEFAULT_GG_PORT = 8074;
        internal const int DEFAULT_GG_VERSION = 0x21; // 6.0 (build 133)
        internal const UInt16 DEFAULT_LOCAL_PORT = 1550;
        internal const string DEFAULT_ENCODING = "windows-1250";

        internal const uint IN_WELCOME = 0x1;
        internal const uint IN_LOGIN_OK = 0x3;
        internal const uint IN_LOGIN_FAILED = 0x9;
        internal const uint IN_DISCONNECTING = 0xb;
        internal const uint IN_RECEIVE_MESSAGE = 0xa;
        internal const uint IN_MESSAGE_ACK = 0x5;
        internal const uint IN_NOTIFY_REPLY = 0xc;
        internal const uint IN_NOTIFY_REPLY60 = 0x11;
        internal const uint IN_STATUS = 0x2;
        internal const uint IN_STATUS60 = 0xf;
        internal const uint IN_PUBDIR_REPLY = 0xe;
        internal const byte IN_PUBDIR_REPLY_SEARCH = 0x5;

        internal const uint OUT_LOGIN60 = 0x15;
        internal const uint OUT_STATUS_CHANGE = 0x2;
        internal const uint OUT_PING = 0x8;
        internal const uint OUT_MESSAGE = 0xb;
        internal const uint OUT_MESSAGE_CONFERENCE_FLAG = 0x1;
        internal const byte OUT_MESSAGE_RICH_FLAG = 0x2;
        internal const byte OUT_USERS_NOTIFY_FIRST = 0xf;
        internal const byte OUT_USERS_NOTIFY_LAST = 0x10;
        internal const byte OUT_USERS_LIST_EMPTY = 0x12;
        internal const byte OUT_USERS_ADD_NOTIFY = 0xd;
        internal const byte OUT_USERS_REMOVE_NOTIFY = 0xe;
        internal const byte OUT_USERS_REQUEST = 0x16;
        internal const uint OUT_PUBDIR_REQUEST = 0x14;
        internal const byte OUT_PUBDIR_WRITE = 0x1;
        internal const byte OUT_PUBDIR_READ = 0x2;
        internal const byte OUT_PUBDIR_SEARCH = 0x3;

        internal const uint MESSAGE_CLASS_NEWMSG = 0x4;
        internal const uint MESSAGE_CLASS_CHAT = 0x8;
        //internal const uint MESSAGE_CLASS_CTCP = 0x10;
        //internal const uint MESSAGE_CLASS_ACK = 0x20;
   
        internal const uint MESSAGE_ACK_BLOCKED = 0x1;
        internal const uint MESSAGE_ACK_DELIVERED = 0x2;
        internal const uint MESSAGE_ACK_QUEUED = 0x3;
        internal const uint MESSAGE_ACK_MBFULL = 0x4;
        internal const uint MESSAGE_ACK_NOTDELIVERED = 0x6;

        internal const byte USER_BUDDY = 0x1;
        internal const byte USER_FRIEND = 0x2;
        internal const byte USER_BLOCKED = 0x4;

        internal const uint STATUS_NOT_AVAILABLE = 0x1;
        internal const uint STATUS_NOT_AVAILABLE_DESC = 0x15;
        internal const uint STATUS_AVAILABLE = 0x2;
        internal const uint STATUS_AVAILABLE_DESC = 0x4;
        internal const uint STATUS_BUSY = 0x3;
        internal const uint STATUS_BUSY_DESC = 0x5;
        internal const uint STATUS_INVISIBLE = 0x14;
        internal const uint STATUS_INVISIBLE_DESC = 0x16;
        internal const uint STATUS_BLOCKED = 0x6;
        internal const uint FRIENDS_MASK = 0x8000;

        internal const byte USERLIST_PUT = 0x0;
        internal const byte USERLIST_PUTMORE = 0x1;
        internal const byte USERLIST_GET = 0x2;
  
        internal const byte FONT_NONE = 0x0;
        internal const byte FONT_BOLD = 0x1;
        internal const byte FONT_ITALIC = 0x2;
        internal const byte FONT_UNDERLINE = 0x4;
        internal const byte FONT_COLOR = 0x8;
        internal const byte FONT_IMAGE = 0x80; // GGSendImage()

        internal const string FONT_TAG_NONE = "<n>";
        internal const string FONT_TAG_BOLD = "<b>";
        internal const string FONT_TAG_ITALIC = "<i>";
        internal const string FONT_TAG_UNDERLINE = "<u>";

        internal Hashtable FONT_COLOR_CODES = new Hashtable { 
            { "<black>", "000000" },                                           
            { "<blue>", "70A3BA" },                                             
            { "<red>", "FF0000" }, 
            { "<green>", "5BA42A" }, 
            { "<purple>", "985479" }, 
            { "<navy>", "126992" }, 
            { "<gray>", "808080" }, 
            { "<gold>", "FBBC35" }, 
            { "<lightblue>", "8ED5F2" }, 
            { "<lemon>", "FFFF00" }, 
            { "<silver>", "C0C0C0" }, 
            { "<orange>", "FF6300" }, 
            { "<maroon>", "840000" }, 
            { "<pistachio>", "B7D193" }, 
            { "<lightpurple>", "F28EE5" }, 
            { "<lightpink>", "FF9C9C" }, 
            { "<pink>", "FC5A8C" }, 
            { "<aqua>", "008484" } };

        internal Hashtable GG_VERSIONS = new Hashtable { 
            { 0x2a, "7.7 (build 3315)" },                                           
            { 0x29, "7.6 (build 1688)" },                                             
            { 0x28, "7.5.0 (build 2201)" }, 
            { 0x27, "7.0 (build 22)" }, 
            { 0x26, "7.0 (build 20)" }, 
            { 0x25, "7.0 (build 1)" }, 
            { 0x24, "6.1 (build 155) lub 7.6 (build 1359)" }, 
            { 0x22, "6.0 (build 140)" }, 
            { 0x21, "6.0 (build 133)" }, 
            { 0x20, "6.0" } }; 

        internal const int MAX_DESCRIPTIONS_SIZE = 70;
        internal const int MAX_MESSAGE_SIZE = 1989;
        internal const int MAX_RECIPIENTS_COUNT = 12;

        internal const int PING_INTERVAL = 120; // [sek] 
        internal const int SPLIT_NOTIFY = 400;

    }
}