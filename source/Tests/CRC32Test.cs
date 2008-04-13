/* CRC32Test.cs

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