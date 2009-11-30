using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SOMAPC
{
    class LoginInfo
    {
        private static Regex loginRegex = new Regex(@"^(?<username>.+?)(:(?<password>.+?)){0,1}@(?<host>.+?)(:(?<path>.+?)){0,1}$");
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public static LoginInfo CreateFromString(string line)
        {
            Match m = loginRegex.Match(line);
            return new LoginInfo() { 
                Username = m.Groups["username"].Value, 
                Password = m.Groups["password"].Value, 
                Host = m.Groups["host"].Value, 
                Path = m.Groups["path"].Value };
        }
    }
}
