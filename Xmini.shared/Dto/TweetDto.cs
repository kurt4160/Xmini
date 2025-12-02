namespace Xmini.shared.Dto
{
    public record TweetDto
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
        public bool HasImage { get; set; }
    }
}
