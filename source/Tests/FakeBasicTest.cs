/* FakeBasicTest.cs

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
using NUnit.Framework;
using HAKGERSoft;

namespace HAKGERSoft.Tests {

    [TestFixture]
    public class FakeBasicTest {

        [Test]
        public void FakeLogin60Test() {
            ConnectionMock mc = new ConnectionMock();
            sHGG mockGG = new sHGG(mc);
            string pass = "abcdefg123";
            uint fakeSeed = 674456;
            mockGG.GGNumber = "100001";
            mockGG.GGPassword = pass;
            mockGG.OutLogin60(fakeSeed);
            Assert.AreEqual(mc.data.Length, 39);
            Assert.AreEqual(mc.ReadUInt(), 0x15); // type
            Assert.AreEqual(mc.ReadUInt(), 31); // size
            Assert.AreEqual(mc.ReadUInt(), 100001);
            Assert.AreEqual(mc.ReadUInt(), sHGG.Hash(pass, fakeSeed));
            Assert.AreEqual(mc.ReadUInt(), 0x14);
            mc.ReadUInt(); // gg ver
            Assert.AreEqual(mc.ReadByte(), 0x0); // unknown byte
            mc.ReadUInt(); // local ip
            mc.ReadShort(); // local port
            mc.ReadUInt(); // external ip
            mc.ReadShort(); // external port
            byte imageSize = mc.ReadByte();
            Assert.AreEqual(mc.ReadByte(), 0xbe); // unknown
            Assert.IsTrue(mc.IsEnd);
        }

        [Test]
        public void FakePingTest() {
            ConnectionMock mc = new ConnectionMock();
            sHGG mockGG = new sHGG(mc);
            mockGG.OutPing(new object(), EventArgs.Empty);
            Assert.AreEqual(mc.data.Length, 8);
            Assert.AreEqual(mc.ReadUInt(), 0x8);
            Assert.AreEqual(mc.ReadUInt(), 0);
            Assert.IsTrue(mc.IsEnd);
        }

        [Test]
        public void FakeStatusNDescTest() {
            ConnectionMock mc = new ConnectionMock();
            sHGG mockGG = new sHGG(mc);
            // available
            mockGG.GGStatus = GGStatusType.Available;
            Assert.AreEqual(mc.data.Length, 12);
            Assert.AreEqual(mc.ReadUInt(), 0x2); // type
            Assert.AreEqual(mc.ReadUInt(), 4); // size
            Assert.AreEqual(mc.ReadUInt(), 0x2); // status
            mc.ClearData();
            // busy
            mockGG.GGStatus = GGStatusType.Busy;
            Assert.AreEqual(mc.data.Length, 12);
            Assert.AreEqual(mc.ReadUInt(), 0x2); // type
            Assert.AreEqual(mc.ReadUInt(), 4); // size
            Assert.AreEqual(mc.ReadUInt(), 0x3); // status
            mc.ClearData();
            // + desc
            string desc = "abcdefghijk lmno";
            mockGG.GGDescription = desc;
            Assert.AreEqual(mc.ReadUInt(), 0x2); // type
            Assert.AreEqual(mc.ReadUInt(), 4 + 1 + desc.Length); // size
            Assert.AreEqual(mc.ReadUInt(), 0x5); // status
            Assert.AreEqual(mc.ReadByte(), Convert.ToByte('a'));
            Assert.AreEqual(mc.ReadByte(), Convert.ToByte('b'));
            Assert.AreEqual(mc.ReadByte(), Convert.ToByte('c'));
            Assert.AreEqual(mc.ReadByte(), Convert.ToByte('d'));
            Assert.AreEqual(mc.ReadByte(), Convert.ToByte('e'));
            Assert.AreEqual(mc.ReadByte(), Convert.ToByte('f'));
            Assert.AreEqual(mc.ReadByte(), Convert.ToByte('g'));
            Assert.AreEqual(mc.ReadByte(), Convert.ToByte('h'));
            Assert.AreEqual(mc.ReadByte(), Convert.ToByte('i'));
            Assert.AreEqual(mc.ReadByte(), Convert.ToByte('j'));
            Assert.AreEqual(mc.ReadByte(), Convert.ToByte('k'));
            Assert.AreEqual(mc.ReadByte(), Convert.ToByte(' '));
            Assert.AreEqual(mc.ReadByte(), Convert.ToByte('l'));
            Assert.AreEqual(mc.ReadByte(), Convert.ToByte('m'));
            Assert.AreEqual(mc.ReadByte(), Convert.ToByte('n'));
            Assert.AreEqual(mc.ReadByte(), Convert.ToByte('o'));
            Assert.AreEqual(mc.ReadByte(), 0);
            Assert.IsTrue(mc.IsEnd);
        }








    }
}
