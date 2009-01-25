/* 
 * SHGG
 * More info in SHGG.cs file 
 * 
*/

using System;
using System.IO;

namespace HAKGERSoft {
 
   internal class CRC32 {      
      UInt32[] crc32Table;
      const int BUFFER_SIZE = 1024;

      internal CRC32() {
         unchecked {
            UInt32 dwPolynomial = 0xedb88320;
            UInt32 i, j;
            crc32Table = new UInt32[256];
            UInt32 dwCrc;
            for(i=0; i<256; i++) {
               dwCrc = i;
               for(j=8; j>0; j--) {
                  if ((dwCrc & 1) == 1)
                     dwCrc = (dwCrc >> 1) ^ dwPolynomial;
                  else
                     dwCrc >>= 1;
               }
               crc32Table[i] = dwCrc;
            }
         }
      }

      internal UInt32 GetCrc32(Stream stream) {
         unchecked {
            UInt32 crc32Result = 0xffffffff;
            byte[] buffer = new byte[BUFFER_SIZE];
            int readSize = BUFFER_SIZE;
            stream.Position = 0;
            int count = stream.Read(buffer, 0, readSize);
            while (count > 0) {
               for (int i=0; i<count; i++)
                  crc32Result = (crc32Result >> 8) ^ crc32Table[(buffer[i]) ^ (crc32Result & 0x000000ff)];
               count = stream.Read(buffer, 0, readSize);
            }
            return ~crc32Result;
         }
      }



   }
}