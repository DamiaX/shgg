/* 
 * SHGG
 * More info in SHGG.cs file 
 * 
*/

using System;
using System.IO;

namespace HAKGERSoft {

    internal class ConnectionMock: BinaryWriter {
        internal byte[] data;
        internal int pos;
        private bool CanRead {
            get { return this.pos <= this.data.Length - 1; }
        }
        internal bool IsEnd {
            get {return this.pos == this.data.Length - 1; }
        }

        internal ConnectionMock() {
            this.ClearData();
        }

        public override void Write(byte[] buffer) {
            this.Write(buffer, 0, buffer.Length);
        }

        public override void Write(byte[] buffer, int index, int count) {
            if (count != buffer.Length)
                Array.Resize<byte>(ref buffer, count);
            data = sHGG.ConcatArray<byte>(data, buffer);
        }

        internal void ClearData() {
            this.pos = -1;
            this.data = new byte[0] { };
        }

        internal byte ReadByte() {
            this.pos++;
            if (!this.CanRead)
                throw new OverflowException();
            return this.data[this.pos];
        }

        internal uint ReadUInt() {
            return (uint)this.ReadByte() | (uint)this.ReadByte() << 8 | (uint)this.ReadByte() << 16 | (uint)this.ReadByte() << 24;
        }

        internal int ReadInt() {
            return (int)this.ReadByte() | (int)this.ReadByte() << 8 | (int)this.ReadByte() << 16 | (int)this.ReadByte() << 24;
        }

        internal short ReadShort() {
            return (short)(this.ReadByte() | this.ReadByte() << 8);
        }





    }
}