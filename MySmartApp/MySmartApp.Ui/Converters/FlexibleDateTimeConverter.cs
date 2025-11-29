using System.Text.Json;
using System.Text.Json.Serialization;

namespace MySmartApp.Ui.Converters;

/// <summary>
/// Convertisseur JSON flexible pour g�rer diff�rents formats de date/heure
/// Compatible avec Blazor WebAssembly et les variations de format du backend
/// </summary>
public class FlexibleDateTimeConverter : JsonConverter<DateTime>
{
    private readonly string[] _formats = new[]
    {
        // Formats ISO 8601
        "yyyy-MM-ddTHH:mm:ss.fffffffZ",
        "yyyy-MM-ddTHH:mm:ss.ffffffZ",
        "yyyy-MM-ddTHH:mm:ss.fffffZ",
        "yyyy-MM-ddTHH:mm:ss.ffffZ",
        "yyyy-MM-ddTHH:mm:ss.fffZ",
        "yyyy-MM-ddTHH:mm:ss.ffZ",
        "yyyy-MM-ddTHH:mm:ss.fZ",
        "yyyy-MM-ddTHH:mm:ssZ",
        
        // Formats avec offset
        "yyyy-MM-ddTHH:mm:ss.fffffffzzz",
        "yyyy-MM-ddTHH:mm:ss.ffffffzzz",
        "yyyy-MM-ddTHH:mm:ss.fffffzzz",
        "yyyy-MM-ddTHH:mm:ss.ffffzzz",
        "yyyy-MM-ddTHH:mm:ss.fffzzz",
        "yyyy-MM-ddTHH:mm:ss.ffzzz",
        "yyyy-MM-ddTHH:mm:ss.fzzz",
        "yyyy-MM-ddTHH:mm:sszzz",
        
        // Formats sans secondes fractionnaires
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd",
        
        // Format .NET par d�faut
        "O", // Round-trip format
        "o"  // Round-trip format (lowercase)
    };

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return DateTime.MinValue;
        }

        var dateString = reader.GetString();
        
        if (string.IsNullOrWhiteSpace(dateString))
        {
            return DateTime.MinValue;
        }

        // Essayer DateTime.Parse en premier (le plus flexible)
        if (DateTime.TryParse(dateString, null, System.Globalization.DateTimeStyles.RoundtripKind, out var result))
        {
            return result;
        }

        // Essayer les formats sp�cifiques
        foreach (var format in _formats)
        {
            if (DateTime.TryParseExact(dateString, format, null, System.Globalization.DateTimeStyles.RoundtripKind, out result))
            {
                return result;
            }
        }

        // Derni�re tentative: parse universel
        if (DateTime.TryParse(dateString, out result))
        {
            return result;
        }

        // Si tout �choue, retourner DateTime.MinValue plut�t que lever une exception
        return DateTime.MinValue;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // �crire au format ISO 8601 standard
        writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"));
    }
}

/// <summary>
/// Convertisseur JSON flexible pour DateTime nullable
/// </summary>
public class FlexibleNullableDateTimeConverter : JsonConverter<DateTime?>
{
    private readonly FlexibleDateTimeConverter _innerConverter = new();

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        var result = _innerConverter.Read(ref reader, typeof(DateTime), options);
        return result == DateTime.MinValue ? null : result;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            _innerConverter.Write(writer, value.Value, options);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
