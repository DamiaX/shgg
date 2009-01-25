/* 
 * SHGG
 * More info in SHGG.cs file 
 * 
*/

using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace HAKGERSoft {

    /// <summary>
    /// Lista kontaktów
    /// </summary>
    public sealed class GGUsers :List<GGUser> {
        sHGG Owner;

        /// <summary>
        /// Pobiera użytkownika o danym nicku GG
        /// </summary>
        /// <param name="nick">nick użytkownika</param>
        /// <returns></returns>
        public GGUser this[string nick] {
            get {
                return this.First(x => x==nick);
            }
        }

        /// <summary>
        /// Pobiera użytkownika o danym numerze GG
        /// </summary>
        /// <param name="ggNumber">numer gg</param>
        /// <returns></returns>
        public GGUser this[uint ggNumber] {
            get {
                return this.First(x => x==ggNumber);
            }
        }

        /// <summary>
        /// Pobiera użytkownika o danym indexie na liście
        /// </summary>
        /// <param name="index">index</param>
        /// <returns></returns>
        new public GGUser this[int index] {
            get {
                return base[index];
            }
        }

        /// <summary>
        /// Konstruktor listy kontaktów
        /// </summary>
        /// <param name="owner">obiekt typu sHGG (właściciel listy)</param>
        internal GGUsers(sHGG owner) : base() {
            this.Owner = owner;
        }

        /// <summary>
        /// Dodaje nową osobę do listy kontaktów
        /// </summary>
        /// <param name="user">nowa osoba</param>
        new public void Add(GGUser user) {
            if (this.Contains(user))
                return;
            base.Add(user);
            if(Owner.IsGGLogged)
                Owner.OutUsersNotify(user, sHGG.OUT_USERS_ADD_NOTIFY, false);
            this.UserAddedHandler(user);
        }

        /// <summary>
        /// Dodaje nową osobę do listy kontaktów
        /// </summary>
        /// <param name="ggNumber">numer GG osoby</param>
        public void Add(int ggNumber) {
            this.Add((GGUser)ggNumber);
        }

        /// <summary>
        /// Dodaje nowe osoby do listy kontaktów
        /// </summary>
        /// <param name="users">nicki i numery GG nowych osób</param>
        /// <param name="transaction">transakcja gdy wartość jest równa TRUE - jeśli
        /// któryś z numerów jest już na liście, nie doda żadnego numeru</param>
        /// <returns>zwraca TRUE jeśli uda się dodać nowe osoby</returns>
        public bool Add(Dictionary<string, int> users, bool transaction) {
            if(users == null)
                return false;
            List<GGUser> transact = new List<GGUser>();
            IDictionaryEnumerator userEnum = users.GetEnumerator();
            while(userEnum.MoveNext()) {
                if((this.Contains((int)userEnum.Value) || transact.Exists(x => x == (int)userEnum.Value)) && transaction)
                    return false; // rollback 
                transact.Add(new GGUser() { GGNick=(string)userEnum.Key, GGNumber=(int)userEnum.Value });
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
        public bool Add(int[] ggnumbers, bool transaction) {
            if (ggnumbers == null || ggnumbers.Length == 0)
                return false;
            Dictionary<string, int> newUsers = new Dictionary<string, int>();
            int i = 1;
            foreach(int number in ggnumbers)
                newUsers.Add("User " + i++.ToString(), number);
            return this.Add(newUsers, transaction);
        }

        /// <summary>
        /// Usuwa osobę z listy kontaktów (po numerze GG)
        /// </summary>
        /// <param name="user">osoba na liście</param>
        /// <returns>zwraca TRUE jeśli uda się usunąć osobę</returns>
        new public bool Remove(GGUser user) {
            if(!this.Contains(user))
                return false;
            Owner.OutUsersNotify(user, sHGG.OUT_USERS_REMOVE_NOTIFY, false);
            bool result = base.Remove(user);
            this.ListChangedHandler();
            return result;
        }

        /// <summary>
        /// Usuwa osobę z listy kontaktów (po numerze GG)
        /// </summary>
        /// <param name="GGnumber">numer GG osoby</param>
        /// <returns>zwraca TRUE jeśli uda się usunąć osobę</returns>
        public bool Remove(int ggnumber) {
            return this.Remove((GGUser)ggnumber);
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
        public bool Contains(int ggNumber) {
            return this.Contains((GGUser)ggNumber);
        }

        /// <summary>
        /// Usuwa wszystkie osoby z listy kontaktów 
        /// </summary>
        new public void Clear() {
            this.ForEach(user => Owner.OutUsersNotify(user, sHGG.OUT_USERS_REMOVE_NOTIFY, false));
            base.Clear();
            this.ListChangedHandler();
        }

        /// <summary>
        /// Blokuje osobę
        /// </summary>
        /// <param name="user">osoba, która ma być zablokowana</param>
        public void Block(GGUser user) {
            Owner.OutUsersNotify(user, sHGG.OUT_USERS_ADD_NOTIFY, true);
            this.Remove(user);
            this.ListChangedHandler();
        }

        /// <summary>
        /// Blokuje osobę
        /// </summary>
        /// <param name="GGNumber">numer GG osoby, która ma być zablokowana</param>
        public void Block(int ggNumber) {
            this.Block((GGUser)ggNumber);
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

        public void ImportFromFile(string filePath) {
            StreamReader sr = new StreamReader(filePath);
            try {
                while (!sr.EndOfStream) {
                    string[] input = sr.ReadLine().Split(new char[] { ';' });
                    try {
                        GGUser u = new GGUser();
                        u.Name = input[0];
                        u.LastName = input[1];
                        u.GGNick = input[3];
                        u.Mobile = input[4];
                        if (!int.TryParse(input[6], out u.GGNumber))
                            continue;
                        u.Email = input[7];
                        u.Friend = (input[11].Trim() == "1");
                        u.Phone = input[12];

                        base.Add(u);

                        if (Owner.IsGGLogged)
                            Owner.OutUsersNotify(u, sHGG.OUT_USERS_ADD_NOTIFY, false);

                    } catch (IndexOutOfRangeException) {
                        continue;
                    }

                }

            } catch {
                throw;
            } finally {
                sr.Close();

                ListChangedHandler();
            }
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

        public class UserEventArgs :EventArgs {
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
                Owner.PostCallback<UserEventArgs>(UserChanged, args);
            }
        }

        internal void UserAddedHandler(GGUser user) {
            this.UsersSort();
            UserEventArgs args = new UserEventArgs() { User = user };
            Owner.PostCallback<UserEventArgs>(UserAdded, args);
        }

        internal void ListChangedHandler() {
            this.UsersSort();
            Owner.PostCallback<EventArgs>(ListChanged, EventArgs.Empty);
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

        void WriteStream(byte[] data, string path) {
            FileStream wstr = null;
            try {
                wstr = new FileStream(path, FileMode.Create, FileAccess.Write);
                wstr.Write(data, 0, data.Length);
            } catch {
                this.Owner.GGLogout();
                throw;
            } finally {
                if (wstr != null)
                    wstr.Close();
            }
        }

        byte[] DeserializeUserlist() {
            string res = string.Empty;
            foreach (GGUser user in this) {
                res += string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};\n\r",
                    user.Name,                  // 0
                    user.LastName,              // 1
                    user.GGNick,                // 2
                    user.GGNick,                // 3
                    user.Mobile,                // 4
                    string.Empty,               // 5 (grupa)
                    user.GGNumber.ToString(),   // 6 
                    user.Email,                 // 7
                    "0",                        // 8 (dźwięki domyślne)
                    string.Empty,               // 9 (brak scieżki do dźwięku)
                    "0",                        // 10 (brak specjalnego dźwięku dla wiadomości)
                    user.Friend ? "0" : "1",    // 11 (znajomy)
                    user.Phone                  // 12
                    );
            }
            if (string.IsNullOrEmpty(res))
                return new byte[] { };
            return Encoding.GetEncoding(sHGG.DEFAULT_ENCODING).GetBytes(res);
        }










    }
}