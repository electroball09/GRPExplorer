using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.Logging;

namespace GRPExplorerLib.BigFile.Versions
{
    public class BigFileVersions
    {
        static IBigFileVersion[] versions = new IBigFileVersion[]
        {
            new BigFileVersion_GRP(),
            new BigFileVersion_GRFSDLC(),
        };

        public static IBigFileVersion GetVersion(short versionNum)
        {
            foreach (IBigFileVersion version in versions)
                if (version.Identifier == versionNum)
                    return version;

            throw new ArgumentException(string.Format("There's no version for number: {0:X4}", versionNum));
        }

        public static void DebugLogVersion(IBigFileVersion version, ILogProxy log)
        {
            log.Debug(" >BigFileVersion:");
            log.Debug("   Identifier: {0}", version.Identifier);
            log.Debug("   VersionName: {0}", version.VersionName);
        }
    }
}
