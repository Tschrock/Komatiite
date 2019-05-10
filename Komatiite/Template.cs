using System;
using System.Collections.Generic;

namespace Komatiite
{
    public class Template
    {
		internal Template()
		{
		}

        public static Template Parse(string source)
        {
            return new Template().ParseInternal(source);
        }

        private Template ParseInternal(string text)
        {
            return this;
        }



    }
}
