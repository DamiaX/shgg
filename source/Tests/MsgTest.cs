/* MsgTest.cs

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
using System.Collections.Generic;
using NUnit.Framework;
using HAKGERSoft;

namespace HAKGERSoft.Tests {

    [TestFixture]
    [Ignore]
    [Obsolete]
    public class MsgTest {
        sHGG ggMock;

        [TestFixtureSetUp]
        public void StartupTest() {
            ggMock = new sHGG(new ConnectionMock());
            ggMock.GGNumber = "123456";
            ggMock.GGPassword = "abcdefg";
        }

        [TestFixtureTearDown]
        public void Dispose() {
            if (ggMock != null)
                ggMock.GGLogout();
        }

        [Test]
        public void RichFormatListTest() {
            SortedDictionary<short,string> format = new SortedDictionary<short,string>() { { 0,"<u>" }, { 3,"<b><i><gray>" }, { 6,"<orange>" } };
            int len;
            List<sHGG.stMsgRichFormat> output = ggMock.BuildRichText(ref format, out len);
            Assert.AreEqual(len, 15);
            Assert.IsNotNull(output);
            Assert.AreEqual(output.Count, 3);
            Assert.AreEqual(output[0].Position, 0);
            Assert.AreEqual(output[1].Position, 3);
            Assert.AreEqual(output[2].Position, 6);
            Assert.IsNull(output[0].RGB);
            Assert.IsNotNull(output[1].RGB);
            Assert.IsNotNull(output[2].RGB);
            Assert.AreEqual(output[0].Font, sHGG.FONT_UNDERLINE);
            Assert.AreEqual(output[1].Font, sHGG.FONT_ITALIC | sHGG.FONT_BOLD | sHGG.FONT_COLOR);
            Assert.AreEqual(output[2].Font, sHGG.FONT_NONE | sHGG.FONT_COLOR);
            // color test
            Assert.AreEqual(output[1].RGB, new byte[] { 128, 128, 128 });
        }





    }
}
