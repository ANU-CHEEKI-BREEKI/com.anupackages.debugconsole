namespace ANU.IngameDebug.Console.Dashboard
{
    public static class ConsolePlatformExtensions
    {
        // permissive: in the editor both Editor and the active build target match,
        // so platform-filtered commands stay testable in the editor
        public static TargetPlatforms GetCurrentPlatform()
        {
            var platforms = GetBuildTargetPlatform();

#if UNITY_EDITOR
            platforms |= TargetPlatforms.Editor;
#endif

            return platforms;
        }

        public static bool HasCurrentPlatform(this TargetPlatforms platforms)
            => platforms.HasFlag(TargetPlatforms.Any)
            || (GetCurrentPlatform() & platforms) != 0;

        // strict: the editor is only the editor - build-target defines like
        // UNITY_ANDROID are also set there and must not leak into device checks
        public static TargetPlatforms GetCurrentDevicePlatform()
        {
#if UNITY_EDITOR
            return TargetPlatforms.Editor;
#else
            return GetBuildTargetPlatform();
#endif
        }

        public static bool HasCurrentDevicePlatform(this TargetPlatforms platforms)
            => platforms.HasFlag(TargetPlatforms.Any)
            || (GetCurrentDevicePlatform() & platforms) != 0;

        private static TargetPlatforms GetBuildTargetPlatform()
        {
#if UNITY_ANDROID || UNITY_IOS
            return TargetPlatforms.Mobile;
#elif UNITY_WEBGL
            return TargetPlatforms.WebGL;
#elif UNITY_STANDALONE
            return TargetPlatforms.PC;
#else
            return (TargetPlatforms)0;
#endif
        }
    }
}
