/* GGExceptions.cs

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
using System.Runtime.Serialization;

namespace HAKGERSoft {

    [Serializable()]
    public class sHGGException : ApplicationException {
        public sHGGException() :
            base() {
        }

        public sHGGException(string message) :
            base(message) {
        }

        public sHGGException(string message,  System.Exception inner) :
            base(message, inner) {
        }

        protected sHGGException(SerializationInfo info, StreamingContext context) : 
            base(info, context) { 
        }
    }
}