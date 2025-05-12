using DotNetEnv;
using MongoDB.Driver;


public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly string? _connectionString;
    private readonly string? _databaseName;

    public MongoDbContext()
    {
        _connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException("MongoDB connection string is not set.");
        }

        _databaseName = Environment.GetEnvironmentVariable("MONGODB_DATABASE_NAME");
        if (string.IsNullOrWhiteSpace(_databaseName))
        {
            throw new InvalidOperationException("MongoDB database name is not set.");
        }

        var client = new MongoClient(_connectionString);
        _database = client.GetDatabase(_databaseName);
    }

    public IMongoCollection<ApplicationUser> Users => _database.GetCollection<ApplicationUser>("Users");
    public IMongoCollection<Post> Posts => _database.GetCollection<Post>("Posts");
    public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("Comments");
    public IMongoCollection<Like> Likes => _database.GetCollection<Like>("Likes");
    public IMongoCollection<Follow> Follows => _database.GetCollection<Follow>("Follows");
    public IMongoCollection<Share> Shares => _database.GetCollection<Share>("Shares");
}