namespace iPerfect.Models
{
    public class PRNUResult
    {
        public string BestCameraId { get; set; } = string.Empty;
        public double SimilarityScore { get; set; }
        public bool MatchesReference { get; set; }
    }
}
