/* BasicTest.cs

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
    public class BasicTest {
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
        public void BasicStartTest() {
            Assert.AreEqual(ggMock.GGNumber, "123456");
            Assert.AreEqual(ggMock.GGPassword, "abcdefg");
            Assert.IsFalse(ggMock.IsGGLogged);
            Assert.IsTrue(ggMock.Users.Count == 0);
            Assert.AreEqual(ggMock.GGDescription, string.Empty);
            Assert.AreEqual(ggMock.GGStatus, GGStatusType.NotAvailable);
        }

        [Test]
        public void StatusDescTest() {
            ggMock.GGStatus = GGStatusType.Busy;
            ggMock.GGDescription = @"test123ąś$#%^&*(&^):L}{,./<>?ć";
            Assert.AreEqual(ggMock.GGStatus, GGStatusType.Busy);
            Assert.AreEqual(ggMock.GGDescription, @"test123ąś$#%^&*(&^):L}{,./<>?ć");
            ggMock.GGStatus = GGStatusType.NotAvailable;
            ggMock.GGDescription = string.Empty;
            Assert.AreEqual(ggMock.GGStatus, GGStatusType.NotAvailable);
            Assert.AreEqual(ggMock.GGDescription, string.Empty);
        }




    }
}
