namespace LifeOS.Companion.Core.Security;

public interface IFieldProtector
{
    byte[] Protect(string plaintext);
    string Unprotect(byte[] protectedValue);
}
