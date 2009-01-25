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
    public class UserListTest {
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

        [TearDown]
        public void ClearUsers() {
            ggMock.Users.Clear();
        }
       
        [Test]
        public void StartUpListTest() {
            Assert.IsNotNull(ggMock.Users);
            Assert.AreEqual(ggMock.Users.Count, 0);
        }

        [Test]
        public void SimpleAddRemoveUsersTest() {
            // add
            ggMock.Users.Add(54634);
            Assert.AreEqual(ggMock.Users.Count, 1);
            Assert.AreEqual(ggMock.Users[0].GGNumber, 54634);
            ggMock.Users.Add(new GGUser() { GGNumber = 1234, GGNick = "John" });
            ggMock.Users.Add(new GGUser() { GGNumber = 234, GGNick = "Mark", Email="abc@wp.pl" });
            Assert.AreEqual(ggMock.Users.Count, 3);
            Assert.IsFalse(ggMock.Users.Contains(null));
            Assert.IsTrue(ggMock.Users.Contains(234));
            Assert.IsFalse(ggMock.Users.Contains(11111111));
            Assert.IsTrue(ggMock.Users.Contains(new GGUser() { GGNumber=1234, GGNick="Different nick" }));
            Assert.IsFalse(ggMock.Users.Contains(new GGUser() { GGNumber = 222, GGNick = "John" }));
            GGUser found = ggMock.Users.Find(x => x.GGNick == "Mark");
            Assert.IsNotNull(found);
            Assert.AreEqual(found.GGNumber, 234);
            Assert.AreEqual(found.GGNick, "Mark");
            Assert.AreEqual(found.Email, "abc@wp.pl");

            // remove
            bool done = ggMock.Users.Remove(1234);
            Assert.IsTrue(done);
            bool fail = ggMock.Users.Remove(9999);
            Assert.IsFalse(fail);
            Assert.AreEqual(ggMock.Users.Count, 2);
            Assert.IsFalse(ggMock.Users.Contains(1234));
            Assert.IsFalse(ggMock.Users.Contains(new GGUser() { GGNick="Some", GGNumber=1234 }));
            Assert.IsTrue(ggMock.Users.Contains(234));
            Assert.IsTrue(ggMock.Users.Contains(new GGUser() { GGNick = "John", GGNumber = 54634 }));
            bool done2 = ggMock.Users.Remove(new GGUser() { GGNumber = 54634, GGNick = "??" });
            Assert.IsTrue(done2);
            Assert.AreEqual(ggMock.Users.Count, 1);
            Assert.IsFalse(ggMock.Users.Contains(54634));
            ggMock.Users.Clear();
            Assert.AreEqual(ggMock.Users.Count, 0);
        }

        [Test]
        public void AddCollectionListTest() {
            bool nullFail = ggMock.Users.Add(null as Dictionary<string,int>, false);
            Assert.IsFalse(nullFail);
            bool done = ggMock.Users.Add(new int[] { 123, 456, 789, 987, 7645, 234 }, false);
            Assert.IsTrue(done);
            Assert.AreEqual(ggMock.Users.Count, 6);
            Assert.IsTrue(ggMock.Users.Contains(987));
            Assert.IsTrue(ggMock.Users.Contains(123));
            Assert.IsFalse(ggMock.Users.Contains(9999));
            Assert.IsFalse(ggMock.Users.Contains(1111));
            bool done2 = ggMock.Users.Add(new Dictionary<string, int> { { "Paul", 222 }, { "John", 333 }, { "Tom", 444 } }, false);
            Assert.IsTrue(done2);
            Assert.AreEqual(ggMock.Users.Count, 9);
            Assert.IsTrue(ggMock.Users.Contains(333));
            Assert.IsTrue(ggMock.Users.Contains(444));
            Assert.IsFalse(ggMock.Users.Contains(555));
            GGUser paul = ggMock.Users.Find(x => x.GGNick == "Paul");
            Assert.IsNotNull(paul);
            Assert.AreEqual(paul.GGNick, "Paul");
            Assert.AreEqual(paul.GGNumber, 222);
            bool done3 = ggMock.Users.Add(new int[] { 7776, 456, 4554, 8776 }, false); // 456 x 2
            Assert.IsTrue(done3);
            Assert.AreEqual(ggMock.Users.Count, 12);
            Assert.IsTrue(ggMock.Users.Contains(456));
        }

        [Test]
        public void AddTransactionListTest() {
            bool done = ggMock.Users.Add(new int[] { 123, 456, 789, 987, 7645, 234 }, false);
            Assert.IsTrue(done);
            Assert.AreEqual(ggMock.Users.Count, 6);
            bool fail = ggMock.Users.Add(new int[] { 453453, 7645, 5, 345, 3435, 36243 }, true);
            Assert.IsFalse(fail);
            Assert.AreEqual(ggMock.Users.Count, 6);
            bool fail2 = ggMock.Users.Add(new int[] { 9988, 33449000,  9988, 7 }, true);
            Assert.IsFalse(fail2);
            Assert.AreEqual(ggMock.Users.Count, 6);
            bool done2 = ggMock.Users.Add(new Dictionary<string, int> { { "Paul", 333334 }, { "John", 444445 }, { "Tom", 555556 } }, true);
            Assert.IsTrue(done2);
            Assert.AreEqual(ggMock.Users.Count, 9);
            bool fail3 = ggMock.Users.Add(new Dictionary<string, int> { { "Monica", 888755 }, { "Tommy", 333334 } }, true);
            Assert.IsFalse(fail3);
            Assert.AreEqual(ggMock.Users.Count, 9);
        }

        [Test]
        public void BlockUserTest() {
            ggMock.Users.Add(new Dictionary<string, int> { { "Paul", 333334 }, { "John", 444445 }, { "Tom", 555556 } }, true);
            Assert.AreEqual(ggMock.Users.Count, 3);
            ggMock.Users.Block(444445);
            Assert.AreEqual(ggMock.Users.Count, 2);
            ggMock.Users.Block(99);
            Assert.AreEqual(ggMock.Users.Count, 2);
        }

        [Test]
        public void IndexerTest() {
            ggMock.Users.Add(new Dictionary<string, int> { { "Paul", 333334 }, { "John", 444445 }, { "Tom", 555556 } }, true);
            // List<T> this[int] standard, not override!
            // dictionary has no order, impossible to test - but it works! Even so, there is sort ....
            //Assert.AreEqual(ggMock.Users[(int)1].GGNumber, 444445);
            //Assert.AreEqual(ggMock.Users[(int)1].GGNick, "John");
            // this[uint ggNumber]
            Assert.AreEqual(ggMock.Users[(uint)555556].GGNumber, 555556);
            Assert.AreEqual(ggMock.Users[(uint)555556].GGNick, "Tom");
            // this[string nick]
            Assert.AreEqual(ggMock.Users["Paul"].GGNumber, 333334);
            Assert.AreEqual(ggMock.Users["Paul"].GGNick, "Paul");

            Assert.IsNotNull(ggMock.Users[(int)1]); // List<T> this[int index] test - will throw if not working

        }


    }
}
