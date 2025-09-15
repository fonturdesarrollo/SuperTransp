using System.Net;
using static SuperTransp.Core.Interfaces;

public class FtpService : IFtpService
{
    private readonly IConfiguration _configuration;
    public FtpService(IConfiguration configuration) => _configuration = configuration;

    private NetworkCredential GetCredentials() =>
        new NetworkCredential(_configuration["FtpSettings:Username"], _configuration["FtpSettings:Password"]);

    public async Task DeleteFilesInFolderAsync(string folderPath)
    {
        if (!await FolderExistsAsync(folderPath)) return;
        var files = await ListFilesAsync(folderPath);
        foreach (var file in files)
            await DeleteFileAsync($"{folderPath}/{file}");
        await DeleteFolderAsync(folderPath);
    }

    public async Task UploadFileAsync(Stream fileStream, string folderPath, string fileName)
    {
        if (!await FolderExistsAsync(folderPath))
            await CreateFolderAsync(folderPath);

        var ftpFilePath = $"{folderPath}/{fileName}";
        var request = (FtpWebRequest)WebRequest.Create(ftpFilePath);
        request.Method = WebRequestMethods.Ftp.UploadFile;
        request.Credentials = GetCredentials();

        using var requestStream = await request.GetRequestStreamAsync();
        await fileStream.CopyToAsync(requestStream);
    }

    public async Task<List<string>> ListFilesAsync(string folderPath)
    {
        var files = new List<string>();
        var request = (FtpWebRequest)WebRequest.Create(folderPath);
        request.Method = WebRequestMethods.Ftp.ListDirectory;
        request.Credentials = GetCredentials();

        using var response = (FtpWebResponse)await request.GetResponseAsync();
        using var reader = new StreamReader(response.GetResponseStream());
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
            files.Add(line);
        return files;
    }

    public async Task<bool> FolderExistsAsync(string folderPath)
    {
        try
        {
            var request = (FtpWebRequest)WebRequest.Create(folderPath);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = GetCredentials();
            using (await request.GetResponseAsync()) { return true; }
        }
        catch (WebException ex)
        {
            return ((FtpWebResponse)ex.Response).StatusCode != FtpStatusCode.ActionNotTakenFileUnavailable;
        }
    }

    public async Task CreateFolderAsync(string folderPath)
    {
        var request = (FtpWebRequest)WebRequest.Create(folderPath);
        request.Method = WebRequestMethods.Ftp.MakeDirectory;
        request.Credentials = GetCredentials();
        using (await request.GetResponseAsync()) { }
    }

    public async Task DeleteFolderAsync(string folderPath)
    {
        var request = (FtpWebRequest)WebRequest.Create(folderPath);
        request.Method = WebRequestMethods.Ftp.RemoveDirectory;
        request.Credentials = GetCredentials();
        using (await request.GetResponseAsync()) { }
    }

    public async Task TransferFileAsync(string sourcePath, string destinationPath)
    {
        var credentials = GetCredentials();
        var downloadRequest = (FtpWebRequest)WebRequest.Create(sourcePath);
        downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
        downloadRequest.Credentials = credentials;

        using var downloadResponse = (FtpWebResponse)await downloadRequest.GetResponseAsync();
        using var responseStream = downloadResponse.GetResponseStream();

        var uploadRequest = (FtpWebRequest)WebRequest.Create(destinationPath);
        uploadRequest.Method = WebRequestMethods.Ftp.UploadFile;
        uploadRequest.Credentials = credentials;

        using var requestStream = await uploadRequest.GetRequestStreamAsync();
        await responseStream.CopyToAsync(requestStream);
    }

    public async Task DeleteFileAsync(string filePath)
    {
        var request = (FtpWebRequest)WebRequest.Create(filePath);
        request.Method = WebRequestMethods.Ftp.DeleteFile;
        request.Credentials = GetCredentials();
        using (await request.GetResponseAsync()) { }
    }
}