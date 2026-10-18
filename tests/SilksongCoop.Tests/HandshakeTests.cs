namespace SilksongCoop.Tests;

public class HandshakeTests
{
    private static readonly Credentials Host = new("0.1.0", "1.0.30000 (abcd1234)", "pw");

    [Fact]
    public void MatchingCredentialsJoin()
    {
        string? reason = Handshake.Reject(Host, Host);
        Assert.Null(reason);
    }

    [Fact]
    public void VersionMismatchNamesBothVersions()
    {
        string? reason = Handshake.Reject(Host, Host with { Version = "0.2.0" });
        Assert.Equal("mod version mismatch: host 0.1.0, you 0.2.0", reason);
    }

    [Fact]
    public void BuildMismatchNamesBothBuilds()
    {
        string? reason = Handshake.Reject(Host, Host with { Build = "1.0.29000 (ffff0000)" });
        Assert.Equal("game build mismatch: host 1.0.30000 (abcd1234), you 1.0.29000 (ffff0000)", reason);
    }

    [Fact]
    public void WrongPasswordIsRejected()
    {
        string? reason = Handshake.Reject(Host, Host with { Password = "wrong" });
        Assert.Equal("wrong password", reason);
    }
}
