using DsaVisual.Application.Common;

namespace DsaVisual.UnitTests;

/// <summary>Test PasswordPolicy + PasswordHasher (PBKDF2-SHA256 100k vòng).</summary>
public class PasswordPolicyTests
{
    [Fact]
    public void ValidPassword_Passes()
    {
        var errors = PasswordPolicy.Validate("MatKhau@123");
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("matkhau")]          // không chữ hoa/số/ký tự đặc biệt
    [InlineData("Matkhau123")]       // không ký tự đặc biệt
    [InlineData("matkhau@")]         // không chữ hoa, không số
    [InlineData("12345678")]         // không chữ hoa/ký tự đặc biệt
    public void WeakPasswords_Fail(string password)
    {
        var errors = PasswordPolicy.Validate(password);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void TooShort_FailsWithMinLengthMessage()
    {
        var errors = PasswordPolicy.Validate("Ab@1");
        Assert.Contains(errors, e => e.Contains("8"));
    }

    [Fact]
    public void Hash_Verify_RoundTrip()
    {
        var hash = PasswordHasher.Hash("MatKhau@123");
        Assert.StartsWith("PBKDF2-SHA256$", hash);
        Assert.True(PasswordHasher.Verify("MatKhau@123", hash));
        Assert.False(PasswordHasher.Verify("MatKhau@124", hash));
        Assert.False(PasswordHasher.Verify("MatKhau@123", "garbage"));
    }
}
