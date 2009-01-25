/* 
 * SHGG
 * More info in SHGG.cs file 
 * 
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