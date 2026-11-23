using System.Security.Cryptography;

namespace Application.Utils;

public static class CodigoVerificacaoUtil
{
    public static string GerarCodigo(int digitos = 6)
    {
        int min = (int)Math.Pow(10, digitos - 1);
        int max = (int)Math.Pow(10, digitos);
        return RandomNumberGenerator.GetInt32(min, max).ToString();
    }
}
