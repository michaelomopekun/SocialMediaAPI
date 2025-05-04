namespace SocialMediaAPI.Constants
{
    public static class CacheKeys
    {
        public static string ProfileById(string id) => $"Profile:Id:{id}";
        public static string ProfileByUserName(string userName) => $"Profile:UserName:{userName}";
        public static string LikesByPost(string postId) => $"Likes:Post:{postId}";
        public static string LikesByComment(string commentId) => $"Likes:Comment:{commentId}";
        public static string UserLikeStatus(string userId, string postId) => $"Like:User:{userId}:Post:{postId}";
        public static string UserCommentLikeStatus(string userId, string commentId) => $"Like:User:{userId}:Comment:{commentId}";
        public static string PostLikesCount(string postId) => $"LikesCount:Post:{postId}";
        public static string CommentLikesCount(string commentId) => $"LikesCount:Comment:{commentId}";
        public static string PostReactionCounts(string postId) => $"Reactions:Post:{postId}:Counts";
        public static string CommentReactionCounts(string commentId) => $"Reactions:Comment:{commentId}:Counts";
        public static string UserReactionType(string userId, string postId) => $"Reaction:User:{userId}:Post:{postId}:Type";
        public static string UserCommentReactionType(string userId, string commentId) => $"Reaction:User:{userId}:Comment:{commentId}:Type";
    }
}
