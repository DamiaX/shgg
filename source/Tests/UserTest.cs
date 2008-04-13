/* UserTest.cs

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
    public class UserTest {

        [Test]
        public void SimpleUserTest() {
            GGUser user1 = new GGUser() { GGNick = "John", GGNumber = 234567, City = "NYC" };
            GGUser user2 = new GGUser() { GGNick = "John", GGNumber = 98765, City = "Los Angeles" };
            GGUser user3 = new GGUser() { GGNick = "Mark", GGNumber = 234567 };
            GGUser user4 = new GGUser() { GGNick = "Monica", GGNumber = 98765 };
            GGUser user5 = new GGUser() { GGNick = "Monica", GGNumber = 123 };

            Assert.AreEqual(user1, user3);
            Assert.AreEqual(user2, user4);
            Assert.AreNotEqual(user1, user2);
            Assert.AreNotEqual(user2, user5);
            Assert.AreNotEqual(user4, user5);
        }

        [Test]
        public void UserShallowCopyTest() {
            GGUser user = new GGUser() { GGNick = "Monica", GGNumber = 123 };
            GGUser copy = user.Clone() as GGUser;
            Assert.IsNotNull(copy);
            Assert.AreEqual(copy, user);
            Assert.AreNotSame(copy, user);

        }

 

    }
}
