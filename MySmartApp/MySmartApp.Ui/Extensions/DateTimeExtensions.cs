namespace MySmartApp.Ui.Extensions;

/// <summary>
/// Extensions pour faciliter la manipulation et l'affichage des DateTime
/// Compatible avec Blazor WebAssembly
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// V�rifie si une date est valide (ni MinValue, ni MaxValue)
    /// </summary>
    public static bool IsValid(this DateTime date)
    {
        return date != DateTime.MinValue && date != DateTime.MaxValue;
    }

    /// <summary>
    /// V�rifie si une date nullable est valide
    /// </summary>
    public static bool IsValid(this DateTime? date)
    {
        return date.HasValue && date.Value.IsValid();
    }

    /// <summary>
    /// Convertit une date en cha�ne "friendly" (Il y a X min/h/jours)
    /// </summary>
    /// <param name="date">Date � convertir</param>
    /// <param name="fallback">Texte � afficher si la date est invalide</param>
    public static string ToFriendlyString(this DateTime date, string fallback = "Date inconnue")
    {
        if (!date.IsValid())
            return fallback;

        var now = DateTime.UtcNow;
        var diff = now - date.ToUniversalTime();

        if (diff.TotalSeconds < 0)
            return "Dans le futur";

        if (diff.TotalSeconds < 60)
            return "� l'instant";

        if (diff.TotalMinutes < 60)
        {
            var minutes = (int)diff.TotalMinutes;
            return $"Il y a {minutes} min";
        }

        if (diff.TotalHours < 24)
        {
            var hours = (int)diff.TotalHours;
            return $"Il y a {hours} h";
        }

        if (diff.TotalDays < 7)
        {
            var days = (int)diff.TotalDays;
            return days == 1 ? "Hier" : $"Il y a {days} jours";
        }

        if (diff.TotalDays < 30)
        {
            var weeks = (int)(diff.TotalDays / 7);
            return weeks == 1 ? "Il y a 1 semaine" : $"Il y a {weeks} semaines";
        }

        if (diff.TotalDays < 365)
        {
            var months = (int)(diff.TotalDays / 30);
            return months == 1 ? "Il y a 1 mois" : $"Il y a {months} mois";
        }

        var years = (int)(diff.TotalDays / 365);
        return years == 1 ? "Il y a 1 an" : $"Il y a {years} ans";
    }

    /// <summary>
    /// Convertit une date nullable en cha�ne "friendly"
    /// </summary>
    public static string ToFriendlyString(this DateTime? date, string fallback = "Date inconnue")
    {
        return date.HasValue ? date.Value.ToFriendlyString(fallback) : fallback;
    }

    /// <summary>
    /// Formate une date au format fran�ais court (dd/MM/yyyy)
    /// </summary>
    public static string ToShortFrenchString(this DateTime date, string fallback = "-")
    {
        return date.IsValid() ? date.ToString("dd/MM/yyyy") : fallback;
    }

    /// <summary>
    /// Formate une date au format fran�ais long (dd/MM/yyyy HH:mm)
    /// </summary>
    public static string ToLongFrenchString(this DateTime date, string fallback = "-")
    {
        return date.IsValid() ? date.ToString("dd/MM/yyyy HH:mm") : fallback;
    }

    /// <summary>
    /// Formate une date au format fran�ais complet (dd MMMM yyyy � HH:mm)
    /// </summary>
    public static string ToFullFrenchString(this DateTime date, string fallback = "-")
    {
        if (!date.IsValid())
            return fallback;

        var localDate = date.ToLocalTime();
        var monthName = GetFrenchMonthName(localDate.Month);
        return $"{localDate.Day} {monthName} {localDate.Year} � {localDate:HH:mm}";
    }

    /// <summary>
    /// Retourne le nom du mois en fran�ais
    /// </summary>
    private static string GetFrenchMonthName(int month)
    {
        return month switch
        {
            1 => "janvier",
            2 => "f�vrier",
            3 => "mars",
            4 => "avril",
            5 => "mai",
            6 => "juin",
            7 => "juillet",
            8 => "ao�t",
            9 => "septembre",
            10 => "octobre",
            11 => "novembre",
            12 => "d�cembre",
            _ => ""
        };
    }

    /// <summary>
    /// Convertit une date en format ISO 8601 pour l'API
    /// </summary>
    public static string ToApiString(this DateTime date)
    {
        return date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
    }

    /// <summary>
    /// V�rifie si une date est aujourd'hui
    /// </summary>
    public static bool IsToday(this DateTime date)
    {
        if (!date.IsValid())
            return false;

        var today = DateTime.Today;
        var dateLocal = date.ToLocalTime().Date;
        return dateLocal == today;
    }

    /// <summary>
    /// V�rifie si une date est dans le pass�
    /// </summary>
    public static bool IsPast(this DateTime date)
    {
        if (!date.IsValid())
            return false;

        return date < DateTime.UtcNow;
    }

    /// <summary>
    /// V�rifie si une date est dans le futur
    /// </summary>
    public static bool IsFuture(this DateTime date)
    {
        if (!date.IsValid())
            return false;

        return date > DateTime.UtcNow;
    }

    /// <summary>
    /// Retourne une ic�ne MudBlazor appropri�e selon l'�ge de la date
    /// </summary>
    public static string GetAgeIcon(this DateTime date)
    {
        if (!date.IsValid())
            return "fa-question";

        var age = DateTime.UtcNow - date.ToUniversalTime();

        if (age.TotalMinutes < 5)
            return "fa-clock";  // Tr�s r�cent

        if (age.TotalHours < 1)
            return "fa-history";  // R�cent

        if (age.TotalDays < 1)
            return "fa-calendar-day";  // Aujourd'hui

        if (age.TotalDays < 7)
            return "fa-calendar-week";  // Cette semaine

        return "fa-calendar";  // Ancien
    }

    /// <summary>
    /// Retourne une couleur appropri�e selon l'�ge de la date
    /// </summary>
    public static string GetAgeColor(this DateTime date)
    {
        if (!date.IsValid())
            return "gray";

        var age = DateTime.UtcNow - date.ToUniversalTime();

        if (age.TotalMinutes < 5)
            return "success";  // Vert - Tr�s r�cent

        if (age.TotalHours < 1)
            return "info";  // Bleu - R�cent

        if (age.TotalDays < 1)
            return "warning";  // Orange - Aujourd'hui

        return "error";  // Rouge - Ancien
    }

    /// <summary>
    /// Calcule la dur�e entre deux dates au format lisible
    /// </summary>
    public static string GetDurationString(this DateTime start, DateTime end)
    {
        if (!start.IsValid() || !end.IsValid())
            return "-";

        var duration = end - start;

        if (duration.TotalSeconds < 60)
            return $"{(int)duration.TotalSeconds} sec";

        if (duration.TotalMinutes < 60)
            return $"{(int)duration.TotalMinutes} min";

        if (duration.TotalHours < 24)
            return $"{(int)duration.TotalHours} h {duration.Minutes} min";

        if (duration.TotalDays < 7)
            return $"{(int)duration.TotalDays} j {duration.Hours} h";

        return $"{(int)duration.TotalDays} jours";
    }

    /// <summary>
    /// Retourne la date de d�but de la journ�e (00:00:00)
    /// </summary>
    public static DateTime StartOfDay(this DateTime date)
    {
        return date.Date;
    }

    /// <summary>
    /// Retourne la date de fin de la journ�e (23:59:59.999)
    /// </summary>
    public static DateTime EndOfDay(this DateTime date)
    {
        return date.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Retourne la date de d�but de la semaine (lundi)
    /// </summary>
    public static DateTime StartOfWeek(this DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    /// <summary>
    /// Retourne la date de d�but du mois
    /// </summary>
    public static DateTime StartOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1);
    }

    /// <summary>
    /// Retourne la date de fin du mois
    /// </summary>
    public static DateTime EndOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
    }
}
