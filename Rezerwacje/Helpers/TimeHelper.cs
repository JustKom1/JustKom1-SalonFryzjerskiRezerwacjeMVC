namespace Rezerwacje.Helpers
{
    public static class TimeHelper
    {
        public static string MinutesToText(int? minutes)
        {
            if (minutes == null || minutes <= 0) return "-";

            var mins = minutes.Value;
            var h = mins / 60;
            var m = mins % 60;

            if (h > 0 && m > 0) return $"{h}h {m}min";
            if (h > 0) return $"{h}h";
            return $"{m}min";
        }
    }
}
