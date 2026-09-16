namespace Loupedeck.NotificationHapticSafePlugin
{
    using System;

    public sealed class NotificationHapticSafeApplication : ClientApplication
    {
        protected override String GetProcessName() => "";
        protected override String GetBundleName() => "";
        public override ClientApplicationStatus GetApplicationStatus() => ClientApplicationStatus.Unknown;
    }
}
