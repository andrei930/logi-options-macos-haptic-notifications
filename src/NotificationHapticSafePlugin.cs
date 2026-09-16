namespace Loupedeck.NotificationHapticSafePlugin
{
    using System;

    public sealed class NotificationHapticSafePlugin : Plugin
    {
        public override Boolean UsesApplicationApiOnly => true;
        public override Boolean HasNoApplication => true;

        public NotificationHapticSafePlugin()
        {
            PluginLog.Init(this.Log);
        }

        public override void Load() { }
        public override void Unload() { }
    }
}
