﻿using System;

namespace SociallyDistant.Core.ContentEditors
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomViewAttribute : Attribute
    {
        public string Name { get; }

        public CustomViewAttribute(string name)
        {
            Name = name;
        }
    }
}