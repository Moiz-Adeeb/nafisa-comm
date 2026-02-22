namespace Common.Util
{
    public class GeoUtil
    {
        public static double Distance(double lat1, double lon1, double lat2, double lon2, char unit)
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                return 0;
            }
            else
            {
                double theta = lon1 - lon2;
                double dist =
                    Math.Sin(DegreeToRadian(lat1)) * Math.Sin(DegreeToRadian(lat2))
                    + Math.Cos(DegreeToRadian(lat1))
                        * Math.Cos(DegreeToRadian(lat2))
                        * Math.Cos(DegreeToRadian(theta));
                dist = Math.Acos(dist);
                dist = RadianToDegree(dist);
                dist = dist * 60 * 1.1515;
                if (unit == 'K')
                {
                    dist = dist * 1.609344;
                }
                else if (unit == 'N')
                {
                    dist = dist * 0.8684;
                }
                return (dist);
            }
        }

        public static double DegreeToRadian(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        public static double RadianToDegree(double rad)
        {
            return (rad / Math.PI * 180.0);
        }
    }
}
