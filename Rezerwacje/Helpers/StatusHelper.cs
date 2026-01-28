using Microsoft.AspNetCore.Html;

namespace Rezerwacje.Helpers
{
    public static class StatusHelper
    {
        public static string ToPl(string? status)
        {
            return (status ?? "").Trim() switch
            {
                "Pending" => "Oczekuje",
                "Approved" => "Zatwierdzona",
                "Rejected" => "Odrzucona",
                "Cancelled" => "Anulowana",
                _ => "-"
            };
        }

        public static string Css(string? status)
        {
            return (status ?? "").Trim() switch
            {
                "Pending" => "badge-pending",
                "Approved" => "badge-approved",
                "Rejected" => "badge-rejected",
                "Cancelled" => "badge-cancelled",
                _ => "badge-unknown"
            };
        }

        // opcja “w jednym”: zwraca gotowy <span> z klasami
        public static IHtmlContent Badge(string? status)
        {
            var text = ToPl(status);
            var css = Css(status);
            return new HtmlString($"<span class=\"badge {css}\">{text}</span>");
        }
    }
}
