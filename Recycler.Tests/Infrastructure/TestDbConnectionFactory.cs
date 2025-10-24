using Npgsql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Recycler.Tests.Infrastructure;

public class TestDbConnectionFactory : IConfiguration
{
    private readonly string _connectionString;

    public TestDbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
        }
    }

        public string? this[string key] { get => null; set => throw new NotImplementedException(); }

    public IEnumerable<IConfigurationSection> GetChildren()
    {
        throw new NotImplementedException();
    }

        public IConfigurationSection GetSection(string key)
        {
            if (key == "ConnectionStrings:DefaultConnection")
            {
                return new TestConfigurationSection(_connectionString);
            }
            return new TestConfigurationSection(string.Empty);
        }

    public string GetConnectionString(string name)
    {
        Console.WriteLine($"GetConnectionString called with name: {name}, returning: {_connectionString}");
        if (string.IsNullOrEmpty(_connectionString))
        {
            throw new InvalidOperationException($"Connection string is null or empty. Name: {name}");
        }
        return _connectionString;
    }

    public IChangeToken GetReloadToken()
    {
        return new TestChangeToken();
    }
}

public class TestConfigurationSection : IConfigurationSection
{
    private readonly string _value;

    public TestConfigurationSection(string? value)
    {
        _value = value ?? string.Empty;
    }

    public string Key => "DefaultConnection";
    public string Path => "ConnectionStrings:DefaultConnection";
    public string? Value { get => _value; set => throw new NotImplementedException(); }
        public string? this[string key] { get => null; set => throw new NotImplementedException(); }

    public IEnumerable<IConfigurationSection> GetChildren()
    {
        throw new NotImplementedException();
    }

    public IConfigurationSection GetSection(string key)
    {
        throw new NotImplementedException();
    }

    public IChangeToken GetReloadToken()
    {
        return new TestChangeToken();
    }
}

public class TestChangeToken : IChangeToken
{
    public bool HasChanged => false;
    public bool ActiveChangeCallbacks => false;
    public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => new TestDisposable();
}

public class TestDisposable : IDisposable
{
    public void Dispose() { }
}
