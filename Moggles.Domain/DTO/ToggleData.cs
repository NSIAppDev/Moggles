namespace Moggles.Domain.DTO
{
    public struct ToggleData
    {
        public string ToggleName { get; set; }
        public bool UserAccepted { get; set; }
        public string Notes { get; set; }
        public bool IsPermanent { get; set; }
    }
}