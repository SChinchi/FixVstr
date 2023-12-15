using RoR2;
using System.Linq;
using System.Text;
using UnityEngine;
using Console = RoR2.Console;

namespace FixVstr
{
    internal class Commands
    {
        [ConCommand(commandName = "clear_vstr", flags = ConVarFlags.None, helpText = "Clear all registered vstrs.")]
        public static void CCClearVstr(ConCommandArgs _)
        {
            Console.instance.vstrs.Clear();
        }

        [ConCommand(commandName = "del_vstr", flags = ConVarFlags.None, helpText = "Removes a specific vstr.")]
        public static void CCDelVstr(ConCommandArgs args)
        {
            args.CheckArgumentCount(1);
            Console.instance.vstrs.Remove(args[0]);
        }

        [ConCommand(commandName = "get_vstr", flags = ConVarFlags.None, helpText = "Print the specified vstr. If no argument is provided, it prints them all.")]
        public static void CCGetVstr(ConCommandArgs args)
        {
            if (args.Count > 0)
            {
                if (Console.instance.vstrs.TryGetValue(args[0], out var vstr))
                {
                    Debug.Log($"{args[0]} = {vstr}");
                }
                else
                {
                    Debug.Log("vstr not found");
                }
                return;
            }
            var vstrs = Console.instance.vstrs;
            var aliases = vstrs.Keys.ToList();
            aliases.Sort();
            var s = new StringBuilder();
            foreach (var alias in aliases)
            {
                s.AppendLine($"{alias} = {vstrs[alias]}");
            }
            Debug.Log(s.ToString().TrimEnd('\n'));
        }
    }
}