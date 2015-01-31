using Pretzel.Logic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Xunit;

namespace Pretzel.Tests
{
    public class SanityCheckTest
    {
        [Fact]
        public void IsLockedByAnotherProcess_File_Not_Exists_Returns_False()
        {
            var tempFile = Path.GetTempFileName();
            Assert.False(SanityCheck.IsLockedByAnotherProcess(tempFile));
        }

        [Fact]
        public void IsLockedByAnotherProcess_File_Exists_Returns_False()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.Create(tempFile).Close();
                Assert.False(SanityCheck.IsLockedByAnotherProcess(tempFile));
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }


        [Fact(Skip="Do not work on AppVeyor")]
        public void IsLockedByAnotherProcess_File_Is_Locked_Returns_True()
        {
            var tempFile = Path.GetTempFileName();
            var proc = new Process();
            proc.StartInfo = new ProcessStartInfo(@"LockFile.exe", tempFile);

            try
            {
                proc.Start();

                Assert.True(SanityCheck.IsLockedByAnotherProcess(tempFile));
            }
            finally
            {
                if (!proc.HasExited)
                {
                    proc.Kill();
                }
                while (!proc.HasExited) { }

                Thread.Sleep(1); // In order to unlock the file

                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
    }
}
