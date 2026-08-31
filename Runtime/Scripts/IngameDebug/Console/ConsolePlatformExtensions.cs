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

        // unlike GetCurrentPlatform, the editor here is only the editor:
        // build-target defines like UNITY_ANDROID are also set in the editor,
        // so the permissive check can never hide anything from it
        public static TargetPlatforms GetCurrentDevicePlatform()
        {
#if UNITY_EDITOR
            return TargetPlatforms.Editor;
#elif UNITY_ANDROID || UNITY_IOS
            return TargetPlatforms.Mobile;
#elif UNITY_STANDALONE
            return TargetPlatforms.PC;
#else
            return (TargetPlatforms)0;
#endif
        }

        public static bool HasCurrentDevicePlatform(this TargetPlatforms platforms)
            => platforms.HasFlag(TargetPlatforms.Any)
            || (GetCurrentDevicePlatform() & platforms) != 0;
    }
}