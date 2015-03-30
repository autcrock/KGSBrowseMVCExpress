namespace Well.Models.WellDataModel
{
    public class LogHeader
    {
        // A Log has a header describing metadata and data layout and meaning.

        public string Mnemonic { get; set; }

        public LogHeader()
        {
        }

        public LogHeader(string mnemonic)
        {
            Mnemonic = mnemonic;
        }
    }
}