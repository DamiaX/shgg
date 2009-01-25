/* 
 * SHGG
 * More info in SHGG.cs file 
 * 
*/

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace HAKGERSoft {

    public sealed partial class sHGG {
        private int vNextStart = 0;
        private string queryStr = string.Empty; 

        const string SEARCH_QUERY_PATTERN = @"FmNumber\.(?<NUM>\d+)\.FmStatus\.(?<STS>\d)(\.firstname\.(?<NAME>\w+))?" + 
            @"(\.nickname\.(?<NNAME>\w+))?(\.birthyear\.(?<BIRTH>\d+))?(\.city\.(?<CITY>\w+))?\.\.(nextstart\.(?<NEXT>\d+).)?";

        const string READ_QUERY_PATTERN = @"(firstname\.(?<NAME>\w+))?(\.lastname\.(?<LNAME>\w+))?(\.birthyear\.(?<BIRTH>\d+))?" +
            @"(\.city\.(?<CITY>\w+))?(\.nickname\.(?<NNAME>\w+))?(\.gender\.(?<GR>\d))?(\.familyname\.(?<MNAME>\w+))?(\.familycity\.(?<FCITY>\w+))?\.";

        private byte[] BuildPubDirQuery(int GGNumber, string firstname, string lastname, string nickname, string birthYear,
                                         string city, GGGender gender, bool activeOnly, out string queryStr)
        {
            queryStr = string.Empty;

            if (GGNumber != 0)
                queryStr += string.Format("FmNumber.{0}.", GGNumber.ToString());
            if (firstname != string.Empty)
                queryStr += string.Format("firstname.{0}.", firstname);
            if (lastname != string.Empty)
                queryStr += string.Format("lastname.{0}.", lastname);
            if (nickname != string.Empty)
                queryStr += string.Format("nickname.{0}.", nickname);
            if (birthYear != string.Empty)
                queryStr += string.Format("birthyear.{0}.", birthYear);
            if (city != string.Empty)
                queryStr += string.Format("city.{0}.", city);
            if (gender == GGGender.Male)
                queryStr += "gender.2.";
            if (gender == GGGender.Female)
                queryStr += "gender.1.";
            if (activeOnly)
                queryStr += "ActiveOnly.1.";

            if (string.IsNullOrEmpty(queryStr))
                return new byte[] { };

            queryStr = queryStr.TrimEnd(new char[] { '.' });
            return Str2ByteArray(queryStr);
        }

        private byte[] Str2ByteArray(string query) {
            byte[] queryArray = Encoding.GetEncoding(DEFAULT_ENCODING).GetBytes(query);
            for (int i = 0; i < queryArray.Length; i++)
                queryArray[i] = (queryArray[i] == 46) ? (byte)0 : queryArray[i];
            return queryArray;
        }

        private List<GGUser> ReplyQuery2List(string query, out int nextStart, bool readMode) {
            nextStart = 0;
            List<GGUser> reply = new List<GGUser>();            
            if (string.IsNullOrEmpty(query))
                return reply;
            string pattern = (readMode) ? READ_QUERY_PATTERN : SEARCH_QUERY_PATTERN;
            Regex regex = new Regex(pattern, RegexOptions.Compiled);
            MatchCollection matches = regex.Matches(query);
            foreach(Match match in matches) {
                GGUser user = new GGUser();
                user.GGNumber = match.Groups["NUM"].Success ? int.Parse(match.Groups["NUM"].Value) : 0;
                user.vGGStatus = match.Groups["STS"].Success ? StatusDecode(uint.Parse(match.Groups["STS"].Value)) : GGStatusType.NotAvailable;
                user.Name = match.Groups["NAME"].Success ? match.Groups["NAME"].Value : string.Empty;
                user.GGNick = match.Groups["NNAME"].Success ? match.Groups["NNAME"].Value : string.Empty;
                user.BirthYear = match.Groups["BIRTH"].Success ? int.Parse(match.Groups["BIRTH"].Value) : 0;
                user.City = match.Groups["CITY"].Success ? match.Groups["CITY"].Value : string.Empty;
                user.FamilyCity = match.Groups["FCITY"].Success ? match.Groups["FCITY"].Value : string.Empty;
                user.FamilyName = match.Groups["MNAME"].Success ? match.Groups["MNAME"].Value : string.Empty;
                nextStart = match.Groups["NEXT"].Success ? int.Parse(match.Groups["NEXT"].Value) : nextStart;
                reply.Add(user);
            }
            return reply;
        }

        private void PublicFolderRead() {
            string queryStr;
            byte[] queryArray = BuildPubDirQuery(int.Parse(this.GGNumber), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, GGGender.All, false, out queryStr);
            stPubDir OutPubDir = new stPubDir();
            OutPubDir.Header.Type = OUT_PUBDIR_REQUEST;
            OutPubDir.Header.Size = 5 + (uint)queryArray.Length;
            OutPubDir.RequestType = OUT_PUBDIR_READ;
            ForwardData(RawSerialize(OutPubDir), 0);
            ForwardData(queryArray, 0);
        }

        private void PublicFolderSearch(int GGNumber, string firstname, string lastname, string nickname, string birthYear,
                                         string city, GGGender gender, bool activeOnly)
        {
            byte[] queryArray = BuildPubDirQuery(GGNumber, firstname, lastname, nickname, birthYear, city, gender, activeOnly, out queryStr);
            stPubDir OutPubDir = new stPubDir();
            OutPubDir.Header.Type = OUT_PUBDIR_REQUEST;
            OutPubDir.Header.Size = 5 + (uint)queryArray.Length;
            OutPubDir.RequestType = OUT_PUBDIR_SEARCH;
            ForwardData(RawSerialize(OutPubDir), 0);
            ForwardData(queryArray, 0);
        }

        private void PublicFolderSearchNext() {
            if (string.IsNullOrEmpty(queryStr) || (!PubDirCanSearchMore))
                return;

            string query = queryStr + ".fmstart." + vNextStart.ToString() + ".";
            byte[] queryArray = Str2ByteArray(query);

            stPubDir OutPubDir = new stPubDir();
            OutPubDir.Header.Type = OUT_PUBDIR_REQUEST;
            OutPubDir.Header.Size = 5 + (uint)queryArray.Length;
            OutPubDir.RequestType = OUT_PUBDIR_SEARCH;
            ForwardData(RawSerialize(OutPubDir), 0);
            ForwardData(queryArray, 0);
        }

        private void PubDirDataReply(byte[] reply) {
            int nextStart = 0;
            for (int i = 0; i < reply.Length; i++)
                reply[i] = (reply[i] == 0) ? (byte)46 : reply[i];
            string query = Encoding.GetEncoding(DEFAULT_ENCODING).GetString(reply);
            List<GGUser> data = ReplyQuery2List(query, out nextStart, true);
            if (data.Count != 1)
                return;
            PubDirReadReplyEventArgs replyEventArgs = new PubDirReadReplyEventArgs();
            replyEventArgs.User = data[0];
            PostCallback<PubDirReadReplyEventArgs>(GGPubDirReadReply, replyEventArgs);
        }

        private void PubDirSearchReply(byte[] reply) {
            for (int i = 0; i < reply.Length; i++)
                reply[i] = (reply[i] == 0) ? (byte)46 : reply[i];
            string query = Encoding.GetEncoding(DEFAULT_ENCODING).GetString(reply);
            int nextStart = 0;
            SearchReplyEventArgs replyEventArgs = new SearchReplyEventArgs();
            replyEventArgs.Users = ReplyQuery2List(query, out nextStart, false);
            vNextStart = nextStart;
            PostCallback<SearchReplyEventArgs>(GGSearchReply, replyEventArgs);
        }




    }
}