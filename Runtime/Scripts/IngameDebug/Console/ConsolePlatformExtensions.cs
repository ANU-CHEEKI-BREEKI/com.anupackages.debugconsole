namespace ANU.IngameDebug.Console.Dashboard
{
    public static class ConsolePlatformExtensions
    {
        public static TargetPlatforms GetCurrentPlatform(this TargetPlatforms platforms)
        {
            platforms = (TargetPlatforms)0;

#if UNITY_EDITOR
            platforms |= TargetPlatforms.Editor;
#endif
#if UNITY_ANDROID
            platforms |= TargetPlatforms.Mobile;
#endif
#if UNITY_IOS
                platforms |= ConsolePlatform.Mobile;
#endif
#if UNITY_STANDALONE
                platforms |= ConsolePlatform.PC;
#endif

            return platforms;
        }
    }
}