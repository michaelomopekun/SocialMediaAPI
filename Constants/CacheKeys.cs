namespace SocialMediaAPI.Constants
{
    public static class CacheKeys
    {
        public static string ProfileById(string id) => $"Profile:Id:{id}";
        public static string ProfileByUserName(string userName) => $"Profile:UserName:{userName}";
    }
}
