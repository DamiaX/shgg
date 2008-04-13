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
    /// G��wna klasa obs�uguj�ca silnik klienta gadu-gadu
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
        /// Lista kontakt�w
        /// </summary>
        public GGUsers Users;

        # region Properties

        /// <summary>
        /// <value>Okre�la adres IP serwera gadu-gadu</value>
        /// </summary>
        public string GGServerAddress {
            get { return vGGServerAddress; }
            set { vGGServerAddress = value; }
        }

        private string vGGServerAddress = DEFAULT_GG_HOST;

        /// <summary>
        /// <value>Okre�la numer gadu-gadu klienta (tw�j numer GG)</value>
        /// </summary>
        public string GGNumber {
            get { return vGGNumber; }
            set { vGGNumber = value; }
        }

        private string vGGNumber = "0";

        /// <summary>
        /// <value>Okre�la has�o klienta gadu-gadu (twoje has�o)</value>
        /// </summary>
        public string GGPassword {
            get { return vGGPassword; }
            set { vGGPassword = value; }
        }

        private string vGGPassword = string.Empty;

        /// <summary>
        /// <value>Okre�la status gadu-gadu</value>
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
        /// <value>Okre�la opis klienta gadu-gadu</value>
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
        /// <vale>Je�li w�a�ciwo�� ma warto�� TRUE to w��czony jest
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
        /// <value>Pozwala okre�li� maksymaln� wielko�� obrazka</value>
        /// </summary>
        public byte GGImageSize {
            get { return vGGImageSize; }
            set { vGGImageSize = value; } // todo: re login?
        }

        private byte vGGImageSize = (byte)255;

        /// <summary>
        /// <value>W�a�ciwo�� jednoznacznie okre�la czy klient jest zalogowany
        /// w danej chwili do serwera gadu-gadu (tylko do odczytu)</value>
        /// </summary>
        public bool IsGGLogged {
            get { return vIsGGLogged; }
            private set { vIsGGLogged = value; } 
        }
        
        private bool vIsGGLogged = false;

        /// <summary>
        /// W�a�ciwo�� okre�la czy mo�na szuka� dalej w katalogu publicznym (tylko do odczytu)
        /// </summary>
        public bool PubDirCanSearchMore {
            get { return vNextStart != 0; }
        }

        # endregion

        # region Events

        public delegate void GenericEventHandler<A>(object sender, A args);
        
        /// <summary>
        /// Zdarzenie wykonuje si� w memencie zalogowania do serwera
        /// </summary>
        public event GenericEventHandler<EventArgs> GGLogged;

        /// <summary>
        /// Zdarzenie wykonuje si� w przypadku nieudanej pr�by zalogowania do serwera
        /// </summary>
        public event GenericEventHandler<EventArgs> GGLogFailed;

        /// <summary>
        /// Zdarzenie wykonuje si� w przypadku, gdy serwer gadu-gadu roz��czy klienta
        /// </summary>
        public event GenericEventHandler<EventArgs> GGDisconnected;

        /// <summary>
        /// Zdarzenie wykonuje si� gdy przysz�a nowa wiadomo��
        /// </summary>
        public event GenericEventHandler<MessageReceiveEventArgs> GGMessageReceive;
               
        public class MessageReceiveEventArgs : EventArgs {
            /// <summary>
            /// numer GG nadawcy wiadomo�ci 
            /// </summary>
            public int Number;

            /// <summary>
            /// tre�� wiadomo�ci
            /// </summary>
            public string Message;

            /// <summary>
            /// czas wys�ania wiadomo�ci
            /// </summary>
            public DateTime Time;
        }

        /// <summary>
        /// Zdarzenie informuje o wynikach szukania os�b w katalogu publicznym
        /// </summary>
        public event GenericEventHandler<SearchReplyEventArgs> GGSearchReply;

        public class SearchReplyEventArgs : EventArgs {
            /// <summary>
            /// znalezione osoby
            /// </summary>
            public List<GGUser> Users;
        }

        /// <summary>
        /// Zdarzenie wykonuje si� gdy odczytano w�asne informacje w katalogu publicznym
        /// </summary>
        public event GenericEventHandler<PubDirReadReplyEventArgs> GGPubDirReadReply;

        public class PubDirReadReplyEventArgs : EventArgs {
            /// <summary>
            /// w�asne informacje w katalogu publicznym
            /// </summary>
            public GGUser User;
        }

        ///// <summary>
        ///// Zdarzenie wykonuje si� w przypadku pomy�lnego zapisania danych w katalogu publicznym
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
        /// Loguje si� do serwera gadu-gadu (domy�lny adres serwera GG)
        /// </summary>
        public void GGLogin() {
            GGLogin(GGServerAddress);
        }

        /// <summary>
        /// Loguje si� do serwera gadu-gadu (dowolny adres serwera GG)
        /// </summary>
        /// <param name="serverAddress"></param>
        public void GGLogin(string serverAddress) {
            OutConnect(serverAddress);
        }

        /// <summary>
        /// Metoda wylogowuje z serwera gadu-gadu (roz��cza)
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
        /// string w przypadku, gdy �aden nie jest dost�pny juz operacja nie
        /// powiedzie si�
        /// </summary>
        /// <returns>adres IP serwera</returns>
        public string GGGetActiveServer() {
            return HttpGGGetActiveServer() ?? string.Empty;
        }

        /// <summary>
        /// Funkcja generuje token gadu-gadu
        /// </summary>
        /// <param name="tokenId">id (numer) tokenu</param>
        /// <param name="tokenUrl">pe�en adres tokenu</param>
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
        /// Rejestruje nowego u�ytkownika gadu-gadu
        /// </summary>
        /// <param name="email">adres e-mail u�ytkownika</param>
        /// <param name="password">has�o u�ytkownika</param>
        /// <param name="tokenId">numer tokenu (konieczny!)</param>
        /// <param name="tokenValue">warto�� odczytana z tokenu (konieczna!)</param>
        /// <returns>nowo otrzymany numer gadu-gadu lub '0' gdy operacja si� nie powiedzie</returns>
        public int GGRegisterAccount(string email, string password, string tokenId, string tokenValue) {
            return HttpRegisterAccount(email, password, tokenId, tokenValue);
        }

        /// <summary>
        /// Usuwa konto gadu-gadu
        /// </summary>
        /// <param name="GGNumber">numer GG u�ytkownika</param>
        /// <param name="password">has�o u�ytkownika</param>
        /// <param name="tokenId">numer tokenu (konieczny!)</param>
        /// <param name="tokenValue">warto�� odczytana z tokenu (konieczna!)</param>
        /// <returns>zwraca TRUE je�li operacja powiedzie si�</returns>
        public bool GGDeleteAccount(int GGNumber, string password, string tokenId, string tokenValue) {
            return HttpDeleteAccount(GGNumber, password, tokenId, tokenValue);
        }

        /// <summary>
        /// Zmiana has�a GG
        /// </summary>
        /// <param name="GGNumber">numer GG u�ytkownika</param>
        /// <param name="password">aktualne has�o</param>
        /// <param name="newPassword">nowe has�o</param>
        /// <param name="email">adres e-mail u�ytkownika</param>
        /// <param name="tokenId">numer tokenu (konieczny!)</param>
        /// <param name="tokenValue">warto�� odczytana z tokenu (konieczna!)</param>
        /// <returns>zwraca TRUE je�li operacja powiedzie si�</returns>
        public bool GGChangePassword(int GGNumber, string password, string newPassword, string email, string tokenId, string tokenValue) {
            return HttpChangePassword(GGNumber, password, newPassword, email, tokenId, tokenValue);
        }

        /// <summary>
        /// Zmiana adresu e-mail klienta
        /// </summary>
        /// <param name="GGNumber">numer GG u�ytkownika</param>
        /// <param name="password">has�o</param>
        /// <param name="newEmail">nowy adres e-mail</param>
        /// <param name="tokenId">numer tokenu (konieczny!)</param>
        /// <param name="tokenValue">warto�� odczytana z tokenu (konieczna!)</param>
        /// <returns>zwraca TRUE je�li operacja powiedzie si�</returns>
        public bool GGChangeEmail(int GGNumber, string password, string newEmail, string tokenId, string tokenValue) {
            return HttpChangePassword(GGNumber, password, password, newEmail, tokenId, tokenValue);
        }

        // todo: funkcja nie dzia�a
        //public bool GGPasswordRemind(int GGNumber, string tokenId, string tokenValue)
        //{
        //    return HttpPasswordRemind(GGNumber, tokenId, tokenValue);
        //}

        /// <summary>
        /// Wysy�a wiadomo�� do u�ytkownika gadu-gadu
        /// </summary>
        /// <param name="recipient">numer GG u�ytkownika, do kt�rego wiadomo�� ma by� wys�ana</param>
        /// <param name="message">tre�� wiadomo�ci</param>
        public void GGSendMessage(int recipient, string message) {
            MessageEngine(new int[] {recipient}, message, null, false);
        }

        /// <summary>
        /// Wysy�a wiadomo�� do kilku u�ytkownik�w gadu-gadu (konferencja)
        /// </summary>
        /// <param name="recipients">numery GG uczestnik�w konferencji</param>
        /// <param name="message">tre�� wiadomo�ci</param>
        public void GGSendMessage(int[] recipients, string message) {
            MessageEngine(recipients, message, null, true);
        }

        /// <summary>
        /// Wysy�a sformatowan� wiadomo�� do u�ytkownika gadu-gadu
        /// </summary>
        /// <param name="recipient">numer GG u�ytkownika, do kt�rego wiadomo�� ma by� wys�ana</param>
        /// <param name="message">tre�� wiadomo�ci</param>
        /// <param name="messageFormat">obiekt opisuj�cy formatowanie wiadomo�ci (zobacz plik manual.txt)</param>
        public void GGSendMessage(int recipient, string message, SortedDictionary<short,string> messageFormat) {
            MessageEngine(new int[] { recipient }, message, messageFormat, false);
        }

        /// <summary>
        /// Wysy�a sformatowan� wiadomo�� do kilku u�ytkownik�w gadu-gadu (konferencja)
        /// </summary>
        /// <param name="recipients">numery GG uczestnik�w konferencji</param>
        /// <param name="message">tre�� wiadomo�ci</param>
        /// <param name="messageFormat">obiekt opisuj�cy formatowanie wiadomo�ci (zobacz plik manual.txt)</param>
        [Obsolete("Uwaga! Wybrana wersja metody GGSendMessage jeszcze nie dzia�a")]
        public void GGSendMessage(int[] recipients, string message, SortedDictionary<short, string> messageFormat) {
            MessageEngine(recipients, message, messageFormat, true);
        }

        /// <summary>
        /// Wysy�a obrazek przez GG
        /// </summary>
        /// <param name="recipient">numer GG u�ytkownika, do kt�rego obrazek ma by� wys�any</param>
        /// <param name="image">obrazek</param>
        public void GGSendImage(int recipient, MemoryStream image) {
            ImageEngine(recipient, string.Empty, 0, image);
        }

        /// <summary>
        /// Wysy�a obrazek przez GG
        /// </summary>
        /// <param name="recipient">numer GG u�ytkownika, do kt�rego obrazek ma by� wys�any</param>
        /// <param name="imagePath">pe�na �cie�ka do pliku z obrazkiem</param>
        public void GGSendImage(int recipient, string imagePath) {
            ImageEngine(recipient, string.Empty, 0, imagePath);
        }

        /// <summary>
        /// Wysy�a obrazek przez GG
        /// </summary>
        /// <param name="recipient">numer GG u�ytkownika, do kt�rego obrazek ma by� wys�any</param>
        /// <param name="message">wiadomo�� dostarczona razem z obrazkiem</param>
        /// <param name="imagePos">pozycja w tek�cie pod jak� ma si� pojawi� obrazek</param>
        /// <param name="imagePath">pe�na �cie�ka do pliku z obrazkiem</param>
        public void GGSendImage(int recipient, string message, int imagePos, string imagePath) {
            ImageEngine(recipient, message, imagePos, imagePath);
        }

        /// <summary>
        /// Wysy�a obrazek przez GG
        /// </summary>
        /// <param name="recipient">numer GG u�ytkownika, do kt�rego obrazek ma by� wys�any</param>
        /// <param name="message">wiadomo�� dostarczona razem z obrazkiem</param>
        /// <param name="imagePos">pozycja w tek�cie pod jak� ma si� pojawi� obrazek</param>
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
        /// <param name="GGNumber">numer GG osoby (0 je�li oboj�tnie)</param>
        /// <param name="firstname">imi� osoby (pusty string je�li oboj�tnie)</param>
        /// <param name="lastname">nazwisko osoby (pusty string je�li oboj�tnie)</param>
        /// <param name="nickname">nick osoby (pusty string je�li oboj�tnie)</param>
        /// <param name="birthYear">rok urodzenia osoby np. '1980' lub przedzia� - daty odzielone spacj�, np. '1980 1984' (pusty string je�li oboj�tnie)</param>
        /// <param name="city">miejscowo�� (pusty string je�li oboj�tnie)</param>
        /// <param name="gender">p�e� osoby</param>
        /// <param name="activeOnly">warto�� TRUE je�li szukamy tylko os�b dost�pnych</param>
        public void PubDirSearch(int GGNumber, string firstname, string lastname, string nickname, string birthYear, 
                                         string city, GGGender gender, bool activeOnly)
        {
            PublicFolderSearch(GGNumber, firstname, lastname, nickname, birthYear, city, gender, activeOnly);
        }

        /// <summary>
        /// Szuka kolejnych os�b w katalogu publicznym z ostatniego zapytania
        /// </summary>
        public void PubDirSearchNext() {
            PublicFolderSearchNext();
        }




    }
}
