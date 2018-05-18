using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Commands.Storage.Common;
using Microsoft.WindowsAzure.Commands.Storage.File;
using Microsoft.WindowsAzure.Commands.Storage.File.Cmdlet;
using Microsoft.WindowsAzure.Management.Storage.Test.Common;
using Microsoft.WindowsAzure.Storage.DataMovement;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using PSHFile = Microsoft.WindowsAzure.Commands.Storage.File;

namespace Microsoft.WindowsAzure.Management.Storage.Test.File.Cmdlet
{
    [TestClass]
    public class GetAzureStorageFileContentTest : StorageFileTestBase<GetAzureStorageFileContent>
    {
        private string destinationFilePath;

        private string destinationPath;

        [TestInitialize]
        public void DownloadInitialize()
        {
            destinationFilePath = Path.GetTempFileName();
            destinationPath = Path.GetTempPath();

            if (System.IO.File.Exists(destinationFilePath))
            {
                System.IO.File.Delete(destinationFilePath);
            }
        }

        [TestCleanup]
        public void DownloadCleanup()
        {
            if (System.IO.File.Exists(destinationFilePath))
            {
                System.IO.File.Delete(destinationFilePath);
            }
        }

        [TestMethod]
        public void DownloadFileUsingShareNameAndPathToLocalFileTest()
        {
            CmdletInstance.DisableDataCollection();
            DownloadFileInternal(
                "share",
                "remoteFile",
                destinationFilePath,
                () => CmdletInstance.RunCmdlet(
                    PSHFile.Constants.ShareNameParameterSetName,
                    new KeyValuePair<string, object>("ShareName", "share"),
                    new KeyValuePair<string, object>("Path", "remoteFile"),
                    new KeyValuePair<string, object>("Destination", destinationFilePath)));
        }

        [TestMethod]
        public void DownloadFileUsingShareObjectAndPathToLocalFileTest()
        {
            CmdletInstance.DisableDataCollection();
            DownloadFileInternal(
                "share",
                "remoteFile",
                destinationFilePath,
                () => CmdletInstance.RunCmdlet(
                    PSHFile.Constants.ShareParameterSetName,
                    new KeyValuePair<string, object>("Share", MockChannel.GetShareReference("share")),
                    new KeyValuePair<string, object>("Path", "remoteFile"),
                    new KeyValuePair<string, object>("Destination", destinationFilePath)));
        }

        [TestMethod]
        public void DownloadFileUsingDirectoryAndPathToLocalFileTest()
        {
            CmdletInstance.DisableDataCollection();
            DownloadFileInternal(
                "share",
                "remoteFile",
                destinationFilePath,
                () => CmdletInstance.RunCmdlet(
                    PSHFile.Constants.DirectoryParameterSetName,
                    new KeyValuePair<string, object>("Directory", MockChannel.GetShareReference("share").GetRootDirectoryReference()),
                    new KeyValuePair<string, object>("Path", "remoteFile"),
                    new KeyValuePair<string, object>("Destination", destinationFilePath)));
        }

        [TestMethod]
        public void DownloadFileUsingFileObjectToLocalFileTest()
        {
            CmdletInstance.DisableDataCollection();
            DownloadFileInternal(
                "share",
                "remoteFile",
                destinationFilePath,
                () => CmdletInstance.RunCmdlet(
                    PSHFile.Constants.FileParameterSetName,
                    new KeyValuePair<string, object>("File", MockChannel.GetShareReference("share").GetRootDirectoryReference().GetFileReference("remoteFile")),
                    new KeyValuePair<string, object>("Destination", destinationFilePath)));
        }

        [TestMethod]
        public void DownloadFileUsingFileObjectToLocalDirectoryTest()
        {
            CmdletInstance.DisableDataCollection();
            destinationFilePath = Path.Combine(destinationPath, "remoteFile");
            DownloadFileInternal(
                "share",
                "remoteFile",
                destinationFilePath,
                () => CmdletInstance.RunCmdlet(
                    PSHFile.Constants.FileParameterSetName,
                    new KeyValuePair<string, object>("File", MockChannel.GetShareReference("share").GetRootDirectoryReference().GetFileReference("remoteFile")),
                    new KeyValuePair<string, object>("Destination", destinationPath)));
        }

        private void DownloadFileInternal(string shareName, string fileName, string destination, Action downloadFileAction)
        {
            var mockupTransferManager = new DownloadTransferManager(
            (sourceFile, destPath) =>
                {
                    Assert.AreEqual(destination, destPath, "Destination validation failed.");
                    Assert.AreEqual(shareName, sourceFile.Share.Name, "Share validation failed.");
                    Assert.AreEqual(fileName, sourceFile.Name, "SourceFile validation failed.");
                });

            TransferManagerFactory.SetCachedTransferManager(mockupTransferManager);

            downloadFileAction();

            mockupTransferManager.ThrowAssertExceptionIfAvailable();
            MockCmdRunTime.OutputPipeline.AssertNoObject();
        }

        private sealed class DownloadTransferManager : MockTransferManager
        {
            private Action<CloudFile, string> validateAction;

            public DownloadTransferManager(Action<CloudFile, string> validate)
            {
                validateAction = validate;
            }

            public override Task DownloadAsync(CloudFile sourceFile, string destFilePath, DownloadOptions options, SingleTransferContext context, CancellationToken cancellationToken)
            {
                validateAction(sourceFile, destFilePath);
                return Task.FromResult(true);
            }
        }
    }
}
