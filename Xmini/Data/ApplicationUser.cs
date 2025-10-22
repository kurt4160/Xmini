using Microsoft.AspNetCore.Identity;

namespace Xmini.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string? Location { get; set; }
        public byte[]? ProfilePicture { get; set; }
        public byte[]? BackgroundPicture { get; set; }

        // Navigation property f�r die Beziehung zu Tweets
        public ICollection<Tweet>? Tweets { get; set; }
        // Navigation property f�r die Beziehung zu Likes
        public ICollection<Like>? Likes { get; set; }

        // Navigation properties f�r die Beziehung zu Followers
        public ICollection<Followers>? Followers { get; set; }
        public ICollection<Followers>? Following { get; set; }
    }

}
