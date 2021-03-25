#region Copyright (c) OPTANO GmbH

// ////////////////////////////////////////////////////////////////////////////////
// 
//        OPTANO GmbH Source Code
//        Copyright (c) 2010-2021 OPTANO GmbH
//        ALL RIGHTS RESERVED.
// 
//    The entire contents of this file is protected by German and
//    International Copyright Laws. Unauthorized reproduction,
//    reverse-engineering, and distribution of all or any portion of
//    the code contained in this file is strictly prohibited and may
//    result in severe civil and criminal penalties and will be
//    prosecuted to the maximum extent possible under the law.
// 
//    RESTRICTIONS
// 
//    THIS SOURCE CODE AND ALL RESULTING INTERMEDIATE FILES
//    ARE CONFIDENTIAL AND PROPRIETARY TRADE SECRETS OF
//    OPTANO GMBH.
// 
//    THE SOURCE CODE CONTAINED WITHIN THIS FILE AND ALL RELATED
//    FILES OR ANY PORTION OF ITS CONTENTS SHALL AT NO TIME BE
//    COPIED, TRANSFERRED, SOLD, DISTRIBUTED, OR OTHERWISE MADE
//    AVAILABLE TO OTHER INDIVIDUALS WITHOUT WRITTEN CONSENT
//    AND PERMISSION FROM OPTANO GMBH.
// 
// ////////////////////////////////////////////////////////////////////////////////

#endregion

namespace Optano.Algorithm.Tuner.Bbob.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Contains utility methods that are helpful in tests.
    /// </summary>
    public static class TestUtils
    {
        #region Public Methods and Operators

        /// <summary>
        /// Checks console output on invoking a certain action.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="check">Checks to do on the output.</param>
        public static void CheckOutput(Action action, Action<StringWriter> check)
        {
            using StringWriter consoleOutput = new StringWriter();
            // Redirect console output.
            var originalOut = Console.Out;
            Console.SetOut(consoleOutput);

            // Execute action.
            action.Invoke();

            try
            {
                // Execute check.
                check.Invoke(consoleOutput);
            }
            finally
            {
                // Don't forget to redo the output redirect.
                Console.SetOut(originalOut);
            }
        }

        /// <summary>
        /// Tries to resolve a python 2.7 binary from the PATH environment variables.
        /// </summary>
        /// <returns>The python binary.</returns>
        public static FileInfo ResolvePython27Binary()
        {
            var binaryName = "python";
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                binaryName += ".exe";
            }

            var pathList = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;

            foreach (var path in pathList.Split(";"))
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                var dir = new DirectoryInfo(path);

                if (!dir.Exists)
                {
                    continue;
                }

                if (TestUtils.TryResolvePythonBinaryFromDirectory(binaryName, dir, out var pythonBinary))
                {
                    return pythonBinary;
                }
            }

            throw new InvalidOperationException(
                $"Could not resolve a python 2.7 binary from your PATH variable. Make sure to add a reference to your installed Python 2.7 folder.");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Looks for a file named <paramref name="binaryName"/> in the given directory.
        /// </summary>
        /// <param name="binaryName">The binary name to look up.</param>
        /// <param name="dir">The target directory.</param>
        /// <param name="pythonBinary">The python binary.</param>
        /// <returns>True, if the python binary exists.</returns>
        private static bool TryResolvePythonBinaryFromDirectory(string binaryName, DirectoryInfo dir, out FileInfo pythonBinary)
        {
            var allFiles = dir.EnumerateFiles().ToList();
            pythonBinary = null;
            foreach (var file in allFiles)
            {
                if (!file.Name.Equals(binaryName))
                {
                    continue;
                }

                var versionInfo = FileVersionInfo.GetVersionInfo(file.FullName);

                if (versionInfo.ProductVersion?.StartsWith("2") ?? false)
                {
                    pythonBinary = file;
                }

                if (versionInfo.FileVersion?.StartsWith("2") ?? false)
                {
                    pythonBinary = file;
                }

                // my version of portable python does not contain any file or product version info. So I need to fall back to this crude approach
                if (file.FullName.Contains("-2.7") || file.FullName.Contains("Python27"))
                {
                    pythonBinary = file;
                }
            }

            return pythonBinary != null;
        }

        #endregion
    }
}