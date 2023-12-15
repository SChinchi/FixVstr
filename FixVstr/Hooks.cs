using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using Console = RoR2.Console;

namespace FixVstr
{
    internal class Hooks
    {
        public static void Init()
        {
            IL.RoR2.Console.CCSetVstr += Console_CCSetVstr;
            IL.RoR2.Console.SubmitCmd_CmdSender_string_bool += Console_SubmitCmd_CmdSender_string_bool;
        }

        private static void Console_CCSetVstr(ILContext il)
        {
            var c = new ILCursor(il);
            if (!c.TryGotoNext(
                x => x.MatchCall<Console>("get_instance"),
                x => x.MatchLdfld<Console>("vstrs"),
                x => x.MatchLdarga(0),
                x => x.MatchLdcI4(0),
                x => x.MatchCall<ConCommandArgs>("get_Item"),
                x => x.MatchLdarga(0),
                x => x.MatchLdcI4(1),
                x => x.MatchCall<ConCommandArgs>("get_Item")/*,
                x => x.MatchCallvirt<Dictionary<string, string>>("Add")
                */
            ))
            {
                FixVstr.Logger.LogError("Failed to patch Console.CCSetVstr");
                return;
            }
            // The IL Matching above fails to match the call to Dictionary.Add for some reason,
            // so we are only matching the 8 lines before it but remove all 9. However, we cannot
            // be sure this patch will not break things if the last line is ever modified.
            c.RemoveRange(9);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<ConCommandArgs>>(args => 
            {
                var alias = args[0];
                if (Console.instance.concommandCatalog.ContainsKey(alias)
                    || Console.instance.allConVars.ContainsKey(alias))
                {
                    Debug.Log("Setting an already registered command/cvar as an alias is prohibited.");
                    return;
                }
                Console.instance.vstrs[alias] = args[1];
            });
        }

        private static void Console_SubmitCmd_CmdSender_string_bool(ILContext il)
        {
            var c = new ILCursor(il);
            if (!c.TryGotoNext(
                MoveType.After,
                x => x.MatchCallvirt(typeof(Console.Lexer), "GetTokens")
            ))
            {
                FixVstr.Logger.LogError("Failed to patch Console.SubmitCmd(CmdSender, string, bool)");
                return;
            }
            c.EmitDelegate<Func<Queue<string>, Queue<string>>>((tokens) =>
            {
                Queue<string> newTokens = new();
                // We need to keep track of the preceeding token, because we must not make a
                // substitution for the first argument of vstr related commands.
                string previousToken = "";
                while (tokens.Count > 0)
                {
                    var text = tokens.Dequeue();
                    if (!IsVstrRelatedCommand(previousToken) && Console.instance.vstrs.TryGetValue(text, out var result))
                    {
                        Queue<string> vstrTokens = new Console.Lexer(result).GetTokens();
                        // `Lexer.GetTokens` adds a semicolon at the end of parsing. By ignoring it,
                        // we can create an alias for any command partial. `tokens` has been parsed
                        // the same way, so we guarantee a terminating semicolon.
                        while (vstrTokens.Count > 1)
                        {
                            previousToken = vstrTokens.Dequeue();
                            newTokens.Enqueue(previousToken);
                        }
                    }
                    else
                    {
                        previousToken = text;
                        newTokens.Enqueue(text);

                    }
                }
                return newTokens;
            });
        }

        private static bool IsVstrRelatedCommand(string token)
        {
            return token.Equals("del_vstr") || token.Equals("get_vstr") || token.Equals("set_vstr");
        }
    }
}