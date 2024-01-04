namespace ANU.IngameDebug.Console.Dashboard
{
    public static class ConsolePlatformExtensions
    {
        public static TargetPlatforms GetCurrentPlatform()
        {
            var platforms = (TargetPlatforms)0;

#if UNITY_EDITOR
            platforms |= TargetPlatforms.Editor;
#endif
#if UNITY_ANDROID
            platforms |= TargetPlatforms.Mobile;
#endif
#if UNITY_IOS
            platforms |= TargetPlatforms.Mobile;
#endif
#if UNITY_STANDALONE
                platforms |= TargetPlatforms.PC;
#endif

            return platforms;
        }

        public static bool HasCurrentPlatform(this TargetPlatforms platforms)
        {
            var current = GetCurrentPlatform();
            return platforms.HasFlag(TargetPlatforms.Any)
                || (current & platforms) != 0;
        }
    }
}