using BepInEx;
using R2API.Utils;
using System.Security.Permissions;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace FixVstr
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    public class FixVstr : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Chinchi";
        public const string PluginName = "FixVstr";
        public const string PluginVersion = "1.0.0";

        internal static new BepInEx.Logging.ManualLogSource Logger;

        public void Awake()
        {
            Logger = base.Logger;
            Hooks.Init();
        }
    }
}