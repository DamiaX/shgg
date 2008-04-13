/* MockTest.cs

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
using System.Runtime.InteropServices;
using NUnit.Framework;
using HAKGERSoft;

namespace HAKGERSoft.Tests {

    [TestFixture]
    public class MockTest {

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        struct basic {
            internal byte one;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        struct simpleStruct {
            internal uint uNumber;
            internal short ShortNumber;
            internal byte SampleFlag;
            internal ushort SampleInt16;
            internal byte SampleFlag2;
            internal int Number;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        struct UnmanagedStruct {
            internal uint Number;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            internal byte[] ByteArr;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10 + 1)]
            internal string Str;
        }

        [Test]
        public void MockCtorTest() {
            sHGG ggNormal = new sHGG();
            Assert.IsFalse(ggNormal.mock);
            Assert.IsNull(ggNormal.mockObj);
            ConnectionMock mock = new ConnectionMock();
            sHGG ggMock = new sHGG(mock);
            Assert.IsTrue(ggMock.mock);
            Assert.IsNotNull(ggMock.mockObj);
            Assert.AreEqual(ggMock.mockObj, mock);
            Assert.AreSame(ggMock.mockObj, mock);      
        }

        [Test]
        public void FullMockTest() {
            ConnectionMock mock = new ConnectionMock();
            Assert.IsNotNull(mock);
            Assert.IsNotNull(mock.data);
            Assert.AreEqual(mock.data.Length, 0);
            byte[] pack1 = new byte[] { 1, 2, 3, 4, 5, 6, 7 };
            mock.Write(pack1);
            Assert.AreEqual(mock.data.Length, 7);
            Assert.AreEqual(mock.data[0], 1);
            Assert.AreEqual(mock.data[3], 4);
            Assert.AreEqual(mock.data[6], 7);
            byte[] pack2 = new byte[] { 9, 8, 7, 6, 5 };
            mock.Write(pack2);
            Assert.AreEqual(mock.data.Length, 12);
            Assert.AreEqual(mock.data[0], 1);
            Assert.AreEqual(mock.data[6], 7);
            Assert.AreEqual(mock.data[7], 9);
            Assert.AreEqual(mock.data[11], 5);
            byte[] pack3 = new byte[] { 0, 1, 2, 3 };
            mock.Write(pack3, 0, 2);
            Assert.AreEqual(mock.data.Length, 14);
            Assert.AreEqual(mock.data[0], 1);
            Assert.AreEqual(mock.data[6], 7);
            Assert.AreEqual(mock.data[7], 9);
            Assert.AreEqual(mock.data[11], 5);
            Assert.AreEqual(mock.data[12], 0);
            Assert.AreEqual(mock.data[13], 1);
            mock.ClearData();
            Assert.IsNotNull(mock.data);
            Assert.AreEqual(mock.data.Length, 0);
            byte[] pack4 = new byte[] { 1, 2, 3, 4, 5 };
            mock.Write(pack4, 0, 1);
            Assert.AreEqual(mock.data.Length, 1);
            Assert.AreEqual(mock.data[0], 1);
            mock.ClearData();
            Assert.IsNotNull(mock.data);
            Assert.AreEqual(mock.data.Length, 0);
        }

        [Test]
        public void SimpleReadMockTest() {
            ConnectionMock mock = new ConnectionMock();
            simpleStruct str = new simpleStruct() {
                Number = 9876,
                SampleFlag = 0x5,
                SampleFlag2 = 0xf,
                SampleInt16 = (UInt16)2001,
                ShortNumber = 1998,
                uNumber = 1000234
            };
            mock.Write(sHGG.RawSerialize(str));
            Assert.AreEqual(mock.data.Length, 14);
            Assert.AreEqual(mock.ReadUInt(), 1000234);
            Assert.AreEqual(mock.ReadShort(), 1998);
            Assert.AreEqual(mock.ReadByte(), 0x5);
            Assert.AreEqual(mock.ReadShort(), 2001); //todo
            Assert.AreEqual(mock.ReadByte(), 0xf);
            Assert.AreEqual(mock.ReadInt(), 9876);
        }

        [Test]
        public void UnmanagedReadMockTest() {
            ConnectionMock mock = new ConnectionMock();
            UnmanagedStruct ustr = new UnmanagedStruct() {
                ByteArr = new byte[] {3, 5, 7},
                Number = UInt32.MaxValue,
                Str = "Unmanaged!"
            };
            mock.Write(sHGG.RawSerialize(ustr));
            Assert.AreEqual(mock.data.Length, 18);
            Assert.AreEqual(mock.ReadUInt(), UInt32.MaxValue);
            // byte[]
            Assert.AreEqual(mock.ReadByte(), 3);
            Assert.AreEqual(mock.ReadByte(), 5);
            Assert.AreEqual(mock.ReadByte(), 7);
            // string + 0 char
            Assert.AreEqual(mock.ReadByte(), Convert.ToByte('U'));
            Assert.AreEqual(mock.ReadByte(), Convert.ToByte('n'));
            Assert.AreEqual(mock.ReadByte(), Convert.ToByte('m'));
            Assert.AreEqual(mock.ReadByte(), Convert.ToByte('a'));
            Assert.AreEqual(mock.ReadByte(), Convert.ToByte('n'));
            Assert.AreEqual(mock.ReadByte(), Convert.ToByte('a'));
            Assert.AreEqual(mock.ReadByte(), Convert.ToByte('g'));
            Assert.AreEqual(mock.ReadByte(), Convert.ToByte('e'));
            Assert.AreEqual(mock.ReadByte(), Convert.ToByte('d'));
            Assert.AreEqual(mock.ReadByte(), Convert.ToByte('!'));
            Assert.AreEqual(mock.ReadByte(), 0);
        }

        [Test]
        [ExpectedException(typeof(OverflowException))]
        public void ThrowOverflowTest() {
            basic bstr = new basic() { one = 0xe };
            ConnectionMock mock = new ConnectionMock();
            mock.Write(sHGG.RawSerialize(bstr));
            byte oneRe = mock.ReadByte();
            Assert.AreEqual(oneRe, bstr.one);
            mock.ReadByte(); // throw
        }


    }
}
