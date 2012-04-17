using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcApplication1.Utils
{
    public static class StringExtensions
    {
        public static bool HasSomething(this string s)
        {
            return !string.IsNullOrEmpty(s);
        }
    }
}