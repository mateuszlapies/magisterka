using System.Text.Json.Serialization;

namespace Application.Model
{
    public class Post
    {
        public Guid UserId { get; set; }
        public string Message { get; set; }

        [JsonIgnore]
        public User User { get; set; }
    }
}
