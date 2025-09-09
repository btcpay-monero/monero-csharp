namespace Monero.Common;

public class SslOptions
{
    private string privateKeyPath;
    private string certificatePath;
    private string certificateAuthorityFile;
    private List<string> allowedFingerprints;
    private bool allowAnyCert;

    public string GetPrivateKeyPath()
    {
        return privateKeyPath;
    }

    public void SetPrivateKeyPath(string privateKeyPath)
    {
        this.privateKeyPath = privateKeyPath;
    }

    public string GetCertificatePath()
    {
        return certificatePath;
    }

    public void SetCertificatePath(string certificatePath)
    {
        this.certificatePath = certificatePath;
    }

    public string GetCertificateAuthorityFile()
    {
        return certificateAuthorityFile;
    }

    public void SetCertificateAuthorityFile(string certificateAuthorityFile)
    {
        this.certificateAuthorityFile = certificateAuthorityFile;
    }

    public List<string> GetAllowedFingerprints()
    {
        return allowedFingerprints;
    }

    public void SetAllowedFingerprints(List<string> allowedFingerprints)
    {
        this.allowedFingerprints = allowedFingerprints;
    }

    public bool GetAllowAnyCert()
    {
        return allowAnyCert;
    }

    public void SetAllowAnyCert(bool allowAnyCert)
    {
        this.allowAnyCert = allowAnyCert;
    }
}