
namespace KBN.Models.DIDModel
{
    public class DID_Assigning
    {
        public int Id {  get; set; }
        
        public long DID { get; set; }
        
        public  string City { get; set; }
        
        public string Country { get; set; }
        
        public int? ReqId { get; set; }

    }
}
