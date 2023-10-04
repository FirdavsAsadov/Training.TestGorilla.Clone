﻿using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TestGorilla.DataAccess.Context;
using TestGorilla.Domain.Entities;
using TestGorilla.Domain.Entities.Questions;
using TestGorilla.Service.Helpers;
using TestGorilla.Service.Interface;

namespace TestGorilla.Service.Service;

public class CheckBoxQuestionService : ICheckboxQuestionService
{
    private readonly IDataContext _appDataContext;
    private readonly ValidationService _validator;

    public CheckBoxQuestionService(IDataContext appDataContext, ValidationService validator)
    {
        _validator = validator;
        _appDataContext = appDataContext;  
    }
    public async Task<CheckBoxQuestion> CreateAsync(CheckBoxQuestion question, CancellationToken cancellationToken , bool saveChanges = true)
    {
        if (isValidCreated(question))
        {
            throw new InvalidOperationException("The question invalid!!");
        }
        if (saveChanges)
        {
            CheckBoxQuestion result = (await _appDataContext.CheckBoxQuestions.AddAsync(question)).Entity;
            await _appDataContext.CheckBoxQuestions.SaveChangesAsync();
        }
        throw new InvalidOperationException("The question is not valid!!");
    }

    public async Task<CheckBoxQuestion> UpdateAsync(CheckBoxQuestion question, CancellationToken cancellationToken, bool saveChanges = true)
    {
        if (isValidUpdated(question))
        {
            throw new InvalidOperationException("The question is not valid upadet!!");
        }

        if (saveChanges)
        {
            var existingQuestion = (await _appDataContext.CheckBoxQuestions.FindAsync(question.Id));
            if (existingQuestion == null)
            {
                throw new InvalidOperationException("The question not found");
            }

            existingQuestion.Title = question.Title;
            existingQuestion.Description = question.Description;
            existingQuestion.Duration = question.Duration;
            existingQuestion.Answers = question.Answers;
            existingQuestion.Category = question.Category;
            existingQuestion.UpdatedTime = DateTime.UtcNow;
        }

        throw new Exception("Xatoo!!!");
    }

    public async Task<bool> DeleteAsync(Guid questionId, CancellationToken cancellationToken, bool saveChanges = true)
    {
        var deletingQuestion = await _appDataContext.CheckBoxQuestions.FindAsync(questionId);
        if (deletingQuestion == null)
        {
            return false;
        }

        _appDataContext.CheckBoxQuestions.RemoveAsync(deletingQuestion);
        if (saveChanges)
        {
            await _appDataContext.CheckBoxQuestions.SaveChangesAsync();
        }
        return true;
    }

    public IQueryable<CheckBoxQuestion> Get(Expression<Func<CheckBoxQuestion, bool>> predicate, CancellationToken cancellationToken, bool saveChanges = true)
    {
        return _appDataContext.CheckBoxQuestions.Where(predicate.Compile()).AsQueryable();
    }

    public async Task<PaginationResult<CheckBoxQuestion>> GetAsync(CheckBoxQuestion question, int PageToken, int PageSize, CancellationToken cancellationToken,
        bool saveChanges = true)
    {
        if (PageToken < 1)
        {
            throw new ArgumentException("PageToken must be greater than or equal to 1");
        }

        if (PageSize < 1)
        {
            throw new ArgumentException("PageSize must be greater than or equal to 1");
        }

        int itemsToSkip = (PageToken -1) * PageSize;
        var query = _appDataContext.CheckBoxQuestions
            .OrderBy(question => question.Id)
            .Skip(itemsToSkip)
            .Take(PageSize).AsQueryable();
        var paginationQuestions = await query.ToListAsync();
        int totalCount =  _appDataContext.CheckBoxQuestions.Count();
        var result = new PaginationResult<CheckBoxQuestion>
        {
            Items = paginationQuestions,
            TotalItems = totalCount
        };
        return result;
    }

    public async Task<CheckBoxQuestion> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool saveChanges = true)
    {
        var existingQuestion = _appDataContext.CheckBoxQuestions.FindAsync(id);
        if (existingQuestion != null)
        {
            return await existingQuestion;
        }

        throw new Exception("This question is not found");
    }

    public async Task<IEnumerable<CheckBoxQuestion>> GetByTitleAsync(string Title, CancellationToken cancellationToken, bool saveChanges = true)
    {
        var question = _appDataContext.CheckBoxQuestions.Where(x => x.Title == Title).AsQueryable();
        if (question != null)
        {
            var questionList = await question.ToListAsync();
            return questionList;
        }
        throw new Exception("This question is not found");
    }

    public async Task<IEnumerable<CheckBoxQuestion>> GetByCategoryAsync(Category category, CancellationToken cancellationToken, bool saveChanges = true)
    {
        var question = _appDataContext.CheckBoxQuestions.Where(x => x.Category == category).AsQueryable();
        if (question != null)
        {
            var questionList = await question.ToListAsync();
            return questionList;
        }

        throw new NotImplementedException("This Category question is not found");
    }

    public bool isValidCreated(CheckBoxQuestion question)
    {
        if (_appDataContext.CheckBoxQuestions.Any(x => x.Title == question.Title))
        {
            return false;
        }
        if(!_validator.IsValidTitle(question.Title))
        {
            return false;
        }

        if (!_validator.IsValidDescription(question.Description))
        {
            return false;
        }

        if (_appDataContext.CheckBoxQuestions.Any(x => x.Duration >= TimeSpan.FromMinutes(90)))
        {
            return false;
        }
        return true;
    }

    public bool isValidUpdated(CheckBoxQuestion question)
    {
        if (!_validator.IsValidTitle(question.Title))
        {
            return false;
        }

        if (!_validator.IsValidDescription(question.Description))
        {
            return false;
        }
        if (_appDataContext.CheckBoxQuestions.Any(x => x.Duration >= TimeSpan.FromMinutes(90)))
        {
            return false;
        }
        return true;
    }
}
