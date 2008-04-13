/* GGHttpServices.cs

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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Drawing;

namespace HAKGERSoft {

    public sealed partial class sHGG {
        private HttpWebRequest request;
        private HttpWebResponse response;
        private StreamReader httpStream;

        private string HttpRequest(string uri, string method, bool readLine) {
            string responseStr = string.Empty;
            try {
                request = (HttpWebRequest)WebRequest.Create(uri);
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 5.0; Windows NT; DigExt)";
                request.ContentLength = 0;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = method;
                response = (HttpWebResponse)request.GetResponse();
                httpStream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("windows-1250"));
                responseStr = (readLine) ? httpStream.ReadLine() : httpStream.ReadToEnd();
            }
            finally {
                if (response != null)
                    response.Close();
            }
            return responseStr;
        }

        private string HttpGGGetActiveServer() {
            string url = string.Format("http://appmsg.gadu-gadu.pl/appsvc/appmsg4.asp?fmnumber={0}&version=6,1,0,155&fmt=2", GGNumber);
            string response = HttpRequest(url, "GET", true);
            if (response == null)
                return null;
            string[] responseParts = response.Split(' ');
            if (responseParts.Length >= 4)
                return responseParts[3];
            return string.Empty;
        }

        private Image HttpGetToken(out string tokenId, out string tokenUrl) {
            string url = "http://register.gadu-gadu.pl/appsvc/regtoken.asp";
            string response = HttpRequest(url, "POST", false);
            string[] stringSeparators = new string[] {"\r\n"};
            string[] tokenInfo;
            tokenInfo = response.Split(stringSeparators, StringSplitOptions.None);
            if (tokenInfo.Length < 3) {
                tokenId = string.Empty;
                tokenUrl = string.Empty;
                return null;
            }
            tokenId = tokenInfo[1];
            tokenUrl = string.Format("{0}?tokenid={1}", tokenInfo[2], tokenId);
            Stream imageStream = new WebClient().OpenRead(tokenUrl);
            Image token = Image.FromStream(imageStream);
            imageStream.Close();
            return token;         
        }

        private int HttpRegisterAccount(string email, string password, string tokenID, string tokenValue) {
            string url = "http://register.gadu-gadu.pl/appsvc/fmregister3.asp";
            string hash = HttpHash(new string[] { email, password }).ToString();
            string query = string.Format("?pwd={0}&email={1}&tokenid={2}&tokenval={3}&code={4}", password, email, tokenID, tokenValue, hash);
            string response = HttpRequest(url + query, "POST", true); //reg_success:3730927
            if (response == null)
                return 0;
            Regex uinReg = new Regex(@"\d{2,}", RegexOptions.None);
            Match match = uinReg.Match(response);
            response = (match.Success) ? match.Value : "0";
            int result;
            return int.TryParse(response, out result) ? result : 0;
        }

        private bool HttpDeleteAccount(int GGNumber, string password, string tokenID, string tokenValue) {
            const string email = "deletedaccount@gadu-gadu.pl";
            string url = "http://register.gadu-gadu.pl/appsvc/fmregister3.asp";
            string hash = HttpHash(new string[] { password, email }).ToString();
            string query = string.Format("?fmnumber={0}&fmpwd={1}&delete=1&email={2}&pwd=%2D388046464&tokenid={3}&tokenval={4}&code={5}", GGNumber.ToString(), password, email, tokenID, tokenValue, hash);
            string response = HttpRequest(url + query, "POST", true); //reg_success:3730927
            return (response != null) ? response.Contains("success") : false;
        }

        private bool HttpChangePassword(int GGNumber, string password, string newPassword, string email, string tokenId, string tokenValue) {
            string url = "http://register.gadu-gadu.pl/appsvc/fmregister3.asp";
            string hash = HttpHash(new string[] { newPassword, email }).ToString();
            string query = string.Format("?fmnumber={0}&fmpwd={1}&pwd={2}&email={3}&tokenid={4}&tokenval={5}&code={6}", GGNumber.ToString(), password, newPassword, email, tokenId, tokenValue, hash);
            string response = HttpRequest(url + query, "POST", true); //reg_success:3730927
            return (response != null) ? response.Contains("success") : false;
        }

        private bool HttpPasswordRemind(int GGNumber, string tokenId, string tokenValue) {
            string url = "http://retr.gadu-gadu.pl/appsvc/fmsendpwd3.asp";
            string hash = HttpHash(new string[] { GGNumber.ToString() }).ToString();
            string query = string.Format("?userid={0}&tokenid={1}&tokenval={2}&code={3}", GGNumber.ToString(), tokenId, tokenValue, hash);
            string response = HttpRequest(url + query, "POST", true); // pwdsend_success
            return (response != null) ? response.Equals("pwdsend_success") : false;
        }




    }
}