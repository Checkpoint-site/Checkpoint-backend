using Domain.Enum;
using Domain.Exceptions.Common;

namespace Application.Utils;

public static class StatusJornadaHelper
{
    public static string? ParaSlug(StatusJornada? status) => status switch
    {
        StatusJornada.Zerado => "zerado",
        StatusJornada.Jogando => "jogando",
        StatusJornada.QueroJogar => "quero_jogar",
        StatusJornada.Abandonado => "abandonado",
        _ => null
    };

    public static StatusJornada? ConverterSlugOpcional(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        return slug.Trim().ToLowerInvariant() switch
        {
            "zerado" => StatusJornada.Zerado,
            "jogando" => StatusJornada.Jogando,
            "quero_jogar" or "quero-jogar" or "querojogar" => StatusJornada.QueroJogar,
            "abandonado" or "pausado" => StatusJornada.Abandonado,
            _ => throw new ValidationException($"Status de jornada '{slug}' é inválido. Valores aceitos: zerado, jogando, quero_jogar, abandonado.")
        };
    }
}
