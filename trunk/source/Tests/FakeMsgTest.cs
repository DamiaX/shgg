/* 
 * SHGG
 * More info in SHGG.cs file 
 * 
*/

using System;
using System.Collections.Generic;
using NUnit.Framework;
using HAKGERSoft;

namespace HAKGERSoft.Tests {

    [TestFixture]
    public class FakeMsgTest {

        [Test]
        public void SimpleMsgTest() {
            ConnectionMock mc = new ConnectionMock();
            sHGG ggmock = new sHGG(mc);
            string msg = "Simple message output test";
            int rec = 234567;
            ggmock.GGSendMessage(rec, msg);
            Assert.AreEqual(mc.data.Length, 20 + 1 + msg.Length);
            Assert.AreEqual(mc.ReadUInt(), 0xb); // type
            Assert.AreEqual(mc.ReadUInt(), 12 + 1 + msg.Length); // size
            Assert.AreEqual(mc.ReadUInt(), rec); // rec
            mc.ReadUInt(); // seq
            mc.ReadUInt(); // msg class
            for (int i=0; i<msg.Length; i++)
                Assert.AreEqual(mc.ReadByte(), Convert.ToByte(msg[i]));
            Assert.AreEqual(mc.ReadByte(), 0); // null char
            Assert.IsTrue(mc.IsEnd);
        }

        [Test]
        public void ConfMsgTest() {
            ConnectionMock mc = new ConnectionMock();
            sHGG ggmock = new sHGG(mc);
            string msg = "Conference message output test";
            int[] recs = new int[] { 12345, 67890, 98765, 4321, 100001 };
            ggmock.GGSendMessage(recs, msg);
            Assert.AreEqual(mc.data.Length, (20 + 1 + msg.Length + 5 + (4*recs.Length))*recs.Length);
            for(int i = 0; i < recs.Length; i++) {
                Assert.AreEqual(mc.ReadUInt(), 0xb); // type
                Assert.AreEqual(mc.ReadUInt(), 12 + 1 + msg.Length + 5 + (4 * recs.Length)); // size
                Assert.AreEqual(mc.ReadUInt(), recs[i]); // rec
                mc.ReadUInt(); // seq
                mc.ReadUInt(); // msg class
                for (int j = 0; j < msg.Length; j++)
                    Assert.AreEqual(mc.ReadByte(), Convert.ToByte(msg[j]));
                Assert.AreEqual(mc.ReadByte(), 0); // null char
                Assert.AreEqual(mc.ReadByte(), 1); // conf flag
                Assert.AreEqual(mc.ReadUInt(), recs.Length);
                for(int m = 0; m < recs.Length; m++) 
                    Assert.AreEqual(mc.ReadUInt(), recs[m]);
            }
            Assert.IsTrue(mc.IsEnd);
        }

        [Test]
        public void RichMsgTest() {
            ConnectionMock mc = new ConnectionMock();
            sHGG ggmock = new sHGG(mc);
            int rec = 234567;
            string msg = "Rich message output test";
            SortedDictionary<short,string> format = new SortedDictionary<short,string>() { 
                { 5, "<u><green>" }, 
                { 10, "<b><gray>" }, 
                { 15, "<n>" } 
            };

            ggmock.GGSendMessage(rec, msg, format);
            Assert.AreEqual(mc.data.Length, 38 + msg.Length + 1); 
            // msg
            Assert.AreEqual(mc.ReadUInt(), 0xb); // type
            Assert.AreEqual(mc.ReadUInt(), 30 + msg.Length + 1); // size 
            Assert.AreEqual(mc.ReadUInt(), rec); // rec
            mc.ReadUInt(); // seq
            mc.ReadUInt(); // msg class
            for (int i = 0;i < msg.Length;i++)
                Assert.AreEqual(mc.ReadByte(), Convert.ToByte(msg[i]));
            Assert.AreEqual(mc.ReadByte(), 0); // null char
            Assert.IsFalse(mc.IsEnd);
            // rich info
            Assert.AreEqual(mc.ReadByte(), 2); // rich info flag
            Assert.AreEqual(mc.ReadShort(), 15); // rich length
            
            // rich format list
            // { 5, "<u><green>" }
            Assert.AreEqual(mc.ReadShort(), 5); // pos
            Assert.AreEqual(mc.ReadByte(), 0x4 | 0x8); // font
            Assert.AreEqual(mc.ReadByte(), 91); // color: R
            Assert.AreEqual(mc.ReadByte(), 164); // color: G
            Assert.AreEqual(mc.ReadByte(), 42); // color: B
            // { 10, "<b><gray>" }
            Assert.AreEqual(mc.ReadShort(), 10); // pos
            Assert.AreEqual(mc.ReadByte(), 0x1 | 0x8); // font
            Assert.AreEqual(mc.ReadByte(), 128); // color: R
            Assert.AreEqual(mc.ReadByte(), 128); // color: G
            Assert.AreEqual(mc.ReadByte(), 128); // color: B
            // { 15, "<n>" }
            Assert.AreEqual(mc.ReadShort(), 15); // pos
            Assert.AreEqual(mc.ReadByte(), 0x0); // font

            Assert.IsTrue(mc.IsEnd);
        }

        [Test]
        [Ignore]
        public void RichConfMsgTest() {




        }




    }
}
