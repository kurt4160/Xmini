namespace Xmini.shared.Dto
{
    public record LikeDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int TweetId { get; set; }
    }
}
