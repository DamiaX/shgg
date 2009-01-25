/* 
 * SHGG
 * More info in SHGG.cs file 
 * 
*/

using System;
using System.IO;
using NUnit.Framework;
using HAKGERSoft;

namespace HAKGERSoft.Tests {

    [TestFixture]
    public class CRC32Test {

        [Test]
        public void SimpleCRC32Test() {
            string mockObj = @"SABNFGTREJ32435457#@$^%&&([p]\[]::<>?<ASCZXNOLółźćżą111"; // crc32=2259372540
            CRC32 crc = new CRC32();
            Stream str = new MemoryStream(sHGG.StrToByteArray(mockObj));
            uint crc32 = crc.GetCrc32(str);
            Assert.IsNotNull(crc32);
            Assert.AreEqual(crc32, 2259372540);
        }




    }
}