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
        this._privateKeyPath = privateKeyPath;
    }

    public string? GetCertificatePath()
    {
        return _certificatePath;
    }

    public void SetCertificatePath(string certificatePath)
    {
        this._certificatePath = certificatePath;
    }

    public string? GetCertificateAuthorityFile()
    {
        return _certificateAuthorityFile;
    }

    public void SetCertificateAuthorityFile(string certificateAuthorityFile)
    {
        this._certificateAuthorityFile = certificateAuthorityFile;
    }

    public List<string>? GetAllowedFingerprints()
    {
        return _allowedFingerprints;
    }

    public void SetAllowedFingerprints(List<string> allowedFingerprints)
    {
        this._allowedFingerprints = allowedFingerprints;
    }

    public bool GetAllowAnyCert()
    {
        return _allowAnyCert;
    }

    public void SetAllowAnyCert(bool allowAnyCert)
    {
        this._allowAnyCert = allowAnyCert;
    }
}