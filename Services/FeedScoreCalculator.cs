public class FeedScoreCalculator
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<FeedScoreCalculator> _logger;
    private const string CacheKeyPrefix = "feed_score_";
    private const int CacheDurationInMinutes = 30;
    private const double TIME_DECAY_FACTOR = 0.1;
    private const double ENGAGEMENT_DECAY_FACTOR = 0.5;

    // Configurable weights
    private const double RECENCY_WEIGHT = 0.4;
    private const double ENGAGEMENT_WEIGHT = 0.3;
    private const double RELATIONSHIP_WEIGHT = 0.2;
    private const double CONTENT_WEIGHT = 0.1;

    public FeedScoreCalculator(ICacheService cacheService, ILogger<FeedScoreCalculator> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<double> CalculateScore(Post post, string viewerId, DateTime now)
    {
        var recency = CalculateRecencyScore(post.CreatedAt, now);
        var engagement = await CalculateEngagementScore(post, viewerId);
        var relationship = await CalculateRelationshipScore(post.UserId, viewerId);
        var content = CalculateContentScore(post);

        return (recency * RECENCY_WEIGHT) +
               (engagement * ENGAGEMENT_WEIGHT) +
               (relationship * RELATIONSHIP_WEIGHT) +
               (content * CONTENT_WEIGHT);
    }

    private double CalculateRecencyScore(DateTime postDate, DateTime now)
    {
        var timeSincePost = (now - postDate).TotalHours;

        return Math.Exp(-TIME_DECAY_FACTOR * timeSincePost);
    }

    public async Task<List<ScoredPost>> RankPosts(List<Post> posts, string viewerId)
    {
        var now = DateTime.UtcNow;

        var scoredPosts = await Task.WhenAll(posts.Select(async post =>
        {
            var score = await CalculateScore(post, viewerId, now);
            
            return new ScoredPost { Post = post, Score = score };
        }));

        return scoredPosts.OrderByDescending(x => x.Score).ToList();
    }


    private async Task<double> CalculateEngagementScore(Post post, string viewerId)
    {
        var cacheKey = $"{CacheKeyPrefix}{post.Id}_{viewerId}";
    
        var cachedScore = await _cacheService.GetAsync<double?>(cacheKey);
        if (cachedScore.HasValue)
        {
            return cachedScore.Value;
        }

        var likes = post.LikesCount ?? 0;
        var comments = post.CommentsCount ?? 0;
        var shares = post.SharesCount ?? 0;

        var age = (DateTime.UtcNow - post.CreatedAt).TotalHours;
        var decayFactor = Math.Exp(-ENGAGEMENT_DECAY_FACTOR * age);

        var weightedEngagement = ((likes * 1) + (comments * 2) + (shares * 3)) * decayFactor;

        await _cacheService.SetAsync(cacheKey, weightedEngagement, TimeSpan.FromMinutes(CacheDurationInMinutes));

        return Math.Log10(weightedEngagement + 1);
    }

    private async Task<double> CalculateRelationshipScore(string authorId, string viewerId)
    {
        try
        {
            var key = $"interaction_{viewerId}_{authorId}";
            var interactions = await _cacheService.GetAsync<int>(key);

            if (interactions == 0)
            {
                interactions = 1;
            }

            return Math.Min(interactions / 10.0, 1.0);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Relationship score error for {viewerId} -> {authorId}: {ex.Message}");
            return 0.1;
        }
    }

    private double CalculateContentScore(Post post)
    {
        double contentScore = 1.0;

        if (!string.IsNullOrEmpty(post.ImageUrl))
            contentScore *= 1.2;
        
        // Boost longer, meaningful content
        if (!string.IsNullOrEmpty(post.Content))
        {
            var wordCount = post.Content.Split(' ').Length;
            if (wordCount > 50)
                contentScore *= 1.1;
        }

        // Boost posts with hashtags
        if (post.Content?.Contains('#') == true)
            contentScore *= 1.1;

        // Boost posts with mentions
        if (post.Content?.Contains('@') == true)
            contentScore *= 1.1;

        return Math.Min(contentScore, 2.0);
    }
}

    public class ScoredPost
    {
        public Post? Post { get; set; }
        public double Score { get; set; }
    }
