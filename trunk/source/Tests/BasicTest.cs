/* 
 * SHGG
 * More info in SHGG.cs file 
 * 
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
