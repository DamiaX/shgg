/* SHGG.cs

Copyright (c) HAKGERSoft 2000 - 2008 www.hakger.xorg.pl

This unit is owned by HAKGERSoft, any modifications without HAKGERSoft permission
are prohibited!

Author:
  DetoX [ reedi(at)poczta(dot)fm ]

Unit description:
  .NET GG engine based on System.Net.Sockets.TcpClient

Requirements:
  .NET 3.5 (at least) to develop
 
Version:
  0.6 / 13.04.2008

Remarks:
  For questions use www.4programmers.net
*/

using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;

namespace HAKGERSoft {

    /// <summary>
    /// G³ówna klasa obs³uguj¹ca silnik klienta gadu-gadu
    /// </summary>
    public sealed partial class sHGG {
        internal bool mock {
            get { return this.mockObj != null; }
        }
        internal ConnectionMock mockObj;
        private System.Net.Sockets.TcpClient TcpEngine = null;
        private Thread IdleEngine;
        private System.Net.Sockets.NetworkStream NetStream;
        private System.Windows.Forms.Timer Timer;
        private SynchronizationContext syncContext = SynchronizationContext.Current;

        /// <summary>
        /// Lista kontaktów
        /// </summary>
        public GGUsers Users;

        # region Properties

        /// <summary>
        /// <value>Okreœla adres IP serwera gadu-gadu</value>
        /// </summary>
        public string GGServerAddress {
            get { return vGGServerAddress; }
            set { vGGServerAddress = value; }
        }

        private string vGGServerAddress = DEFAULT_GG_HOST;

        /// <summary>
        /// <value>Okreœla numer gadu-gadu klienta (twój numer GG)</value>
        /// </summary>
        public string GGNumber {
            get { return vGGNumber; }
            set { vGGNumber = value; }
        }

        private string vGGNumber = "0";

        /// <summary>
        /// <value>Okreœla has³o klienta gadu-gadu (twoje has³o)</value>
        /// </summary>
        public string GGPassword {
            get { return vGGPassword; }
            set { vGGPassword = value; }
        }

        private string vGGPassword = string.Empty;

        /// <summary>
        /// <value>Okreœla status gadu-gadu</value>
        /// </summary>
        public GGStatusType GGStatus {
            get { return vGGStatus; }
            set {
                vGGStatus = value;
                if (this.IsGGLogged || this.mock)
                    this.OutStatus();  
            }  
        }

        private GGStatusType vGGStatus = GGStatusType.NotAvailable;

        /// <summary>
        /// <value>Okreœla opis klienta gadu-gadu</value>
        /// </summary>
        public string GGDescription {
            get { return vGGDescription; }
            set { 
                vGGDescription = value;
                if (vGGDescription.Length > MAX_DESCRIPTIONS_SIZE)
                    vGGDescription = vGGDescription.Substring(0, MAX_DESCRIPTIONS_SIZE);
                if (this.IsGGLogged || this.mock)
                    GGStatus = vGGStatus;
            }
        }

        private string vGGDescription = string.Empty;

        /// <summary>
        /// <vale>Jeœli w³aœciwoœæ ma wartoœæ TRUE to w³¹czony jest
        /// tryb "Pokazuj status tylko znajomym"</vale>
        /// </summary>
        public bool GGFriendsMask {
            get { return vGGFriendsMask; }
            set {
                vGGFriendsMask = value;
                if (this.IsGGLogged || this.mock)
                    GGStatus = vGGStatus;
            }
        }

        private bool vGGFriendsMask = false;

        /// <summary>
        /// <value>Pozwala okreœliæ maksymaln¹ wielkoœæ obrazka</value>
        /// </summary>
        public byte GGImageSize {
            get { return vGGImageSize; }
            set { vGGImageSize = value; } // todo: re login?
        }

        private byte vGGImageSize = (byte)255;

        /// <summary>
        /// <value>W³aœciwoœæ jednoznacznie okreœla czy klient jest zalogowany
        /// w danej chwili do serwera gadu-gadu (tylko do odczytu)</value>
        /// </summary>
        public bool IsGGLogged {
            get { return vIsGGLogged; }
            private set { vIsGGLogged = value; } 
        }
        
        private bool vIsGGLogged = false;

        /// <summary>
        /// W³aœciwoœæ okreœla czy mo¿na szukaæ dalej w katalogu publicznym (tylko do odczytu)
        /// </summary>
        public bool PubDirCanSearchMore {
            get { return vNextStart != 0; }
        }

        # endregion

        # region Events

        public delegate void GenericEventHandler<A>(object sender, A args);
        
        /// <summary>
        /// Zdarzenie wykonuje siê w memencie zalogowania do serwera
        /// </summary>
        public event GenericEventHandler<EventArgs> GGLogged;

        /// <summary>
        /// Zdarzenie wykonuje siê w przypadku nieudanej próby zalogowania do serwera
        /// </summary>
        public event GenericEventHandler<EventArgs> GGLogFailed;

        /// <summary>
        /// Zdarzenie wykonuje siê w przypadku, gdy serwer gadu-gadu roz³¹czy klienta
        /// </summary>
        public event GenericEventHandler<EventArgs> GGDisconnected;

        /// <summary>
        /// Zdarzenie wykonuje siê gdy przysz³a nowa wiadomoœæ
        /// </summary>
        public event GenericEventHandler<MessageReceiveEventArgs> GGMessageReceive;
               
        public class MessageReceiveEventArgs : EventArgs {
            /// <summary>
            /// numer GG nadawcy wiadomoœci 
            /// </summary>
            public int Number;

            /// <summary>
            /// treœæ wiadomoœci
            /// </summary>
            public string Message;

            /// <summary>
            /// czas wys³ania wiadomoœci
            /// </summary>
            public DateTime Time;
        }

        /// <summary>
        /// Zdarzenie informuje o wynikach szukania osób w katalogu publicznym
        /// </summary>
        public event GenericEventHandler<SearchReplyEventArgs> GGSearchReply;

        public class SearchReplyEventArgs : EventArgs {
            /// <summary>
            /// znalezione osoby
            /// </summary>
            public List<GGUser> Users;
        }

        /// <summary>
        /// Zdarzenie wykonuje siê gdy odczytano w³asne informacje w katalogu publicznym
        /// </summary>
        public event GenericEventHandler<PubDirReadReplyEventArgs> GGPubDirReadReply;

        public class PubDirReadReplyEventArgs : EventArgs {
            /// <summary>
            /// w³asne informacje w katalogu publicznym
            /// </summary>
            public GGUser User;
        }

        ///// <summary>
        ///// Zdarzenie wykonuje siê w przypadku pomyœlnego zapisania danych w katalogu publicznym
        ///// </summary>
        //public event GenericEventHandler<EventArgs> GGPubDirWriteReply;

        # endregion

        /// <summary>
        /// Konstruktor klasy sHGG
        /// </summary>
        public sHGG(): base() {
            syncContext = syncContext ?? new SynchronizationContext();
            this.Users = new GGUsers(this);
        }

        internal sHGG(ConnectionMock mockObj): this() {
            this.mockObj = mockObj;
        }

        /// <summary>
        /// Loguje siê do serwera gadu-gadu (domyœlny adres serwera GG)
        /// </summary>
        public void GGLogin() {
            GGLogin(GGServerAddress);
        }

        /// <summary>
        /// Loguje siê do serwera gadu-gadu (dowolny adres serwera GG)
        /// </summary>
        /// <param name="serverAddress"></param>
        public void GGLogin(string serverAddress) {
            OutConnect(serverAddress);
        }

        /// <summary>
        /// Metoda wylogowuje z serwera gadu-gadu (roz³¹cza)
        /// </summary>
        public void GGLogout() {
            if (this.IsGGLogged) {
                IdleEngine.Abort();
                TcpEngine.Close();
                IsGGLogged = false;
                NetStream.Close();
                this.Users.UsersRestart();
           }
        }

        /// <summary>
        /// Funkcja zwraca aktualnie czynny adres IP serwera gadu-gadu lub pusty
        /// string w przypadku, gdy ¿aden nie jest dostêpny juz operacja nie
        /// powiedzie siê
        /// </summary>
        /// <returns>adres IP serwera</returns>
        public string GGGetActiveServer() {
            return HttpGGGetActiveServer() ?? string.Empty;
        }

        /// <summary>
        /// Funkcja generuje token gadu-gadu
        /// </summary>
        /// <param name="tokenId">id (numer) tokenu</param>
        /// <param name="tokenUrl">pe³en adres tokenu</param>
        /// <returns>obrazek z tokenem</returns>
        public Image GGGetToken(out string tokenId, out string tokenUrl) {
            return HttpGetToken(out tokenId, out tokenUrl);
        }

        /// <summary>
        /// Funkcja generuje token gadu-gadu
        /// </summary>
        /// <returns>obrazek z tokenem</returns>
        public Image GGGetToken() {
            string tokenId, tokenUrl;
            return HttpGetToken(out tokenId, out tokenUrl);
        }

        /// <summary>
        /// Rejestruje nowego u¿ytkownika gadu-gadu
        /// </summary>
        /// <param name="email">adres e-mail u¿ytkownika</param>
        /// <param name="password">has³o u¿ytkownika</param>
        /// <param name="tokenId">numer tokenu (konieczny!)</param>
        /// <param name="tokenValue">wartoœæ odczytana z tokenu (konieczna!)</param>
        /// <returns>nowo otrzymany numer gadu-gadu lub '0' gdy operacja siê nie powiedzie</returns>
        public int GGRegisterAccount(string email, string password, string tokenId, string tokenValue) {
            return HttpRegisterAccount(email, password, tokenId, tokenValue);
        }

        /// <summary>
        /// Usuwa konto gadu-gadu
        /// </summary>
        /// <param name="GGNumber">numer GG u¿ytkownika</param>
        /// <param name="password">has³o u¿ytkownika</param>
        /// <param name="tokenId">numer tokenu (konieczny!)</param>
        /// <param name="tokenValue">wartoœæ odczytana z tokenu (konieczna!)</param>
        /// <returns>zwraca TRUE jeœli operacja powiedzie siê</returns>
        public bool GGDeleteAccount(int GGNumber, string password, string tokenId, string tokenValue) {
            return HttpDeleteAccount(GGNumber, password, tokenId, tokenValue);
        }

        /// <summary>
        /// Zmiana has³a GG
        /// </summary>
        /// <param name="GGNumber">numer GG u¿ytkownika</param>
        /// <param name="password">aktualne has³o</param>
        /// <param name="newPassword">nowe has³o</param>
        /// <param name="email">adres e-mail u¿ytkownika</param>
        /// <param name="tokenId">numer tokenu (konieczny!)</param>
        /// <param name="tokenValue">wartoœæ odczytana z tokenu (konieczna!)</param>
        /// <returns>zwraca TRUE jeœli operacja powiedzie siê</returns>
        public bool GGChangePassword(int GGNumber, string password, string newPassword, string email, string tokenId, string tokenValue) {
            return HttpChangePassword(GGNumber, password, newPassword, email, tokenId, tokenValue);
        }

        /// <summary>
        /// Zmiana adresu e-mail klienta
        /// </summary>
        /// <param name="GGNumber">numer GG u¿ytkownika</param>
        /// <param name="password">has³o</param>
        /// <param name="newEmail">nowy adres e-mail</param>
        /// <param name="tokenId">numer tokenu (konieczny!)</param>
        /// <param name="tokenValue">wartoœæ odczytana z tokenu (konieczna!)</param>
        /// <returns>zwraca TRUE jeœli operacja powiedzie siê</returns>
        public bool GGChangeEmail(int GGNumber, string password, string newEmail, string tokenId, string tokenValue) {
            return HttpChangePassword(GGNumber, password, password, newEmail, tokenId, tokenValue);
        }

        // todo: funkcja nie dzia³a
        //public bool GGPasswordRemind(int GGNumber, string tokenId, string tokenValue)
        //{
        //    return HttpPasswordRemind(GGNumber, tokenId, tokenValue);
        //}

        /// <summary>
        /// Wysy³a wiadomoœæ do u¿ytkownika gadu-gadu
        /// </summary>
        /// <param name="recipient">numer GG u¿ytkownika, do którego wiadomoœæ ma byæ wys³ana</param>
        /// <param name="message">treœæ wiadomoœci</param>
        public void GGSendMessage(int recipient, string message) {
            MessageEngine(new int[] {recipient}, message, null, false);
        }

        /// <summary>
        /// Wysy³a wiadomoœæ do kilku u¿ytkowników gadu-gadu (konferencja)
        /// </summary>
        /// <param name="recipients">numery GG uczestników konferencji</param>
        /// <param name="message">treœæ wiadomoœci</param>
        public void GGSendMessage(int[] recipients, string message) {
            MessageEngine(recipients, message, null, true);
        }

        /// <summary>
        /// Wysy³a sformatowan¹ wiadomoœæ do u¿ytkownika gadu-gadu
        /// </summary>
        /// <param name="recipient">numer GG u¿ytkownika, do którego wiadomoœæ ma byæ wys³ana</param>
        /// <param name="message">treœæ wiadomoœci</param>
        /// <param name="messageFormat">obiekt opisuj¹cy formatowanie wiadomoœci (zobacz plik manual.txt)</param>
        public void GGSendMessage(int recipient, string message, SortedDictionary<short,string> messageFormat) {
            MessageEngine(new int[] { recipient }, message, messageFormat, false);
        }

        /// <summary>
        /// Wysy³a sformatowan¹ wiadomoœæ do kilku u¿ytkowników gadu-gadu (konferencja)
        /// </summary>
        /// <param name="recipients">numery GG uczestników konferencji</param>
        /// <param name="message">treœæ wiadomoœci</param>
        /// <param name="messageFormat">obiekt opisuj¹cy formatowanie wiadomoœci (zobacz plik manual.txt)</param>
        [Obsolete("Uwaga! Wybrana wersja metody GGSendMessage jeszcze nie dzia³a")]
        public void GGSendMessage(int[] recipients, string message, SortedDictionary<short, string> messageFormat) {
            MessageEngine(recipients, message, messageFormat, true);
        }

        /// <summary>
        /// Wysy³a obrazek przez GG
        /// </summary>
        /// <param name="recipient">numer GG u¿ytkownika, do którego obrazek ma byæ wys³any</param>
        /// <param name="image">obrazek</param>
        public void GGSendImage(int recipient, MemoryStream image) {
            ImageEngine(recipient, string.Empty, 0, image);
        }

        /// <summary>
        /// Wysy³a obrazek przez GG
        /// </summary>
        /// <param name="recipient">numer GG u¿ytkownika, do którego obrazek ma byæ wys³any</param>
        /// <param name="imagePath">pe³na œcie¿ka do pliku z obrazkiem</param>
        public void GGSendImage(int recipient, string imagePath) {
            ImageEngine(recipient, string.Empty, 0, imagePath);
        }

        /// <summary>
        /// Wysy³a obrazek przez GG
        /// </summary>
        /// <param name="recipient">numer GG u¿ytkownika, do którego obrazek ma byæ wys³any</param>
        /// <param name="message">wiadomoœæ dostarczona razem z obrazkiem</param>
        /// <param name="imagePos">pozycja w tekœcie pod jak¹ ma siê pojawiæ obrazek</param>
        /// <param name="imagePath">pe³na œcie¿ka do pliku z obrazkiem</param>
        public void GGSendImage(int recipient, string message, int imagePos, string imagePath) {
            ImageEngine(recipient, message, imagePos, imagePath);
        }

        /// <summary>
        /// Wysy³a obrazek przez GG
        /// </summary>
        /// <param name="recipient">numer GG u¿ytkownika, do którego obrazek ma byæ wys³any</param>
        /// <param name="message">wiadomoœæ dostarczona razem z obrazkiem</param>
        /// <param name="imagePos">pozycja w tekœcie pod jak¹ ma siê pojawiæ obrazek</param>
        /// <param name="image">obrazek</param>
        public void GGSendImage(int recipient, string message, int imagePos, MemoryStream image) {
            ImageEngine(recipient, message, imagePos, image);
        }

        /// <summary>
        /// Odczytuje informacje z katalogu publicznego
        /// </summary>
        public void PubDirRead() {
            PublicFolderRead();
        }

        /// <summary>
        /// Wyszukuje osoby w katalogu publicznym
        /// </summary>
        /// <param name="GGNumber">numer GG osoby (0 jeœli obojêtnie)</param>
        /// <param name="firstname">imiê osoby (pusty string jeœli obojêtnie)</param>
        /// <param name="lastname">nazwisko osoby (pusty string jeœli obojêtnie)</param>
        /// <param name="nickname">nick osoby (pusty string jeœli obojêtnie)</param>
        /// <param name="birthYear">rok urodzenia osoby np. '1980' lub przedzia³ - daty odzielone spacj¹, np. '1980 1984' (pusty string jeœli obojêtnie)</param>
        /// <param name="city">miejscowoœæ (pusty string jeœli obojêtnie)</param>
        /// <param name="gender">p³eæ osoby</param>
        /// <param name="activeOnly">wartoœæ TRUE jeœli szukamy tylko osób dostêpnych</param>
        public void PubDirSearch(int GGNumber, string firstname, string lastname, string nickname, string birthYear, 
                                         string city, GGGender gender, bool activeOnly)
        {
            PublicFolderSearch(GGNumber, firstname, lastname, nickname, birthYear, city, gender, activeOnly);
        }

        /// <summary>
        /// Szuka kolejnych osób w katalogu publicznym z ostatniego zapytania
        /// </summary>
        public void PubDirSearchNext() {
            PublicFolderSearchNext();
        }




    }
}
