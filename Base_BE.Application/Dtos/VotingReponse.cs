namespace Base_BE.Application.Dtos
{
    public class VotingReponse
    {
        public Guid Id { get; set; }
        public required string VoteName { get; set; }
        public int MaxCandidateVote { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public Guid PositionId { get; set; }
        public string PositionName { get; set; }
        public string Status { get; set; }
        public required string Tenure { get; set; }
        public DateTime StartDateTenure { get; set; }
        public DateTime EndDateTenure { get; set; }
        public string ExtraData { get; set; }
        public List<string> Candidates { get; set; }
        public List<string> CandidateNames { get; set; }
        public List<string> Voters { get; set; }
        public List<string> VoterNames { get; set; }
        public int? TotalVoter { get; set; }
        public int? TotalCandidate { get; set; }
        public List<string> SelectedCandidates { get; set; }
        public string? RoleUser { get; set; }
    }
}
