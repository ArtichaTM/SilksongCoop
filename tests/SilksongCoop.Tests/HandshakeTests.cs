namespace SilksongCoop.Tests;

public class HandshakeTests
{
    private const string Version = "0.1.0";
    private const string Password = "pw";

    [Fact]
    public void MatchingVersionAndPasswordJoin()
    {
        string? reason = Handshake.Reject(Version, Version, Password, Password);
        Assert.Null(reason);
    }

    [Fact]
    public void VersionMismatchNamesBothVersions()
    {
        string? reason = Handshake.Reject(Version, "0.2.0", Password, Password);
        Assert.Equal("version mismatch: host 0.1.0, you 0.2.0", reason);
    }

    [Fact]
    public void WrongPasswordIsRejected()
    {
        string? reason = Handshake.Reject(Version, Version, Password, "wrong");
        Assert.Equal("wrong password", reason);
    }
}
