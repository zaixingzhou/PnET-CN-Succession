﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtensionMethods
{
    public static class MyExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            if (string.IsNullOrEmpty(toCheck) || string.IsNullOrEmpty(source))
            {
                return true;
            }
            return source.IndexOf(toCheck, comp) >= 0;
        } 
    }
}
