namespace SimplePrimaryKey.Models;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public void Rename(string name)
    {
        Name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeEmail(string email)
    {
        Email = email;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string password)
    {
        Password = password;
        UpdatedAt = DateTime.UtcNow;
    }

    public class Builder
    {
        private readonly User _user;

        public Builder()
        {
            _user = new User();
            _user.Id = Guid.NewGuid();
            _user.CreatedAt = DateTime.UtcNow;
            _user.UpdatedAt = default;
        }

        public Builder WithName(string name)
        {
            _user.Name = name;
            return this;
        }

        public Builder WithEmail(string email)
        {
            _user.Email = email;
            return this;
        }

        public Builder WithPassword(string password)
        {
            _user.Password = password;
            return this;
        }

        public User Build()
        {
            Validate();
            return _user;
        }

        private void Validate()
        {
            if (string.IsNullOrEmpty(_user.Name))
            {
                throw new InvalidOperationException("Name is required");
            }

            if (string.IsNullOrEmpty(_user.Email))
            {
                throw new InvalidOperationException("Email is required");
            }

            if (string.IsNullOrEmpty(_user.Password))
            {
                throw new InvalidOperationException("Password is required");
            }
        }
    }
}
