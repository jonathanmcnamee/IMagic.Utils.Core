using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IMagic.Core.Tests
{
    [TestClass]
    public class DriveUtilTests
    {
        // WNetGetUniversalName(UNIVERSAL_NAME_INFO_LEVEL) fills a UNIVERSAL_NAME_INFO struct
        // whose single field is a pointer to the UNC string, not the string itself.
        [TestMethod]
        public void ReadUniversalName_FollowsPointerInUniversalNameInfo()
        {
            const string uncPath = @"\\server\share\folder";

            IntPtr stringPtr = Marshal.StringToHGlobalUni(uncPath);
            IntPtr buffer = Marshal.AllocHGlobal(IntPtr.Size);
            try
            {
                Marshal.WriteIntPtr(buffer, stringPtr);

                string actual = DriveUtil.ReadUniversalName(buffer);

                Assert.AreEqual(uncPath, actual);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
                Marshal.FreeHGlobal(stringPtr);
            }
        }

        [TestMethod]
        public void ReadUniversalName_ReturnsNullWhenUniversalNamePointerIsNull()
        {
            IntPtr buffer = Marshal.AllocHGlobal(IntPtr.Size);
            try
            {
                Marshal.WriteIntPtr(buffer, IntPtr.Zero);

                Assert.IsNull(DriveUtil.ReadUniversalName(buffer));
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        [TestMethod]
        public void ReadUniversalName_ReturnsNullForNullBuffer()
        {
            Assert.IsNull(DriveUtil.ReadUniversalName(IntPtr.Zero));
        }
    }
}
