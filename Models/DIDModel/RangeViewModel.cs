namespace KBN.Models.DIDModel
{
    public class RangeViewModel
    {  
        public int? Id { get; set; }
        public long? DID { get; set; }
        public string City { get; set; } = null;
        public string Country { get; set; } = null;
        public string NumberType { get; set; } = null;

        public string? RecordAt { get; set; }

        public string? RecordedBy { get; set; }
    }
}
