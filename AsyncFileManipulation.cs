using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public static class AsyncFileManipulation
{
    public static async Task CopyFileAsync(
        string sourcePath, 
        string destinationPath, 
        bool overwrite = false, 
        CancellationToken cancellationToken = default)
    {
        const FileOptions fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
        const int bufferSize = 81920 * 2;
        
        try
        {
            await DeleteExistingFile(destinationPath, overwrite, cancellationToken);
                
            await using var sourceStream =
                new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, fileOptions);
            await using var destinationStream =
                new FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize,
                    fileOptions);
            await sourceStream.CopyToAsync(destinationStream, bufferSize, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(3));
            await Delete(destinationPath, cts.Token);
        }
        catch (Exception)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10));
            await Delete(destinationPath, cts.Token);
            throw;
        }
    }

    public static async Task MoveFileAsync(
        string sourcePath, 
        string destinationPath, 
        bool overwrite = false, 
        CancellationToken cancellationToken = default)
    {
        await CopyFileAsync(sourcePath, destinationPath, overwrite, cancellationToken);

        try
        {
            await Delete(sourcePath, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(3));
            await Delete(sourcePath, cts.Token);
        }
        catch (Exception)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10));
            await Delete(destinationPath, cts.Token);
            throw;
        }
    }
    
    private static async Task DeleteExistingFile(string path, bool deleteAllowed, CancellationToken cst)
    {
        if (deleteAllowed && File.Exists(path))
            await Delete(path, cst);
    }

    private static async Task Delete(string path, CancellationToken cts)
    {
        try
        {
            await Task.Run(() => File.Delete(path), cts);
        }
        catch (OperationCanceledException)
        { } // ignore
    }
}
