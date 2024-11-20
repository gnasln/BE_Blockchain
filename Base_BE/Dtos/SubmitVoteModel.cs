using System.Numerics;

namespace Base_BE.Dtos;

public class SubmitVoteModel
{
    public string BitcoinAddress { get; set; }
    public List<string> Candidates { get; set; }
    public string VoterId { get; set; }
    public string VoteId { get; set; }
    public DateTime VotedTime { get; set; }
    public string PrivateKey { get; set; }
}