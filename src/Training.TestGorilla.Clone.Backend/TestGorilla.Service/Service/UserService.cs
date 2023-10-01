﻿using System.Linq.Expressions;
using TestGorilla.DataAccess.Context;
using TestGorilla.Domain.Entities.Users;
using TestGorilla.Service.Helpers;
using TestGorilla.Service.Interface;

namespace TestGorilla.Service.Service;

public class UserService : IUserService
{
    private readonly IDataContext _appDataContext;
    private readonly ValidationService _validator;

    public UserService(IDataContext dataContext, ValidationService validator)
    {
        _appDataContext = dataContext;
        _validator = validator;
    }

    public async ValueTask<User> CreateAsync(User user, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        if (!_validator.IsValidEmailAddress(user.EmailAddress))
            throw new ArgumentException("invalid email address");

        if (!_validator.IsValidName(user.FirstName) || !_validator.IsValidName(user.LastName))
            throw new ArgumentException("Invalid firstname or lastname");
        ;
        var foundUser = await _appDataContext.Users.FindAsync(user.Id);

        if (foundUser != null)
            throw new InvalidOperationException("User already exists");

        await _appDataContext.Users.AddAsync(user);

        if (saveChanges)
            await _appDataContext.Users.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async ValueTask<User> DeleteAsync(Guid userId, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        var foundUser = await _appDataContext.Users.FindAsync(userId);

        if (foundUser == null)
            throw new InvalidOperationException("User does not exists");

        await _appDataContext.Users.RemoveAsync(foundUser);

        return foundUser;
    }

    public IQueryable<User> Get(Expression<Func<User, bool>> predicate)
    {
        return _appDataContext.Users.Where(predicate.Compile()).AsQueryable();
    }

    public async ValueTask<User> GetByIdAsync(Guid id)
    {
        var foundUser = await _appDataContext.Users.FindAsync(id);

        if (foundUser == null)
            throw new InvalidOperationException("User does not exists");

        return foundUser;
    }

    public async ValueTask<User> UpdateAsync(User user, bool saveChanges = true, CancellationToken cancellationToken = default)
    {
        var foundUser = _appDataContext.Users.FirstOrDefault(user);

        if (foundUser == null)
            throw new InvalidOperationException("User does not exists");

        foundUser.FirstName = user.FirstName;
        foundUser.LastName = user.LastName;
        foundUser.PhoneNumber = user.PhoneNumber;
        foundUser.DateOfBirth = user.DateOfBirth;
        foundUser.EmailAddress = user.EmailAddress;
        foundUser.UpdatedTime = DateTime.UtcNow;

        if (saveChanges)
            await _appDataContext.Users.SaveChangesAsync(cancellationToken);

        return foundUser;
    }

}
