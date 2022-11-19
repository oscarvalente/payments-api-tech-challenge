namespace PaymentsAPI.Utils
{
    public class Converter
    {
        public static byte[] bitConverterStringToByteArray(string s)
        {
            string[] strArray = s.Split('-');
            byte[] byteArray = new byte[strArray.Length];
            for (int i = 0; i < strArray.Length; i++)
            {
                byteArray[i] = Convert.ToByte(strArray[i], 16);
            }

            return byteArray;
        }
    }
}