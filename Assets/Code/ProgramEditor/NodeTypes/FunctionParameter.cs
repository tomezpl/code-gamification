using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[System.Serializable]
public class FunctionParameter : IProgramNode
{
    public string Name;

    // Literal value or result of evaluating an arithmetic expression
    public string Value;
    // Used for carrying over arithmetic expressions
    public string Expression;

    public string Type;
    public bool IsReference;
    public bool IsNull { get { return Name == null && Value == null && Type == null && !IsReference; } set { IsNull = value; } }

    public string Serialize()
    {
        return IsReference ? (string.IsNullOrWhiteSpace(Name) ? "" : Name) : (string.IsNullOrWhiteSpace(Expression) ? (string.IsNullOrWhiteSpace(Value) ? "" : Value) : Expression);
    }
}
