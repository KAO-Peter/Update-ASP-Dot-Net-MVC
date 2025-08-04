
namespace HRPortal.DBEntities.DDMC_PFA
{

    public partial class PfaCycle
    {
        public bool IsSign { get; set; }
        public bool IsLock { get; set; }
        public bool IsEdit { get; set; }
        public bool canReject { get; set; }
        public string NowStatus { get; set; }
    }
}