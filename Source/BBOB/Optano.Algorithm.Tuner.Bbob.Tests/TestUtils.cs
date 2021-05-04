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
        /// Tries to resolve a python 2.7 binary from the PATH environment variables.
        /// </summary>
        /// <param name="pythonBinary">The python 2.7 binary.</param>
        /// <returns>True, if the python 2.7 binary exists.</returns>
        public static bool TryToResolvePython27BinaryFromPath(out FileInfo pythonBinary)
        {
            pythonBinary = null;

            var pathList = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            foreach (var path in pathList.Split(";"))
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                var directory = new DirectoryInfo(path);

                if (!directory.Exists)
                {
                    continue;
                }

                if (TestUtils.TryToResolvePython27BinaryFromDirectory(directory, out pythonBinary))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Tries to resolve a python 2.7 binary from the given directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="pythonBinary">The python 2.7 binary.</param>
        /// <returns>True, if the python 2.7 binary exists.</returns>
        private static bool TryToResolvePython27BinaryFromDirectory(DirectoryInfo directory, out FileInfo pythonBinary)
        {
            pythonBinary = null;

            var pythonBinaryName = "python";
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                pythonBinaryName += ".exe";
            }

            var allFiles = directory.EnumerateFiles().ToList();
            foreach (var file in allFiles)
            {
                if (!file.Name.Equals(pythonBinaryName))
                {
                    continue;
                }

                var versionInfo = FileVersionInfo.GetVersionInfo(file.FullName);

                if (versionInfo.ProductVersion?.StartsWith("2") ?? false)
                {
                    pythonBinary = file;
                    return true;
                }

                if (versionInfo.FileVersion?.StartsWith("2") ?? false)
                {
                    pythonBinary = file;
                    return true;
                }

                // Our version of portable python does not contain any file or product version info. So we need to fall back to this crude approach.
                if (file.FullName.Contains("-2.7") || file.FullName.Contains("Python27"))
                {
                    pythonBinary = file;
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}