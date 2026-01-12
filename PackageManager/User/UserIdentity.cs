using System;

namespace PackageManager.User;

using System.Runtime.InteropServices;

public static partial class UserIdentity
{
    [LibraryImport("libc")]
    private static partial uint getuid();
    
    [LibraryImport("libc", StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr getpwuid(uint uid);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct Passwd
    {
        public string pw_name;
        public string pw_passwd;
        public uint pw_uid;
        public uint pw_gid;
        public string pw_gecos;
        public string pw_dir;
        public string pw_shell;
    }

    public static bool IsRoot() => getuid() == 0;

    public static string GetRealUserHome()
    {
        string? sudoUser = Environment.GetEnvironmentVariable("SUDO_USER");
        if (!string.IsNullOrEmpty(sudoUser))
        {
            var ptr = getpwnam(sudoUser);
            if (ptr != IntPtr.Zero)
            {
                var pw = Marshal.PtrToStructure<Passwd>(ptr);
                return pw.pw_dir;
            }
        }

        string? pkexecUid = Environment.GetEnvironmentVariable("PKEXEC_UID");
        if (!string.IsNullOrEmpty(pkexecUid) && uint.TryParse(pkexecUid, out uint uid))
        {
            var ptr = getpwuid(uid);
            if (ptr != IntPtr.Zero)
            {
                var pw = Marshal.PtrToStructure<Passwd>(ptr);
                return pw.pw_dir;
            }
        }

        return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }

    [LibraryImport("libc", StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr getpwnam(string name);
}