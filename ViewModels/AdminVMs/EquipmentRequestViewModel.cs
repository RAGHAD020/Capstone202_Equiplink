namespace EquipLink.ViewModels.AdminVMs
{
    public class EquipmentRequestViewModel
    {
        public int ReqId { get; set; }
        public string EquipmentName { get; set; }
        public string RequestedBy { get; set; }
        public DateOnly? RequestDate { get; set; }
        public string ApprovalStatus { get; set; }
        public string AdminNotes { get; set; }
    }
}
