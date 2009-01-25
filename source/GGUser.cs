/* 
 * SHGG
 * More info in SHGG.cs file 
 * 
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

        //ctor

        public GGUser() {
        }

        public GGUser(uint ggNumber) {
            this.GGNumber = (int)ggNumber;
        }

        public GGUser(int ggNumber) : this((uint)ggNumber) {
        }

        public GGUser(string nick) {
            this.GGNick = nick;
        }

        public GGUser(string nick, int ggNumber) {
            this.GGNick = nick;
            this.GGNumber = ggNumber;
        }

        public GGUser(int ggNumber, string nick): this(nick,ggNumber) {
        }

        //operator

        public static implicit operator int(GGUser m) {
            return m.GGNumber;
        }

        public static implicit operator GGUser(int i) {
            return new GGUser(i);
        }

        public static implicit operator string(GGUser m) {
            return m.GGNick;
        }

        public static implicit operator GGUser(string s) {
            return new GGUser(s);
        }

        #region IComparable Members

        public int CompareTo(object value) {
            if (value == null)
                return 1;
            if (!(value is GGUser))
                throw new ArgumentException("Argument nie jest typu HAKGERSoft.GGUser");
            return (this.GGNumber - (value as GGUser).GGNumber); // ten sam numer GG
        }

        #endregion

        #region ICloneable Members

        public object Clone() {
            return base.MemberwiseClone();
        }

        #endregion

        #region IEquatable<GGUser> Members

        public bool Equals(GGUser value) {
            if (value==null && this!=null)
                return false;
            return (value.GGNumber == this.GGNumber); // ten sam numer GG
        }

        #endregion

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