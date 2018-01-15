namespace LargeFileUploader
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {

        static void Main(string[] args)
        {
            FolderUploader().Wait();
        }

        public static async Task FolderUploader()
        {
            LargeFileUploaderUtils.Log = Console.Out.WriteLine;
            LargeFileUploaderUtils.NumBytesPerChunk = 1 * 1024 * 1024;

            var accountName = "<Enter Account Name>";
            var keyValue = "<Enter Key Value>";
            var useHttps = true;
            var exportSecrets = true;


            ///// Case 1: 
            //var storageCredentials = new StorageCredentials(accountName, keyValue);
            //var storageAccount = new CloudStorageAccount(storageCredentials, useHttps);
            //var connectionString = storageAccount.ToString(exportSecrets);


            ///// Case 2:
            //var blobClient = storageAccount.CreateCloudBlobClient();
            //var container = blobClient.GetContainerReference("container" + Guid.NewGuid().ToString("N").ToLowerInvariant());
            //await container.CreateIfNotExistsAsync().ConfigureAwait(false);


            Console.WriteLine("Enter folder path to upload to Azure");
            string baseDir = Console.ReadLine();

            var files = Directory.GetFiles(baseDir);

            using (var semaphoreSlim = new SemaphoreSlim(2))
            {
                var tasks = files.Select(async file =>
                {
                    await semaphoreSlim.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        // Case 1
                        //// Uploading files in chunks thereby resulting in increase of Upload speed
                        
                        //LargeFileUploaderUtils.UploadAsync(
                        //         inputFile: file,
                        //         storageConnectionString: connectionString,
                        //         containerName: "programflow",
                        //         uploadParallelism: 6).Wait();

                        //////////////////////

                        // Case 2
                        //// Uploading the file as a single entity (more time consuming during upload)

                        //var blobName = ConvertToRelativeUri(file, baseDir);
                        //var blob = container.GetBlockBlobReference(blobName);

                        //using (var fileStream = File.OpenRead(file))
                        //{
                        //    await blob.UploadFromStreamAsync(fileStream).ConfigureAwait(false);
                        //}

                        //////////////////
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                });

                await Task.WhenAll(tasks).ConfigureAwait(false);

            }
        }

        private static string ConvertToRelativeUri(string filePath, string baseDir)
        {
            var uri = new Uri(filePath);
            if (!baseDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                baseDir += Path.DirectorySeparatorChar.ToString();
            }
            var baseUri = new Uri(baseDir);
            return baseUri.MakeRelativeUri(uri).ToString();
        }
    }
}