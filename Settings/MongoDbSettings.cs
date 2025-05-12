namespace SocialMediaAPI.Settings;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string PostsCollection { get; set; } = string.Empty;
    public string UsersCollection { get; set; } = string.Empty;
    public string CommentsCollection { get; set; } = string.Empty;
    public string LikesCollection { get; set; } = string.Empty;
    public string FollowsCollection { get; set; } = string.Empty;
}