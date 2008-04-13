/* GGUsersList.cs

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
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace HAKGERSoft  {

    /// <summary>
    /// Lista kontaktów
    /// </summary>
    public sealed class GGUsers : List<GGUser> {
        private sHGG owner;

        /// <summary>
        /// Konstruktor listy kontaktów
        /// </summary>
        /// <param name="owner">obiekt typu sHGG (właściciel listy)</param>
        public GGUsers(sHGG owner): base() {
            this.owner = owner;
        }

        /// <summary>
        /// Dodaje nową osobę do listy kontaktów
        /// </summary>
        /// <param name="user">nowa osoba</param>
        new public void Add(GGUser user) {
            if (this.Contains(user))
                return;
            base.Add(user);
            owner.OutUsersNotify(user, sHGG.OUT_USERS_ADD_NOTIFY, false);
            this.UserAddedHandler(user); 
        }

        /// <summary>
        /// Dodaje nową osobę do listy kontaktów
        /// </summary>
        /// <param name="GGnumber">numer GG osoby</param>
        public void Add(int GGnumber) {
            this.Add(new GGUser() { GGNumber = GGnumber });
        }

        /// <summary>
        /// Dodaje nowe osoby do listy kontaktów
        /// </summary>
        /// <param name="users">nicki i numery GG nowych osób</param>
        /// <param name="transaction">transakcja gdy wartość jest równa TRUE - jeśli
        /// któryś z numerów jest już na liście, nie doda żadnego numeru</param>
        /// <returns>zwraca TRUE jeśli uda się dodać nowe osoby</returns>
        public bool Add(Dictionary<string,int> users, bool transaction) {
            if (users==null)
                return false;
            List<GGUser> transact = new List<GGUser>();
            IDictionaryEnumerator userEnum = users.GetEnumerator();
            while (userEnum.MoveNext()) {
                if ((this.Contains((int)userEnum.Value) || transact.Exists(x => x.GGNumber==(int)userEnum.Value)) & transaction)
                    return false; // rollback 
                transact.Add(new GGUser() { GGNick = (string)userEnum.Key, GGNumber = (int)userEnum.Value });
            }
            transact.ForEach(user => this.Add(user));
            return true;
        }

        /// <summary>
        /// Dodaje nowe osoby do listy kontaktów
        /// </summary>
        /// <param name="GGnumbers">numery GG nowych osób</param>
        /// <param name="transaction">transakcja gdy wartość jest równa TRUE - jeśli
        /// któryś z numerów jest już na liście, nie doda żadnego numeru</param>
        /// <returns>zwraca TRUE jeśli uda się dodać nowe osoby</returns>
        public bool Add(int[] GGnumbers, bool transaction) {
            if (GGnumbers == null || GGnumbers.Length == 0)
                return false;
            Dictionary<string,int> newUsers = new Dictionary<string,int>();
            int i = 1;
            foreach (int number in GGnumbers)
                newUsers.Add("User " + i++.ToString(), number);
            return this.Add(newUsers, transaction);
        }

        /// <summary>
        /// Usuwa osobę z listy kontaktów (po numerze GG)
        /// </summary>
        /// <param name="user">osoba na liście</param>
        /// <returns>zwraca TRUE jeśli uda się usunąć osobę</returns>
        new public bool Remove(GGUser user) {
            if (!this.Contains(user))
                return false;
            owner.OutUsersNotify(user, sHGG.OUT_USERS_REMOVE_NOTIFY, false);
            bool result = base.Remove(user);
            this.ListChangedHandler();
            return result;
        }

        /// <summary>
        /// Usuwa osobę z listy kontaktów (po numerze GG)
        /// </summary>
        /// <param name="GGnumber">numer GG osoby</param>
        /// <returns>zwraca TRUE jeśli uda się usunąć osobę</returns>
        public bool Remove(int GGnumber) {
            return this.Remove(new GGUser() {GGNumber = GGnumber} );
        }

        /// <summary>
        /// Sprawdza czy osoba znajduje się już na liście kontaktów
        /// </summary>
        /// <param name="item">osoba</param>
        /// <returns>zwraca TRUE jeśli osoba znajduje się na liście</returns>
        new public bool Contains(GGUser item) {
            if (item == null)
                return false;
            return this.Exists(i => i.Equals(item));
        }

        /// <summary>
        /// Sprawdza czy osoba znajduje się już na liście kontaktów
        /// </summary>
        /// <param name="GGNumber">numer GG osoby</param>
        /// <returns>zwraca TRUE jeśli osoba znajduje się na liście</returns>
        public bool Contains(int GGNumber) {
            return this.Contains( new GGUser() { GGNumber = GGNumber });
        }

        /// <summary>
        /// Usuwa wszystkie osoby z listy kontaktów 
        /// </summary>
        new public void Clear() {
            this.ForEach(user => owner.OutUsersNotify(user, sHGG.OUT_USERS_REMOVE_NOTIFY, false));
            base.Clear();
            this.ListChangedHandler();
        }

        /// <summary>
        /// Blokuje osobę
        /// </summary>
        /// <param name="user">osoba, która ma być zablokowana</param>
        public void Block(GGUser user) {
            owner.OutUsersNotify(user, sHGG.OUT_USERS_ADD_NOTIFY, true);
            this.Remove(user);
            this.ListChangedHandler();
        }

        /// <summary>
        /// Blokuje osobę
        /// </summary>
        /// <param name="GGNumber">numer GG osoby, która ma być zablokowana</param>
        public void Block(int GGNumber) {
            this.Block(new GGUser() { GGNumber = GGNumber } );
        }

        //public void ExportToGGServer() {
            
        //}

        /// <summary>
        /// Zapisuje listę kontaktów do pliku 
        /// </summary>
        /// <param name="filePath">Ścieżka do pliku</param>
        public void ExportToFile(string filePath) {
            byte[] usersBin = this.DeserializeUserlist();
            this.WriteStream(usersBin, filePath); 
        }

        /// <summary>
        /// Zdarzenie się wykonuje gdy osoba na liście kontaktów zmieni stan
        /// </summary>
        public event sHGG.GenericEventHandler<UserEventArgs> UserChanged;

        /// <summary>
        /// Zdarzenie się wykonuje w przypadku dodania osoby do listy kontaktów
        /// </summary>
        public event sHGG.GenericEventHandler<UserEventArgs> UserAdded;

        //public event GenericEventHandler<UserEventArgs> UserRemoved;

        /// <summary>
        /// Zdarzenie obsługujące większe zmiany listy kontaktów 
        /// </summary>
        public event sHGG.GenericEventHandler<EventArgs> ListChanged;

        public class UserEventArgs : EventArgs
        {
            public GGUser User;
        }

        internal void UserChangedHandler(GGUser user) {
            if (user != null && this.Contains(user)) {
                GGUser item = this.Find(x => x.Equals(user));
                if (item == null)
                    return;
                item.vGGStatus = user.GGStatus;
                item.vDescription = user.Description;
                item.vGGClientVersion = user.GGClientVersion;
                item.vIPAdress = user.IPAdress;
                item.vMaxImageSize = user.MaxImageSize;
                user = item;
                this.UsersSort();
                UserEventArgs args = new UserEventArgs() { User = user };
                owner.PostCallback<UserEventArgs>(UserChanged, args);
            }
        }

        internal void UserAddedHandler(GGUser user) {
            this.UsersSort();
            UserEventArgs args = new UserEventArgs() { User = user };
            owner.PostCallback<UserEventArgs>(UserAdded, args);
        }

        internal void ListChangedHandler() {
            this.UsersSort();
            owner.PostCallback<EventArgs>(ListChanged, EventArgs.Empty);
        }

        internal void UsersRestart() {
            foreach (GGUser user in this) {
                user.vGGStatus = GGStatusType.NotAvailable;
                user.vDescription = string.Empty;
                user.vIPAdress = string.Empty;
                user.vGGClientVersion = string.Empty;
                user.vMaxImageSize = 0;
            }
            ListChangedHandler();
        }

        internal void UsersSort() {
            if (this == null || this.Count == 0)
                return;
            this.Sort(DefaultSort);
        }

        internal int DefaultSort(GGUser x, GGUser y) {
            int xStatusOrder = sHGG.GetStatusOrder(x);
            int yStatusOrder = sHGG.GetStatusOrder(y);
            if (xStatusOrder == yStatusOrder)
                return x.GGNick.CompareTo(y.GGNick);
            else
                return yStatusOrder - xStatusOrder;
        }

        private void WriteStream(byte[] data, string path) {
            FileStream wstr = null;
            try {
                wstr = new FileStream(path, FileMode.Create, FileAccess.Write);
                wstr.Write(data, 0, data.Length);
            }
            catch {
                this.owner.GGLogout();
                throw;
            }
            finally {
                if (wstr != null)
                    wstr.Close();      
            }
        }

        private byte[] DeserializeUserlist() {
            string res = string.Empty;
            foreach(GGUser user in this) {
                res += string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};\n", 
                    user.Name, 
                    user.LastName, 
                    user.GGNick,
                    user.GGNick,
                    user.Mobile,
                    string.Empty, // grupa
                    user.GGNumber.ToString(), 
                    user.Email, 
                    "0", // dźwięki domyślne
                    string.Empty, // brak scieżki do dźwięku
                    "0", // brak specjalnego dźwięku dla wiadomości
                    user.Friend ? "0" : "1", // znajomy
                    string.Empty // telefon domowy
                    );
            }
            if (string.IsNullOrEmpty(res))
                return new byte[] { };
            return Encoding.GetEncoding(sHGG.DEFAULT_ENCODING).GetBytes(res);
        }
       
 



    }
}