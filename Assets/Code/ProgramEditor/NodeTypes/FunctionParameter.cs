﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[System.Serializable]
public class FunctionParameter
{
    public string Name;
    public string Value;
    public string Type;
    public bool IsReference;
    public bool IsNull { get { return Name == null && Value == null && Type == null && !IsReference; } set { IsNull = value; } }
}
