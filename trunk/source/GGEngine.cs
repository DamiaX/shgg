/* GGEngine.cs

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
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace HAKGERSoft {

    public sealed partial class sHGG {
        internal BinBuffer<uint, imageBuffEl> imageBuff = new BinBuffer<uint, imageBuffEl>();
        internal struct imageBuffEl {
            internal byte[] bin;
            internal string path;
        }

        private void ForwardData(byte[] obj) {
            this.ForwardData(obj, 0, true);
        }

        private void ForwardData(byte[] obj, bool checkConn) {
            this.ForwardData(obj, 0, checkConn);
        }

        private void ForwardData(byte[] obj, int len) {
            this.ForwardData(obj, len, true);
        }

        private void ForwardData(byte[] obj, int len, bool checkConn) {
            if (obj == null)
                return;
            if (!this.IsGGLogged && !this.mock && checkConn)
                throw new sHGGException("Musisz się zalogować aby wywołać operację");
            BinaryWriter writer = GetConnectionObj();
            if (len == 0)
                writer.Write(obj);
            else
                writer.Write(obj, 0, len);
        }

        private BinaryWriter GetConnectionObj() {
            return mock ? this.mockObj ?? new ConnectionMock() : new BinaryWriter(NetStream, Encoding.ASCII);
        }

        private void OutConnect(string serverAddress) {
            if (this.mock)
                return;
            try {
                TcpEngine = new System.Net.Sockets.TcpClient();
                TcpEngine.ReceiveTimeout = 90;
                TcpEngine.Connect(serverAddress, DEFAULT_GG_PORT);
                StartIdleEngine();
            } catch {
                this.GGLogout();
                throw;
            } finally {
                IsGGLogged = false;
            }
        }

        #region PingTimer
        private bool pingThreadRunning = false;
        private bool stopPingThread = false;

        internal void StartPing() {
            if (pingThreadRunning)
                throw new sHGGException("Ping thread is already running");
            else {
                pingTimer = new Thread(new ThreadStart(pingThread));
                pingTimer.Name = "Ping timer";
                stopPingThread = false;
                pingTimer.Start();
                while (!pingThreadRunning)
                    Thread.Sleep(50);
            }
        }

        internal void StopPing() {
            while (pingThreadRunning)
                stopPingThread = true;
            pingTimer = null;
        }

        TimeSpan pingInterval = new TimeSpan(0, 4, 30);

        private void pingThread() {
            DateTime lastPing = DateTime.Now;
            while (!stopPingThread) {
                pingThreadRunning = true;
                if (DateTime.Now - lastPing >= pingInterval) {
                    OutPing(this, EventArgs.Empty);
                    lastPing = DateTime.Now;
                }
                Thread.Sleep(1000);
            }
            pingThreadRunning = false;
        }
        #endregion

        internal void OutPing(object sender, EventArgs e) {
            stPing outPing = new stPing();
            outPing.Header.Type = OUT_PING;
            outPing.Header.Size = 0;
            ForwardData(RawSerialize(outPing), 0);
        }

        internal void OutLogin60(uint seed) {
            stLogin60 outLogin60 = new stLogin60();
            outLogin60.Header.Type = OUT_LOGIN60;
            outLogin60.Header.Size = 31;
            outLogin60.Number = Convert.ToUInt32(GGNumber);
            outLogin60.Hash = (uint) Hash(GGPassword, seed);
            outLogin60.Status = STATUS_INVISIBLE;
            outLogin60.Version = DEFAULT_GG_VERSION;
            outLogin60.Unknown1 = (byte) 0x0;
            outLogin60.LocalIp = 0;
            outLogin60.LocalPort = DEFAULT_LOCAL_PORT;
            outLogin60.ExternalIp = 0;
            outLogin60.ExternalPort = (UInt16) 0;
            outLogin60.ImageSize = GGImageSize;
            outLogin60.Unknown2 = (byte) 0xbe;
            ForwardData(RawSerialize(outLogin60), false);
        }

        private void OutStatus() {
            stStatus outStatus = new stStatus();
            outStatus.Header.Type = (uint) OUT_STATUS_CHANGE;
            outStatus.Status = StatusCode(this.GGStatus, this.GGDescription);
            if (this.GGFriendsMask)
                outStatus.Status |= FRIENDS_MASK;
            outStatus.Desc = GGDescription;
            outStatus.Header.Size = 4 + (uint) GGDescription.Length;
            if (GGDescription != string.Empty)
                outStatus.Header.Size++;

            if (GGDescription == string.Empty)
                ForwardData(RawSerialize(outStatus), 12);
            else
                ForwardData(RawSerialize(outStatus), 13 + GGDescription.Length);

            if (this.GGStatus == GGStatusType.NotAvailable)
                this.GGLogout();
        }

        private void MessageEngine(int[] recs, string msg, SortedDictionary<short, string> msgFormat, bool conference) {
            try {
                ValidateMessage(recs, msg, msgFormat, conference);
            } catch (sHGGException) {
                this.GGLogout();
                throw;
            }
            if (msg.Length > MAX_MESSAGE_SIZE)
                msg = msg.Substring(0, MAX_MESSAGE_SIZE);
            for (int i = 0; i < recs.Length; i++)
                BuildMessage(i, msg, recs, msgFormat, conference);
        }

        private void ValidateMessage(int[] recs, string msg, SortedDictionary<short, string> msgFormat, bool conference) {
            if (recs == null || recs.Length == 0)
                throw new sHGGException("Brak informacji do kogo wiadomość ma zostać wysłana!");
            foreach (int recipient in recs)
                if (recipient <= 0)
                    throw new sHGGException(string.Format("Numer GG: {0} nie może być liczbą ujemną", recipient.ToString()));
        }

        private void ForwardMessage(stMsg outMsg, stMsgRecs outRecs, int[] recs, stMsgRich outMsgRich, List<stMsgRichFormat> outRichList, int listLength) {
            outMsg.Header.Size = 13 + (uint) outMsg.Message.Length;
            if (outRichList.Count > 0)
                outMsg.Header.Size += (uint) listLength + 3;
            if (recs.Length > 0)
                outMsg.Header.Size += (uint) (5 + (4 * recs.Length));
            ForwardData(RawSerialize(outMsg), 21 + outMsg.Message.Length);
            if (recs.Length > 0) {
                ForwardData(RawSerialize(outRecs), 5);
                for (int i = 0; i < recs.Length; i++)
                    ForwardData(RawSerialize(recs[i]));
            }
            if (outRichList.Count > 0) {
                ForwardData(RawSerialize(outMsgRich), 3);
                outRichList.ForEach(delegate(stMsgRichFormat formatItem) {
                    int rgbOffset = (formatItem.RGB != null) ? 6 : 3;
                    ForwardData(RawSerialize(formatItem), rgbOffset);
                });
            }
            return;
        }

        private stMsg BuildMsgInfo(uint uin, string msg) {
            stMsg outMsg = new stMsg();
            outMsg.Header.Type = OUT_MESSAGE;
            outMsg.Seq = 0;
            outMsg.Class = MESSAGE_CLASS_CHAT;
            outMsg.Message = msg;
            outMsg.Recipient = uin;
            return outMsg;
        }

        private stMsgRich BuildRichInfo(int listLength) {
            stMsgRich outMsgRich = new stMsgRich();
            outMsgRich.Flag = OUT_MESSAGE_RICH_FLAG;
            outMsgRich.Length = (short) listLength;
            return outMsgRich;
        }

        private stMsgRecs BuildRecipientsInfo(int recsCount) {
            stMsgRecs outRecs = new stMsgRecs();
            outRecs.Flag = (byte) OUT_MESSAGE_CONFERENCE_FLAG;
            outRecs.RecipientsCount = (uint) recsCount;
            return outRecs;
        }

        internal List<stMsgRichFormat> BuildRichText(ref SortedDictionary<short, string> msgFormat, out int listLength) {
            listLength = 0;
            if (msgFormat == null)
                return new List<stMsgRichFormat>();
            List<stMsgRichFormat> outRichList = new List<stMsgRichFormat>();
            IDictionaryEnumerator msgFEnum = msgFormat.GetEnumerator();
            while (msgFEnum.MoveNext()) {
                stMsgRichFormat outMsgRich = new stMsgRichFormat();
                string formatTags = msgFEnum.Value.ToString();
                outMsgRich.Position = short.Parse(msgFEnum.Key.ToString()); ;
                outMsgRich.Font = FONT_NONE;
                if (formatTags.Contains(FONT_TAG_BOLD))
                    outMsgRich.Font |= FONT_BOLD;
                if (formatTags.Contains(FONT_TAG_ITALIC))
                    outMsgRich.Font |= FONT_ITALIC;
                if (formatTags.Contains(FONT_TAG_UNDERLINE))
                    outMsgRich.Font |= FONT_UNDERLINE;
                listLength += 3;
                IDictionaryEnumerator fontColorEnumerator = FONT_COLOR_CODES.GetEnumerator();
                while (fontColorEnumerator.MoveNext()) {
                    if (formatTags.Contains(fontColorEnumerator.Key.ToString())) {
                        outMsgRich.Font |= FONT_COLOR;
                        outMsgRich.RGB = new byte[3];
                        outMsgRich.RGB[0] = byte.Parse(fontColorEnumerator.Value.ToString().Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                        outMsgRich.RGB[1] = byte.Parse(fontColorEnumerator.Value.ToString().Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                        outMsgRich.RGB[2] = byte.Parse(fontColorEnumerator.Value.ToString().Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                        listLength += 3;
                    }
                }
                outRichList.Add(outMsgRich);
            }
            return outRichList;
        }

        private void BuildMessage(int recNum, string msg, int[] recs, SortedDictionary<short, string> msgFormat, bool conference) {
            stMsg outMsg = BuildMsgInfo((uint) recs[recNum], msg);
            stMsgRecs outMsgRecs = BuildRecipientsInfo(recs.Length);
            int listLength;
            List<stMsgRichFormat> outMsgRichFormat = BuildRichText(ref msgFormat, out listLength);
            stMsgRich outMsgRich = BuildRichInfo(listLength);
            if (!conference)
                ForwardMessage(outMsg, new stMsgRecs(), new int[0], outMsgRich, outMsgRichFormat, listLength);
            else
                ForwardMessage(outMsg, outMsgRecs, recs, outMsgRich, outMsgRichFormat, listLength);
        }

        private bool ImageEngine(int recipient, string msg, int imgPos, MemoryStream stream) {
            byte[] binary;
            string path;
            try {
                path = "image.jpg";
                binary = Stream2Array(stream);
            } catch (sHGGException) {
                this.GGLogout();
                throw;
            }
            BuildImage(0, msg, imgPos, new int[] { recipient }, null, false, binary, path);
            return true;
        }



        private bool ImageEngine(int recipient, string msg, int imgPos, string imagePath) {
            byte[] binary;
            string path;
            try {
                path = GetImage(imagePath, out binary);
            } catch (sHGGException) {
                this.GGLogout();
                throw;
            }
            BuildImage(0, msg, imgPos, new int[] { recipient }, null, false, binary, path);
            return true;
        }

        private string GetImage(string imagePath, out byte[] binary) {
            if (!File.Exists(imagePath))
                throw new sHGGException("Podana ścieżka nie istnieje!" + imagePath);
            if (Path.GetExtension(imagePath) != ".jpg")
                throw new sHGGException("Podany plik nie jest obrazkiem!" + imagePath);
            binary = File.ReadAllBytes(imagePath);
            if (binary == null)
                throw new sHGGException("Podany plik jest pusty!" + imagePath);
            return Path.GetFileName(imagePath);
        }

        private stMsgImage BuildImageQuery(byte[] binary, uint uin, string path) {
            stMsgImage outMsgImage = new stMsgImage();
            outMsgImage.Unknown = 0x109;
            outMsgImage.Size = binary.Length;
            outMsgImage.CRC32 = new CRC32().GetCrc32(new MemoryStream(binary));
            imageBuff.pushBin2Buff(uin, new imageBuffEl() { bin = binary, path = path });
            return outMsgImage;
        }

        internal void OutUsersNotify(GGUser user, uint notifyType, bool block) {
            stUsersNotify outUsersNotify = new stUsersNotify();
            outUsersNotify.Header.Type = notifyType;
            outUsersNotify.Header.Size = 5;
            outUsersNotify.GGNumber = user.GGNumber;
            outUsersNotify.Type = USER_BUDDY;
            if (user.Friend)
                outUsersNotify.Type |= USER_FRIEND;
            if (block)
                outUsersNotify.Type = USER_BLOCKED;
            ForwardData(RawSerialize(outUsersNotify), 13);
        }

        internal void OutNoUsers() {
            stHeader outNoUsers = new stHeader();
            outNoUsers.Type = OUT_USERS_LIST_EMPTY;
            outNoUsers.Size = 0;
            ForwardData(RawSerialize(outNoUsers), 8);
        }

        internal void StartUsersNotify() {
            if (Users.Count == 0) {
                OutNoUsers();
                return;
            }
            int lastPacket = SplitPacket(Users.Count, SPLIT_NOTIFY);
            for (int i = 0; i < Users.Count; i++) {
                if (i < lastPacket)
                    OutUsersNotify(Users[i], OUT_USERS_NOTIFY_FIRST, false);
                else
                    OutUsersNotify(Users[i], OUT_USERS_NOTIFY_LAST, false);
            }
        }

        private void BuildImage(int recNum, string msg, int imgPos, int[] recs, SortedDictionary<short, string> msgFormat, bool conference, byte[] bin, string path) {
            stMsg outMsg = BuildMsgInfo((uint) recs[recNum], msg);
            stMsgImage outMsgImage = BuildImageQuery(bin, outMsg.Recipient, path);
            int listLength;
            List<stMsgRichFormat> outMsgRichFormat = BuildRichImg(out listLength, imgPos);
            stMsgRich outMsgRich = BuildRichInfo(listLength);
            if (!conference)
                ForwardImageRequest(outMsg, outMsgRich, outMsgRichFormat, listLength, outMsgImage);
        }

        internal List<stMsgRichFormat> BuildRichImg(out int listLength, int imgPos) {
            listLength = 0;
            List<stMsgRichFormat> outRichList = new List<stMsgRichFormat>();
            stMsgRichFormat outMsgRich = new stMsgRichFormat();
            outMsgRich.Position = (short) imgPos; // or 1 ??? // todo
            outMsgRich.Font = FONT_IMAGE;
            listLength += 13;
            outRichList.Add(outMsgRich);
            return outRichList;
        }

        private void ForwardImageRequest(stMsg outMsg, stMsgRich outMsgRich, List<stMsgRichFormat> outRichList, int listLength, stMsgImage image) {
            outMsg.Header.Size = 13 + (uint) outMsg.Message.Length;
            if (outRichList.Count > 0)
                outMsg.Header.Size += (uint) listLength + 3;
            ForwardData(RawSerialize(outMsg), 21 + outMsg.Message.Length);
            if (outRichList.Count > 0) {
                ForwardData(RawSerialize(outMsgRich), 3);
                outRichList.ForEach(delegate(stMsgRichFormat formatItem) {
                    ForwardData(RawSerialize(formatItem), 3);
                    ForwardData(RawSerialize(image), 10);
                });
            }
            return;
        }

        internal void ImageReply(int size, uint crc32, uint uin) {
            stMsg outMsg = BuildMsgInfo(uin, string.Empty);
            stMsgImageReply outMsgImageReply = BuildImageReply(size, crc32);
            ForwardImage(outMsg, outMsgImageReply);
        }

        private stMsgImageReply BuildImageReply(int size, uint crc32) {
            stMsgImageReply outMsgImageReply = new stMsgImageReply();
            outMsgImageReply.Flag = 0x05; // or 0x06
            outMsgImageReply.Size = size;
            outMsgImageReply.CRC32 = crc32;
            return outMsgImageReply;
        }

        private void ForwardImage(stMsg outMsg, stMsgImageReply outMsgImageReply) {
            imageBuffEl buffEl = imageBuff.popBinFromBuff(outMsg.Recipient);
            byte[][] imagePack = sHGG.ArrayChop<byte>(buffEl.bin, 1800);
            byte[] filename = StrToByteArray(buffEl.path);
            for (int i = 0; i < imagePack.Length; i++) {
                outMsg.Header.Size = 13 + (uint) imagePack[i].Length; // msg
                outMsg.Header.Size += 9; // image reply struct
                if (i == 0)
                    outMsg.Header.Size += (uint) (filename.Length + 1); // image name
                outMsgImageReply.Flag = (i > 0) ? (byte) 0x6 : (byte) 0x5;
                ForwardData(RawSerialize(outMsg), 21 + outMsg.Message.Length);
                ForwardData(RawSerialize(outMsgImageReply), 9);
                if (i == 0) {
                    ForwardData(filename);
                    ForwardData(new byte[] { 0 });
                }
                ForwardData(imagePack[i]);
            }
        }
    }
}