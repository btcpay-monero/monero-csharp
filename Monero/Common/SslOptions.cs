namespace Monero.Common;

public class SslOptions
{
    private bool _allowAnyCert;
    private List<string>? _allowedFingerprints;
    private string? _certificateAuthorityFile;
    private string? _certificatePath;
    private string? _privateKeyPath;

    public string? GetPrivateKeyPath()
    {
        return _privateKeyPath;
    }

    public void SetPrivateKeyPath(string privateKeyPath)
    {
        _privateKeyPath = privateKeyPath;
    }

    public string? GetCertificatePath()
    {
        return _certificatePath;
    }

    public void SetCertificatePath(string certificatePath)
    {
        _certificatePath = certificatePath;
    }

    public string? GetCertificateAuthorityFile()
    {
        return _certificateAuthorityFile;
    }

    public void SetCertificateAuthorityFile(string certificateAuthorityFile)
    {
        _certificateAuthorityFile = certificateAuthorityFile;
    }

    public List<string>? GetAllowedFingerprints()
    {
        return _allowedFingerprints;
    }

    public void SetAllowedFingerprints(List<string> allowedFingerprints)
    {
        _allowedFingerprints = allowedFingerprints;
    }

    public bool GetAllowAnyCert()
    {
        return _allowAnyCert;
    }

    public void SetAllowAnyCert(bool allowAnyCert)
    {
        _allowAnyCert = allowAnyCert;
    }
}