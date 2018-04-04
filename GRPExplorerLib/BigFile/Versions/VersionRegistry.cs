using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRPExplorerLib.BigFile.Versions
{
    public class VersionRegistry
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
    }
}
