/* 
 * SHGG
 * More info in SHGG.cs file 
 * 
*/

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using HAKGERSoft;

namespace HAKGERSoft.Tests {

    [TestFixture]
    public class FunctionsTest {
     
        [Test]
        public void SplitPacketTest() {
            Assert.AreEqual(sHGG.SplitPacket(1000, 400), 799);
            Assert.AreEqual(sHGG.SplitPacket(400, 200), 199);
        }

        [Test]
        public void RawSerializeTest() {
            byte[] int64Fake = sHGG.RawSerialize(new Int64());
            Assert.IsNotNull(int64Fake);
            Assert.AreEqual(int64Fake.Length, 8);
        }

        [Test]
        public void PassHashTest() {
            string pass = "abcdef";
            uint randonSeed = 2274271960;
            long hash = sHGG.Hash(pass, randonSeed);
            Assert.IsNotNull(hash);
            Assert.AreEqual(hash, 3680342896);
        }

        [Test]
        public void GGStatusEncodingTest() {
            Assert.AreEqual(sHGG.StatusCode(GGStatusType.Available, string.Empty), sHGG.STATUS_AVAILABLE);
            Assert.AreEqual(sHGG.StatusCode(GGStatusType.Available, "a"), sHGG.STATUS_AVAILABLE_DESC);
            Assert.AreEqual(sHGG.StatusCode(GGStatusType.Busy, ""), sHGG.STATUS_BUSY);
            Assert.AreEqual(sHGG.StatusCode(GGStatusType.Busy, "desc"), sHGG.STATUS_BUSY_DESC);
            Assert.AreEqual(sHGG.StatusCode(GGStatusType.NotAvailable, ""), sHGG.STATUS_NOT_AVAILABLE);
            Assert.AreEqual(sHGG.StatusCode(GGStatusType.NotAvailable, "$#%&"), sHGG.STATUS_NOT_AVAILABLE_DESC);
            Assert.AreEqual(sHGG.StatusCode(GGStatusType.Invisible, ""), sHGG.STATUS_INVISIBLE);
            Assert.AreEqual(sHGG.StatusCode(GGStatusType.Invisible, "1"), sHGG.STATUS_INVISIBLE_DESC);

            Assert.AreEqual(sHGG.StatusDecode(sHGG.STATUS_AVAILABLE), GGStatusType.Available);
            Assert.AreEqual(sHGG.StatusDecode(sHGG.STATUS_AVAILABLE_DESC), GGStatusType.Available);
            Assert.AreEqual(sHGG.StatusDecode(sHGG.STATUS_BUSY), GGStatusType.Busy);
            Assert.AreEqual(sHGG.StatusDecode(sHGG.STATUS_BUSY_DESC), GGStatusType.Busy);
            Assert.AreEqual(sHGG.StatusDecode(sHGG.STATUS_INVISIBLE), GGStatusType.Invisible);
            Assert.AreEqual(sHGG.StatusDecode(sHGG.STATUS_INVISIBLE_DESC), GGStatusType.Invisible);
            Assert.AreEqual(sHGG.StatusDecode(sHGG.STATUS_BLOCKED), GGStatusType.Blocked);
            Assert.AreEqual(sHGG.StatusDecode(sHGG.STATUS_NOT_AVAILABLE), GGStatusType.NotAvailable);
            Assert.AreEqual(sHGG.StatusDecode(sHGG.STATUS_NOT_AVAILABLE_DESC), GGStatusType.NotAvailable);
            Assert.AreEqual(sHGG.StatusDecode(10004), GGStatusType.NotAvailable); // switch default
        }

        [Test]
        public void HttpHashTest() {
            string[] hashParams = new string[] { "lkrs@poczta.onet.pl", "123456a" };
            int hash = sHGG.HttpHash(hashParams);
            Assert.IsNotNull(hash);
            Assert.AreEqual(hash, 828141457);
        }

        [Test]
        public void ByteArray2StrEncodingTest() {
            string testStr = @"123400abcdefąśćźżńół[]\ ;$#^ %$% &&#@@@#$&&^(@!!!@###";
            int testLen = testStr.Length;
            byte[] testArr = sHGG.StrToByteArray(testStr);
            Assert.IsNotNull(testArr);
            Assert.AreEqual(testArr.Length, testLen);
            Assert.AreEqual(testArr[1], Convert.ToByte('2')); // second
            Assert.AreEqual(testArr[testArr.Length - 1], Convert.ToByte('#')); // last
            string revert = sHGG.ByteArray2Str(testArr);
            Assert.IsFalse(string.IsNullOrEmpty(revert));
            Assert.AreEqual(revert.Length, testLen);
            Assert.AreEqual(revert[1], Convert.ToByte('2')); // second
            Assert.AreEqual(revert[revert.Length - 1], Convert.ToByte('#')); // last
            Assert.AreEqual(revert, testStr);
        }

        [Test]
        public void ToArrayFuncTest() {
            List<string> mockList = new List<string>() { "123", "456", "789" };
            string[] nArray = sHGG.ToArray<string>(mockList);
            Assert.AreEqual(nArray.Length, 3);
            Assert.AreEqual(nArray[0], "123");
            Assert.AreEqual(nArray[1], "456");
            Assert.AreEqual(nArray[2], "789");
            List<string> empty = new List<string>();
            string[] emptyArray = sHGG.ToArray<string>(empty);
            Assert.AreEqual(emptyArray.Length, 0);
        }

        [Test]
        public void ArrayChopTest() {
            string testStr = "bcdefghijk";
            byte[] bin = sHGG.StrToByteArray(testStr);
            byte[][] listLow = sHGG.ArrayChop<byte>(bin, 20); // {10}
            byte[][] list1 = sHGG.ArrayChop<byte>(bin, 10); // {10}
            byte[][] list2 = sHGG.ArrayChop<byte>(bin, 9); // {9, 1}
            byte[][] list3 = sHGG.ArrayChop<byte>(bin, 6); // {6, 4}
            byte[][] list4 = sHGG.ArrayChop<byte>(bin, 5); // {5, 5}
            byte[][] list5 = sHGG.ArrayChop<byte>(bin, 3); // {3, 3, 3, 1}

            Assert.IsNotNull(listLow);
            Assert.IsNotNull(list1);
            Assert.IsNotNull(list2);
            Assert.IsNotNull(list3);
            Assert.IsNotNull(list4);
            Assert.IsNotNull(list5);
            // first dim. length
            Assert.AreEqual(listLow.Length, 1);
            Assert.AreEqual(list1.Length, 1);
            Assert.AreEqual(list2.Length, 2);
            Assert.AreEqual(list3.Length, 2);
            Assert.AreEqual(list4.Length, 2);
            Assert.AreEqual(list5.Length, 4);
            // second dim. length
            Assert.AreEqual(listLow[0].Length, 10);
            Assert.AreEqual(list1[0].Length, 10);
            Assert.AreEqual(list2[0].Length, 9);
            Assert.AreEqual(list2[1].Length, 1);
            Assert.AreEqual(list3[0].Length, 6);
            Assert.AreEqual(list3[1].Length, 4);
            Assert.AreEqual(list4[0].Length, 5);
            Assert.AreEqual(list4[1].Length, 5);
            Assert.AreEqual(list5[0].Length, 3);
            Assert.AreEqual(list5[1].Length, 3);
            Assert.AreEqual(list5[2].Length, 3);
            Assert.AreEqual(list5[3].Length, 1);
            // random values
            Assert.AreEqual(listLow[0][0], Convert.ToByte('b'));
            Assert.AreEqual(listLow[0][3], Convert.ToByte('e'));
            Assert.AreEqual(listLow[0][9], Convert.ToByte('k'));
            Assert.AreEqual(list5[3][0], Convert.ToByte('k'));
            Assert.AreEqual(list5[2][2], Convert.ToByte('j'));
            Assert.AreEqual(list2[0][0], Convert.ToByte('b'));
            Assert.AreEqual(list3[1][2], Convert.ToByte('j'));
            Assert.AreEqual(list1[0][5], Convert.ToByte('g'));
            Assert.AreEqual(list4[1][0], Convert.ToByte('g'));
        }

        [Test]
        public void ImageBinBuffTest() {
            BinBuffer<uint, byte[]> imageBuff = new BinBuffer<uint, byte[]>();
            imageBuff.pushSave(1234, new byte[] { 1, 2, 3, 4 });
            imageBuff.pushSave(5678, new byte[] { 5, 6, 7, 8 });

            byte[] expectNull = imageBuff.popSave(0000);
            Assert.IsNull(expectNull);
            byte[] firstArr = imageBuff.popSave(1234);
            Assert.IsNotNull(firstArr);
            Assert.AreEqual(firstArr.Length, 4);
            Assert.AreEqual(firstArr[3], 4);
            byte[] firstAnother = imageBuff.popSave(1234);
            Assert.IsNull(firstAnother);
            imageBuff.pushSave(1234, new byte[] { 9 });
            byte[] firstNew = imageBuff.popSave(1234);
            Assert.IsNotNull(firstArr);
            Assert.AreEqual(firstNew.Length, 1);
            Assert.AreEqual(firstNew[0], 9);
            byte[] expectNull2 = imageBuff.popSave(1234);
            Assert.IsNull(expectNull2);
            byte[] secArr = imageBuff.popSave(5678);
            Assert.IsNotNull(secArr);
            Assert.AreEqual(secArr.Length, 4);
            Assert.AreEqual(secArr[3], 8);
            byte[] expectNull3 = imageBuff.popSave(5678);
            Assert.IsNull(expectNull3);
        }

        [Test]
        public void ConcatArrayTest() {
            byte[] arr1 = new byte[3] { 1, 2, 3 };
            byte[] arr2 = new byte[2] { 4, 5 };
            byte[] res1 = sHGG.ConcatArray<byte>(arr1, arr2);
            Assert.IsNotNull(res1);
            Assert.AreEqual(res1.Length, 5);
            Assert.AreEqual(res1[0], 1);
            Assert.AreEqual(res1[1], 2);
            Assert.AreEqual(res1[2], 3);
            Assert.AreEqual(res1[3], 4);
            Assert.AreEqual(res1[4], 5);
            arr2 = sHGG.ConcatArray<byte>(arr2, arr1);
            Assert.IsNotNull(arr2);
            Assert.AreEqual(arr2.Length, 5);
            Assert.AreEqual(arr2[0], 4);
            Assert.AreEqual(arr2[1], 5);
            Assert.AreEqual(arr2[2], 1);
            Assert.AreEqual(arr2[3], 2);
            Assert.AreEqual(arr2[4], 3);
        }



    }
}
