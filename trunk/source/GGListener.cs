/* 
 * SHGG
 * More info in SHGG.cs file 
 * 
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
            Sync.Post(new SendOrPostCallback((state) => { handler(this, args); }), null);
        }

        private void FlushData(uint bytes) {
            for (uint i = 0; i < bytes; i++)
                NetStream.ReadByte();
        }

        #region IdleEngine
        private bool idleEngineRunning = false;
        private bool stopIdleEngine = false;        
        
        private void StartIdleEngine() {
            if (idleEngineRunning)
                throw new sHGGException("Idle engine is already running");

            IdleEngine = new Thread(new ThreadStart(WaitForData));
            IdleEngine.Name = "TCP Listener (IdleEngine)"; // just for debugging purposes
            stopIdleEngine = false;
            IdleEngine.Start();
            // Don't exit method until thread starts
            while (!idleEngineRunning)
                Thread.Sleep(50);
        }

        private void StopIdleEngine() {
            while (idleEngineRunning) {
                stopIdleEngine = true;
                Thread.Sleep(50);
            }
        }

        private void WaitForData() {
            int msToWait = 0;
            lock (ThreadLock) {
                while (!stopIdleEngine) {
                    idleEngineRunning = true;

                    // Are we still connected?
                    if (TcpEngine.Client.Poll(10, System.Net.Sockets.SelectMode.SelectRead) && (TcpEngine.Client.Available == 0)) {
                        // DISCONNECTED!
                        IsGGLogged = false;
                        this.Users.UsersRestart();
                        //Timer.Abort();
                        PostCallback<EventArgs>(GGDisconnected, EventArgs.Empty);
                        return;
                    }

                    NetStream = this.TcpEngine.GetStream();
                    if (NetStream.CanRead && NetStream.DataAvailable) {
                        uint packetType = ReadUint();
                        ExecuteAction(packetType);
                        msToWait = 0;
                    } else {
                        // There's no data available to receive, let's wait some milliseconds
                        // to give other threads some processor time...
                        msToWait = msToWait >= 500 ? 500 : msToWait + 50;
                        Thread.Sleep(msToWait);
                    }
                }
            }
            idleEngineRunning = false;
        }
        #endregion
        
        private uint ReadUint() {
            return ((uint) NetStream.ReadByte() | (uint) NetStream.ReadByte() << 8 | (uint) NetStream.ReadByte() << 16 | (uint) NetStream.ReadByte() << 24);
        }

        private uint ReadShort() {
            return ((uint) NetStream.ReadByte() | (uint) NetStream.ReadByte() << 8);

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
            uint lenght = ReadUint();   // packet size, should be 0 or 1
            FlushData(lenght);          // ignore data
            if (IsGGLogged)
                return;
            IsGGLogged = true;
            // A timer won't work here - it works only in a windows form
            //Timer = new System.Windows.Forms.Timer();
            //Timer.Interval = PING_INTERVAL * 1000;
            //Timer.Tick += new EventHandler(OutPing);
            //Timer.Enabled = true;
            StartPing();
            this.StartUsersNotify();
            GGStatus = (GGStatus == GGStatusType.NotAvailable) ? GGStatusType.Invisible : vGGStatus;
            PostCallback<EventArgs>(GGLogged, EventArgs.Empty);
        }

        private void LoginFailedAction() {
            uint lenght = ReadUint();   // packet size, should be 0
            FlushData(lenght);
            IsGGLogged = false;
            PostCallback<EventArgs>(GGLogFailed, EventArgs.Empty);
            // todo: kill timer and WaitForData thread
        }

        private void DisconnectingAction() {
            uint lenght = ReadUint();   // packet size, should be 0
            FlushData(lenght);
            IsGGLogged = false;
            this.Users.UsersRestart();
            StopIdleEngine();
            PostCallback<EventArgs>(GGDisconnected, EventArgs.Empty);
        }

        private static DateTime utcStartTime = new DateTime(1970, 1, 1, 0, 0, 0);

        private void ReceiveMessageAction() {
            uint packetSize = ReadUint();
            MessageReceiveEventArgs messageArgs = new MessageReceiveEventArgs();
            messageArgs.Number = (int) ReadUint();
            int seq = (int) ReadUint();
            int time = (int) ReadUint();
            int msgClass = (int) ReadUint();

            if ((msgClass == MESSAGE_CLASS_NEWMSG) && (packetSize == 26)) {
                RequestImageAction((uint) messageArgs.Number);
                return;
            }

            messageArgs.Time = utcStartTime.AddSeconds(time);

            byte[] msg = new byte[packetSize - 16];

            for (int i = 0; i < packetSize - 16; i++)
                msg[i] = (byte) NetStream.ReadByte();

            messageArgs.Message = Encoding.GetEncoding(DEFAULT_ENCODING).GetString(msg);
            PostCallback<MessageReceiveEventArgs>(GGMessageReceive, messageArgs);
        }

        private void RequestImageAction(uint uin) {
            NetStream.ReadByte();
            NetStream.ReadByte();
            int size = (int) ReadUint();
            uint crc32 = ReadUint();
            ImageReply(size, crc32, uin);
        }

        private void NotifyReplyAction(uint packetType) {
            uint packetSize = ReadUint();

            if(packetSize > 0 && NetStream.DataAvailable) {
                GGUser user = new GGUser();
                user.GGNumber = (int) ReadUint() & 0xffffff;
                user.vGGStatus = StatusDecode((uint) NetStream.ReadByte());
                user.vIPAdress = ReadUint().ToString(); // todo
                user.vRemotePort = (int) ReadShort();
                user.vGGClientVersion = GGClientVersionDecode((byte) NetStream.ReadByte());
                user.vMaxImageSize = (byte) NetStream.ReadByte();

                NetStream.ReadByte(); // unknown

                if (packetSize > 14) {
                    int descSize = (packetType == IN_NOTIFY_REPLY60) ? (byte) NetStream.ReadByte() : (int) packetSize - 14;
                    byte[] desc = new byte[descSize];
                    for (int i = 0; i < descSize; i++)
                        desc[i] = (byte) NetStream.ReadByte();
                    user.vDescription = Encoding.GetEncoding(DEFAULT_ENCODING).GetString(desc);
                }
                this.Users.UserChangedHandler(user);
            }
        }

        private void PubDirReplyAction() {
            uint packetSize = ReadUint();
            byte replyType = (byte) NetStream.ReadByte();
            int seq = (int) ReadUint();

            byte[] reply = new byte[packetSize - 5];
            for (int i = 0; i < packetSize - 5; i++)
                reply[i] = (byte) NetStream.ReadByte();

            if (replyType == IN_PUBDIR_REPLY_SEARCH)
                PubDirSearchReply(reply);
            else
                PubDirDataReply(reply);
        }



    }
}