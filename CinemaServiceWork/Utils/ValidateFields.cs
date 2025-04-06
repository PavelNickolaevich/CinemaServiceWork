using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CinemaServiceWork.Utils
{
    public static class ValidateFields
    {
        public static bool CheckIsValidEmail(string email)
        {
            const string pattern = "[a-zA-Z0-9._-]+@[a-zA-Z0-9._-]+\\.[a-zA-Z0-9_-]+";
            return Regex.IsMatch(email, pattern);

        }

    }
}
