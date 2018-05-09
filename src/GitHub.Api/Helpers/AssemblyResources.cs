﻿using System;
using System.Reflection;

namespace GitHub.Unity
{
    enum ResourceType
    {
        Icon,
        Platform,
        Generic
    }

    class AssemblyResources
    {
        public static NPath ToFile(ResourceType resourceType, string resource, NPath destinationPath, IEnvironment environment)
        {
            var os = "";
            if (resourceType == ResourceType.Platform)
            {
                os = DefaultEnvironment.OnWindows ? "windows"
                    : DefaultEnvironment.OnLinux ? "linux"
                        : "mac";
            }
            var type = resourceType == ResourceType.Icon ? "IconsAndLogos"
                : resourceType == ResourceType.Platform ? "PlatformResources"
                : "Resources";

            var asm = Assembly.GetCallingAssembly();
            if (!asm.FullName.StartsWith("IntegrationTests"))
                asm = typeof(AssemblyResources).Assembly;
            var stream = asm.GetManifestResourceStream(
                                     String.Format("GitHub.Unity.{0}{1}.{2}", type, !string.IsNullOrEmpty(os) ? "." + os : os, resource));
            if (stream != null)
                return destinationPath.Combine(resource).WriteAllBytes(stream.ToByteArray());

            // check the GitHub.Api assembly
            if (!asm.FullName.StartsWith("GitHub.Api"))
            {
                asm = typeof(ApplicationManagerBase).Assembly;
                stream = asm.GetManifestResourceStream(
                                        String.Format("GitHub.Unity.{0}{1}.{2}", type, !string.IsNullOrEmpty(os) ? "." + os : os, resource));
                if (stream != null)
                    return destinationPath.Combine(resource).WriteAllBytes(stream.ToByteArray());
            }

            if (!Guard.InUnitTestRunner)
            {
                // check the filesystem
                NPath possiblePath = environment.ExtensionInstallPath.Combine(type, os, resource);
                if (possiblePath.FileExists())
                    return possiblePath.Copy(destinationPath.Combine(resource));
            }
            return NPath.Default;
        }
    }
}
