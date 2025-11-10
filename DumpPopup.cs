using System;
using System.Linq;
using System.Reflection;

class Dump
{
    static void Main()
    {
        var asm = Assembly.LoadFrom(@"C:\Program Files (x86)\Steam\steamapps\common\Caves of Qud\CoQ_Data\Managed\Assembly-CSharp.dll");
        var type = asm.GetType("XRL.UI.Popup");
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            if (method.Name == "Show")
            {
                var parameters = string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name));
                Console.WriteLine(string.Format("Show({0})", parameters));
            }
        }
    }
}
