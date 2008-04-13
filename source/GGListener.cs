/* GGListener.cs

Copyright (c) HAKGERSoft 2000 - 2008 www.hakger.xorg.pl

This unit is owned by HAKGERSoft, any modifications without HAKGERSoft permission
are prohibited!

Author:
  DetoX [ reedi(at)poczta(dot)fm ]

Unit description:
  information in SHGG.cs file

Requirements:
  information in SHGG.cs file
 
Version:
  information in SHGG.cs file

Remarks:
  information in SHGG.cs file
*/

using System;
using System.Text;
using System.Threading;

namespace HAKGERSoft {
   
    public sealed partial class sHGG {
        private object ThreadLock = new object();

        internal void PostCallback<A>(GenericEventHandler<A> handler, A args) {
            if (handler == null)
                return;
            syncContext.Post(new SendOrPostCallback((state) => { handler(this, args); }), null);
        }

        private void WaitForData() {
            lock (ThreadLock) { 
                while (true) {
                    NetStream = this.TcpEngine.GetStream();
                    if (NetStream.CanRead) {
                        uint packetType = (uint)(NetStream.ReadByte() | NetStream.ReadByte() | NetStream.ReadByte() | NetStream.ReadByte());
                        ExecuteAction(packetType);
                    }
                }
            }
        }

        private uint ReadUint() {
            return ((uint)NetStream.ReadByte() | (uint)NetStream.ReadByte() << 8 | (uint)NetStream.ReadByte() << 16 | (uint)NetStream.ReadByte() << 24);
        }

        private uint ReadShort() {
            return ((uint)NetStream.ReadByte() | (uint)NetStream.ReadByte() << 8);

        }

        private void ExecuteAction(uint packetType) {
            switch (packetType) {
                case IN_WELCOME: // done
                    ReadUint();
                    uint seed = ReadUint();
                    OutLogin60(seed);
                    break;

                case IN_LOGIN_OK: // done
                    this.LoginOKAction();
                    break;

                case IN_LOGIN_FAILED: // done
                    this.LoginFailedAction();
                    break;

                case IN_DISCONNECTING: // done
                    this.DisconnectingAction();
                    break;

                case IN_RECEIVE_MESSAGE:
                    this.ReceiveMessageAction();
                    break;

                case IN_NOTIFY_REPLY60:
                case IN_STATUS60:
                    this.NotifyReplyAction(packetType);
                    break;

                case IN_PUBDIR_REPLY:
                    this.PubDirReplyAction();
                    break;
                    
                default:
                    break;
            }
        }

        private void LoginOKAction() {
            if (IsGGLogged)
                return;
            IsGGLogged = true;
            Timer = new System.Windows.Forms.Timer();
            Timer.Interval = PING_INTERVAL * 1000;
            Timer.Tick += new EventHandler(OutPing);
            Timer.Enabled = true;
            this.StartUsersNotify();
            GGStatus = (GGStatus == GGStatusType.NotAvailable) ? GGStatusType.Invisible : vGGStatus;
            PostCallback<EventArgs>(GGLogged, EventArgs.Empty);
        }

        private void LoginFailedAction() {
            IsGGLogged = false;
            PostCallback<EventArgs>(GGLogFailed, EventArgs.Empty);
        }

        private void DisconnectingAction() {
            IsGGLogged = false;
            this.Users.UsersRestart();
            PostCallback<EventArgs>(GGDisconnected, EventArgs.Empty);
        }

        private void ReceiveMessageAction() {
            uint packetSize = ReadUint();
            MessageReceiveEventArgs messageArgs = new MessageReceiveEventArgs();
            messageArgs.Number = (int)ReadUint();
            int seq = (int)ReadUint();
            int time = (int)ReadUint(); // todo!
            int msgClass = (int)ReadUint();

            if ((msgClass==MESSAGE_CLASS_NEWMSG) && (packetSize==26)) {
                RequestImageAction((uint)messageArgs.Number);
                return; 
            }

            messageArgs.Time = DateTime.Now; // todo!
            byte[] msg = new byte[packetSize - 16];

            for (int i = 0; i < packetSize - 16; i++)
                msg[i] = (byte)NetStream.ReadByte();

            messageArgs.Message = Encoding.GetEncoding(DEFAULT_ENCODING).GetString(msg);
            PostCallback<MessageReceiveEventArgs>(GGMessageReceive, messageArgs);
        }

        private void RequestImageAction(uint uin) {
            NetStream.ReadByte();
            NetStream.ReadByte();
            int size = (int)ReadUint();
            uint crc32 = ReadUint();
            ImageReply(size, crc32, uin);
        }

        private void NotifyReplyAction(uint packetType) {
            uint packetSize = ReadUint();

            GGUser user = new GGUser();
            user.GGNumber = (int)ReadUint() & 0xffffff;
            user.vGGStatus = StatusDecode((uint)NetStream.ReadByte());
            user.vIPAdress = ReadUint().ToString(); // todo
            user.vRemotePort = (int)ReadShort();
            user.vGGClientVersion = GGClientVersionDecode((byte)NetStream.ReadByte());
            user.vMaxImageSize = (byte)NetStream.ReadByte();
            
            NetStream.ReadByte(); // unknown

            if (packetSize > 14) {
                int descSize = (packetType == IN_NOTIFY_REPLY60) ? (byte)NetStream.ReadByte() : (int)packetSize - 14;
                byte[] desc = new byte[descSize];
                for (int i = 0; i < descSize; i++)
                    desc[i] = (byte)NetStream.ReadByte();
                user.vDescription = Encoding.GetEncoding(DEFAULT_ENCODING).GetString(desc);
            }
            this.Users.UserChangedHandler(user);
        }

        private void PubDirReplyAction() {
            uint packetSize = ReadUint();
            byte replyType = (byte)NetStream.ReadByte();
            int seq = (int)ReadUint();
 
            byte[] reply = new byte[packetSize - 5];
            for (int i = 0; i < packetSize - 5; i++)
                reply[i] = (byte)NetStream.ReadByte();

            if (replyType == IN_PUBDIR_REPLY_SEARCH)
                PubDirSearchReply(reply);
            else
                PubDirDataReply(reply);
        }



    }
}