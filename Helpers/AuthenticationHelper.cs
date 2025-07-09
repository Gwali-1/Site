
using System.Security.Cryptography;
using System.Text;

public interface IAuthenticationHelper
{
    bool CompareHashes(ReadOnlySpan<byte> githubSignature, ReadOnlySpan<byte> calculatedSignature);
    byte[] GetHashofBody(string key, string payload);
    byte[] StringToByteArray(string hex);
}

public class AuthenticationHelper : IAuthenticationHelper
{
    public byte[] GetHashofBody(string key, string payload)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var payLoadBytes = Encoding.UTF8.GetBytes(payload);


        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(payLoadBytes);

        return hash;
    }


    public bool CompareHashes(ReadOnlySpan<byte> githubSignature, ReadOnlySpan<byte> calculatedSignature)
    {
        return CryptographicOperations.FixedTimeEquals(githubSignature, calculatedSignature);
    }

    public byte[] StringToByteArray(string hex)
    {
        int length = hex.Length;
        var bytes = new byte[length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }

}

