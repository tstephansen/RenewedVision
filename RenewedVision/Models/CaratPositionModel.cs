namespace RenewedVision.Models
{
    public class CaratPositionModel
    {
        public int OriginalTextLength { get; set; }
        public int NewTextLength { get; set; }
        public int OriginalSpecialCharacterCount { get; set; }
        public int NewSpecialCharacterCount { get; set; }
        public int StartPosition { get; set; }
        public int PointerOffset => NewTextLength - OriginalTextLength;
        public int RunCount { get; set; }
    }
}
