/* GGUser.cs

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

namespace HAKGERSoft {
    
    /// <summary>
    /// Osoba na liście kontaktów
    /// </summary>
    public sealed class GGUser : IComparable, ICloneable, IEquatable<GGUser> {

        /// <summary>
        /// Numer gadu-gadu
        /// </summary>
        public int GGNumber = 0;

        /// <summary>
        /// Nick osoby
        /// </summary>
        public string GGNick = string.Empty;

        /// <summary>
        /// Imię osoby
        /// </summary>
        public string Name = string.Empty;

        /// <summary>
        /// Nazwisko osoby
        /// </summary>
        public string LastName = string.Empty;

        /// <summary>
        /// Adres e-mail
        /// </summary>
        public string Email = string.Empty;

        /// <summary>
        /// Numer telefonu
        /// </summary>
        public string Phone = string.Empty;

        /// <summary>
        /// Numer telefonu komórkowego
        /// </summary>
        public string Mobile = string.Empty;

        /// <summary>
        /// Rok urodzenia
        /// </summary>
        public int BirthYear = 0;

        /// <summary>
        /// Miasto zamieszkania
        /// </summary>
        public string City = string.Empty;

        /// <summary>
        /// Miasto rodzinne
        /// </summary>
        public string FamilyCity = string.Empty;

        /// <summary>
        /// Nazwisko panieńskie
        /// </summary>
        public string FamilyName = string.Empty;

        /// <summary>
        /// Jeżeli pole ma wartość FALSE, to serwer ukrywa twój status
        /// przy włączonym trybie FriendsMask (standardowo TRUE)
        /// </summary>
        public bool Friend = true;

        /// <summary>
        /// Aktualny status osoby (tylko do odczytu)
        /// </summary>
        public GGStatusType GGStatus {
            get { return vGGStatus; }
        }

        internal GGStatusType vGGStatus = GGStatusType.NotAvailable;

        /// <summary>
        /// Aktualny opis osoby (tylko do odczytu)
        /// </summary>
        public string Description {
            get { return vDescription; }
        }

        internal string vDescription = string.Empty;

        /// <summary>
        /// Adres IP osoby (tylko do odczytu)
        /// </summary>
        public string IPAdress {
            get { return vIPAdress; }
        }

        internal string vIPAdress = string.Empty;

        /// <summary>
        /// Port klienta GG osoby (tylko do odczytu)
        /// </summary>
        public int RemotePort {
            get { return vRemotePort; }
        }

        internal int vRemotePort = 0;

        /// <summary>
        /// Maksymalna wielkość otrzymywanego obrazka (tylko do odczytu)
        /// </summary>
        public int MaxImageSize {
            get { return vMaxImageSize; }
        }

        internal int vMaxImageSize = 0;

        /// <summary>
        /// Informacja o wersji klienta GG osoby (tylko do odczytu)
        /// </summary>
        public string GGClientVersion {
            get { return vGGClientVersion; }
        }

        internal string vGGClientVersion = string.Empty;

        public int CompareTo(object value) {
            if (value == null)
                return 1;
            if (!(value is GGUser))
                throw new ArgumentException("Argument nie jest typu HAKGERSoft.GGUser");
            return (this.GGNumber - (value as GGUser).GGNumber); // ten sam numer GG
        }

        public object Clone() {
            return base.MemberwiseClone();
        }

        public bool Equals(GGUser value) {
            if (value==null && this!=null)
                return false;
            return (value.GGNumber == this.GGNumber); // ten sam numer GG
        }

        public override bool Equals(object o) {
            if (o is GGUser && this!=null)
                return this.Equals((GGUser)o);
            return false;
        }

        public override int GetHashCode() {
            return this.GGNumber;
        }
    }
}